// Copyright (c) 2010-2013 Xomega.Net. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Xomega.Framework.Properties
{
    /// <summary>
    /// A combo property allows displaying a read-only combination of the values
    /// from several other properties that is formatted according to the specified format string.
    /// Being a <see cref="DataProperty"/> itself it can be bound to labels and other read-only text controls,
    /// and it will automatically reflect any changes in the underlying component properties.
    /// For example, you can display an address composed of several individual components
    /// like city, state and zipcode all in one field properly formatted.
    /// If no address fields are set though, it will show a blank string or your choice of a null value
    /// instead of a bunch of commas that you would get from the String.Format.
    /// You can also control this behavior and hide unwanted formatting if your address is partially blank.
    /// </summary>
    /// <seealso cref="Format"/>
    public class ComboProperty : DataProperty
    {
        /// <summary>
        /// Instrumentation hook.
        /// </summary>
        static ComboProperty() { }

        /// <summary>
        /// The format string with placeholders for the component values
        /// that is used to build the combo value. The placeholders are numbers in curly braces,
        /// e.g. {0}, where the number corresponds to the index of each component property
        /// as set in the <see cref="SetComponentProperties"/> method.
        /// If any adjacent part of the static format should not be shown when the value is blank,
        /// then these parts should be placed inside the curly braces
        /// and the number should be marked with a preceding $.
        /// For example, the placeholder { ($1)} will show the value of the second property in parenthesis,
        /// but won't show anything if that value is blank as opposed to showing empty parenthesis.
        /// </summary>
        public string Format { get; set; }

        /// <summary>
        /// A flag indicating whether or not to trim component values
        /// when building the combo value.  The default is true.
        /// </summary>
        public bool TrimValues { get; set; }

        /// <summary>
        ///  Constructs a ComboProperty with a given name 
        ///  and adds it to the parent data object under this name.
        /// </summary>
        /// <param name="parent">The parent data object to add the property to if applicable.</param>
        /// <param name="name">The property name that should be unique within the parent data object.</param>
        public ComboProperty(DataObject parent, string name)
            : base(parent, name)
        {
            IsMultiValued = true;
            TrimValues = true;
        }

        /// <summary>
        /// Sets the component properties that this combo property is based on.
        /// Also subscribes to listen for changes in those properties
        /// in order to retransmit the event and get the combo value refreshed.
        /// </summary>
        /// <param name="properties">An array of component properties for the current combo property.</param>
        public void SetComponentProperties(params DataProperty[] properties)
        {
            DataProperty[] props = InternalValue as DataProperty[];
            if (props != null)
                foreach (DataProperty p in props) p.Change -= ComponentPropertyChange;
            SetValue(properties);
            Modified = false;
            if (properties != null)
                foreach (DataProperty p in properties) p.Change += ComponentPropertyChange;
        }

        /// <summary>
        /// Retransmits property change events from any of the component properties
        /// as its own property change event, so that the combined value would be refreshed.
        /// </summary>
        /// <param name="property">The component property that was changed.</param>
        /// <param name="eventArgs">Event arguments that describe the property change.</param>
        private void ComponentPropertyChange(object property, PropertyChangeEventArgs eventArgs)
        {
            FirePropertyChange(eventArgs);
        }

        /// <summary>
        /// Overrides the base method to format a list of values according to the specified format string.
        /// </summary>
        /// <param name="list">The list of values to convert to string.</param>
        /// <param name="format">The string format for which the conversion is required.</param>
        /// <returns>The string representation of the given list.</returns>
        public override string ListToString(IList list, ValueFormat format)
        {
            string res = Format;
            for (int i = 0; i < list.Count; i++)
            {
                string val = Convert.ToString(list[i]);
                if (TrimValues) val = val.Trim();
                res = Regex.Replace(res, @"(\{" + i + @"\}|\{(?<pre>.*?)\$" + i + @"(?<post>.*?)\})",
                    string.IsNullOrEmpty(val) ? "" : "${pre}" + val + "${post}");
            }
            return res;
        }

        /// <summary>
        /// Overrides the base method to return null if all properties are null for a string format.
        /// If the values are not properties then it returns false, so that they would still be added
        /// to the list that is passed to the <see cref="ListToString"/> method.
        /// </summary>
        /// <param name="value">The value to check for null.</param>
        /// <param name="format">The value format, for which the null check is performed.</param>
        /// <returns>True if the value is considered to be null for the given format, otherwise false.</returns>
        public override bool IsValueNull(object value, ValueFormat format)
        {
            if (format.IsString())
            {
                List<object> props = value as List<object>;
                if (props != null && props.All(p => p is DataProperty))
                    return props.Cast<DataProperty>().All(p => p.IsNull());
            }
            return false;
        }

        /// <summary>
        /// Overrides the base method if the value passed is a data property
        /// to return the value of that property in the specified format.
        /// </summary>
        /// <param name="value">A single value to convert to the given format.</param>
        /// <param name="format">The value format to convert the value to.</param>
        /// <returns>The value converted to the given format.</returns>
        protected override object ConvertValue(object value, ValueFormat format)
        {
            DataProperty p = value as DataProperty;
            if (p == null) return value;

            if (format == ValueFormat.DisplayString) return p.DisplayStringValue;
            if (format == ValueFormat.EditString) return p.EditStringValue;
            if (format == ValueFormat.Transport) return p.TransportValue;

            return p;
        }
    }
}
