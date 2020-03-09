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
            get
            {
                object value = InternalValue;
                if (value is T) return (T)value;
                else
                {
                    IList lst = value as IList;
                    if (lst != null && lst.Count > 0 && lst[0] is T)
                        return (T)lst[0];
                    else return default(T);
                }
            }
            set
            {
                if (IsMultiValued)
                {
                    List<T> lst = new List<T>();
                    lst.Add(value);
                    SetValue(lst);
                }
                else SetValue(value);
            }
        }

        /// <summary>
        /// Gets or sets a typed list of values for multivalued properties.
        /// </summary>
        public virtual List<T> Values
        {
            get
            {
                object value = InternalValue;
                if (value is List<T>) return (List<T>)value;
                else if (value is T)
                {
                    List<T> lst = new List<T>();
                    lst.Add((T)value);
                    return lst;
                }
                else
                {
                    IList lst = value as IList;
                    if (lst == null) return null;
                    
                    List<T> res = new List<T>();
                    foreach (object obj in lst)
                    {
                        if (obj is T) res.Add((T)obj);
                        else return null;
                    }
                    return res;
                }
            }
            set
            {
                if (IsMultiValued) SetValue(value);
                else if (value != null && value.Count > 0) SetValue(value[0]);
                else SetValue(null);
            }
        }
    }
}
