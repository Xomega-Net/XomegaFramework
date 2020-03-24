// Copyright (c) 2020 Xomega.Net. All rights reserved.

using System.Collections;
using System.Collections.Generic;

namespace Xomega.Framework
{
    /// <summary>
    /// A generic typed data property that provides typed access
    /// to the underlying value or list of values as appropriate.
    /// </summary>
    /// <typeparam name="T">The type of the underlying property value.</typeparam>
    public class DataProperty<T> : DataProperty
    {
        /// <summary>
        /// Constructs a generic typed data property with a given name
        /// and adds it to the parent data object under this name.
        /// </summary>
        /// <param name="parent">The parent data object to add the property to if applicable.</param>
        /// <param name="name">The property name that should be unique within the parent data object.</param>
        public DataProperty(DataObject parent, string name)
            : base(parent, name)
        {
        }

        /// <summary>
        /// Creates a new list for the given format.
        /// Overrides the default behavior to return a typed generic list for the Transport format.
        /// </summary>
        /// <param name="format">The format to create a new list for.</param>
        /// <returns>A new list for the given format.</returns>
        protected override IList CreateList(ValueFormat format)
        {
            // we don't have to store values in a typed list if the format is internal,
            // since some values may be still invalid. However, the values should be validated
            // before they are transfered over the network, so the transport format requires a typed list.
            return format == ValueFormat.Transport ? new List<T>() : base.CreateList(format);
        }

        /// <summary>
        /// Gets or sets a single typed value for properties that are not multivalued.
        /// </summary>
        public virtual T Value
        {
            get => GetValue();
            set => SetValue(value);
        }

        /// <summary>
        /// Gets a single typed value for properties that are not multivalued.
        /// </summary>
        /// <param name="row">The data row context, if any.</param>
        /// <returns>A typed value for the specified row or the current property.</returns>
        public virtual T GetValue(DataRow row = null)
        {
            object value = GetValue(ValueFormat.Internal, row);
            if (value is T) return (T)value;
            else
            {
                if (value is IList lst && lst.Count > 0 && lst[0] is T)
                    return (T)lst[0];
                else return default;
            }
        }

        /// <summary>
        /// Gets or sets a typed list of values for multivalued properties.
        /// </summary>
        public List<T> Values
        {
            get => GetValues();
            set => SetValues(value);
        }

        /// <summary>
        /// Gets a typed list of values from the property or the specified row for multivalued properties.
        /// </summary>
        /// <param name="row">The data row context, if any.</param>
        /// <returns>A typed list of values for the specified row or the current property.</returns>
        public virtual List<T> GetValues(DataRow row = null)
        {
            object value = GetValue(ValueFormat.Internal, row);
            if (value is List<T>) return (List<T>)value;
            else if (value is T)
                return new List<T> { (T)value };
            else
            {
                if (!(value is IList lst)) return null;

                List<T> res = new List<T>();
                foreach (object obj in lst)
                {
                    if (obj is T) res.Add((T)obj);
                    else return null;
                }
                return res;
            }
        }

        /// <summary>
        /// Sets a list of typed values on the specified data row or the property itself.
        /// </summary>
        /// <param name="value">The typed value to set.</param>
        /// <param name="row">The data row context, if any.</param>
        public virtual void SetValues(List<T> value, DataRow row = null)
        {
            if (IsMultiValued) SetValue(value, row);
            else if (value != null && value.Count > 0) SetValue(value[0], row);
            else SetValue(null, row);
        }
    }
}
