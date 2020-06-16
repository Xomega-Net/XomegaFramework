// Copyright (c) 2020 Xomega.Net. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Xomega.Framework
{
    /// <summary>
    /// A base class for properties that contain a piece of data.
    /// The data could be a single value or a list of values based on the property's <c>IsMultiValued</c> flag.
    /// While the member to store the value is untyped, the actual values stored in the property
    /// are always converted to the internal format whenever possible, which would be typed.
    /// Data property also provides support for value conversion, validation and modification tracking.
    /// It can also provide a list of possible values (items) where applicable.
    /// </summary>
    public partial class DataProperty : BaseProperty
    {
        /// <summary>
        ///  Constructs the data property with a given name 
        ///  and adds it to the parent data object under this name.
        /// </summary>
        /// <param name="parent">The parent data object to add the property to if applicable.</param>
        /// <param name="name">The property name that should be unique within the parent data object.</param>
        public DataProperty(DataObject parent, string name)
            : base(parent, name)
        {
            if (parent != null) parent.AddProperty(this);
            Change += ValidateOnStopEditing;
        }

        #region Property value(s) accessors

        /// <summary>
        /// The column index of the property when it is part of a data list object.
        /// </summary>
        internal int Column { get; set; } = -1;

        /// <summary>
        /// The property value as it is stored internally.
        /// </summary>
        public object InternalValue { get; private set; }

        /// <summary>
        /// Gets the value of the current property in the specified format,
        /// using the provided data row as the data source, if applicable.
        /// If no row is specified, then returns the value of the property itself.
        /// </summary>
        /// <param name="format">The format to return the value in.</param>
        /// <param name="row">The data row to use as a source.</param>
        /// <returns></returns>
        public object GetValue(ValueFormat format, DataRow row = null)
        {
            object val = Column < 0 ? InternalValue : row == null || row.List != parent || Column >= row.Count ? null : row[Column];
            return format.IsString() ? ValueToString(val, format) : ResolveValue(val, format);
        }

        /// <summary>
        /// Gets a string value of the current property in the specified string format,
        /// using the provided data row as the data source, if applicable.
        /// If no row is specified, then returns the value of the property itself.
        /// </summary>
        /// <param name="format">The string format to return the value in.</param>
        /// <param name="row">The data row to use as a source.</param>
        /// <returns></returns>
        public string GetStringValue(ValueFormat format, DataRow row = null)
            => GetValue(format, row)?.ToString();

        /// <summary>
        /// Returns the property value in a display string format.
        /// Multiple values are each converted to the display string format
        /// and combined into a delimited string.
        /// </summary>
        /// <seealso cref="ValueFormat.DisplayString"/>
        public string DisplayStringValue => GetStringValue(ValueFormat.DisplayString);

        /// <summary>
        /// Returns the property value in an edit string format.
        /// Multiple values are each converted to the edit string format
        /// and combined into a delimited string.
        /// </summary>
        /// <seealso cref="ValueFormat.EditString"/>
        public string EditStringValue => GetStringValue(ValueFormat.EditString);

        /// <summary>
        /// Returns the property value in a transport format.
        /// Multiple values will be returned as a list of values converted to the transport format.
        /// </summary>
        /// <seealso cref="ValueFormat.Transport"/>
        public object TransportValue => GetValue(ValueFormat.Transport);

        /// <summary>
        /// Sets the value of the property and triggers a property change event.
        /// The value is first converted to the internal format if possible.
        /// If data row is specified, sets the value of this property in that row.
        /// </summary>
        /// <param name="val">The new value to set to the property.</param>
        /// <param name="row">The data row context, if any.</param>
        public void SetValue(object val, DataRow row = null)
        {
            object oldValue = InternalValue;
            object newValue = ResolveValue(val, ValueFormat.Internal);

            if (Column < 0)
                InternalValue = newValue;
            else if (row != null && row.List == parent && Column < row.Count)
                row[Column] = newValue;

            // update Modified flag, make sure to not set it back from true to false
            if (!Modified.HasValue) Modified = false;
            else if (!Equals(oldValue, newValue)) Modified = true;
            ResetValidation();
            FirePropertyChange(new PropertyChangeEventArgs(PropertyChange.Value, oldValue, newValue, row));
        }

        /// <summary>
        /// Asyncronously sets the value of the property and triggers a property change event.
        /// The value is first converted to the internal format if possible.
        /// If data row is specified, sets the value of this property in that row.
        /// </summary>
        /// <param name="val">The new value to set to the property.</param>
        /// <param name="row">The data row context, if any.</param>
        /// <param name="token">Cancellation token.</param>
        public async Task SetValueAsync(object val, DataRow row = null, CancellationToken token = default)
        {
            object oldValue = InternalValue;
            object newValue = await ResolveValueAsync(val, ValueFormat.Internal);

            if (Column < 0)
                InternalValue = newValue;
            else if (row != null && row.List == parent && Column < row.Count)
                row[Column] = newValue;

            // update Modified flag, make sure to not set it back from true to false
            if (!Modified.HasValue) Modified = false;
            else if (!Equals(oldValue, newValue)) Modified = true;
            ResetValidation();
            await FirePropertyChangeAsync(new PropertyChangeEventArgs(PropertyChange.Value, oldValue, newValue, row), token);
        }

        /// <summary>
        /// Checks if the current property value is null.
        /// </summary>
        /// <returns>True if the current property value is null, otherwise false.</returns>
        public bool IsNull(DataRow row = null)
            => IsValueNull(GetValue(ValueFormat.Internal, row), ValueFormat.Internal);

        /// <summary>
        /// Resets property value and modified state to default values
        /// </summary>
        public virtual void ResetValue()
        {
            SetValue(null);
            Modified = null;
        }

        /// <summary>
        /// Tracks the modification state of the property. Null means the property value has never been set.
        /// False means the value has been set only once (initialized).
        /// True means that the value has been modified since it was initialized.
        /// </summary>
        public bool? Modified { get; set; }

        #endregion

        #region Value configuration

        /// <summary>
        /// Gets or sets whether the property contains multiple values (a list) or a single value.
        /// </summary>
        public bool IsMultiValued { get; set; }

        /// <summary>
        /// The string to display when the property value is null.
        /// Setting such string as a value will be considered as setting the value to null.
        /// The default is empty string.
        /// </summary>
        public string NullString { get; set; } = "";

        /// <summary>
        /// The string to display when the property value is restricted and not allowed to be viewed (e.g. N/A).
        /// The default is empty string.
        /// </summary>
        public string RestrictedString { get; set; } = "";

        /// <summary>
        /// The separators to use for multivalued properties to parse the list of values from the input string.
        /// The default is comma, semicolon and a new line.
        /// </summary>
        public string[] ParseListSeparators { get; set; } = new string[] { ",", ";", "\n" };

        /// <summary>
        /// The separator to use for multivalued properties to combine the list of values into a display string.
        /// The default is comma with a space.
        /// </summary>
        public string DisplayListSeparator { get; set; } = ", ";

        /// <summary>
        /// The maximum length for each property value when the value is of type string.
        /// The default is -1, which means there is no maximum length.
        /// </summary>
        public int Size { get; set; } = -1;


        /// <summary>
        /// A delegate to provide a list of possible values given the user input so far.
        /// </summary>
        /// <param name="input">The user input so far.</param>
        /// <returns>A list of possible values.</returns>
        /// <param name="row">The data row context, if any.</param>
        public delegate IEnumerable GetValueList(object input, DataRow row);

        /// <summary>
        /// A function to provide a list of possible values for the property where applicable.
        /// </summary>
        public GetValueList ItemsProvider;

        /// <summary>
        /// A function to asyncronously provide a list of possible values for the property
        /// based on the specified input and current row, where applicable.
        /// </summary>
        public Func<object, DataRow, CancellationToken, Task<IEnumerable>> AsyncItemsProvider;

        #endregion

        #region Value conversion

        /// <summary>
        /// Converts a list of values to a string for the given format.
        /// Default implementation uses the DisplayListSeparator to concatenate the values for any format.
        /// Subclasses can override this behavior to differentiate between the <c>DisplayString</c> format
        /// and the <c>EditString</c> format and can also provide custom delimiting, e.g. comma-separated
        /// and a new line between every five values to get five comma-separated values per line.
        /// </summary>
        /// <param name="list">The list of values to convert to string.</param>
        /// <param name="format">The string format for which the conversion is required.</param>
        /// <returns>The string representation of the given list.</returns>
        public virtual string ListToString(IList list, ValueFormat format)
        {
            string res = "";
            foreach (object val in list)
                res += (res == "" ? "" : DisplayListSeparator) + Convert.ToString(val);
            return res;
        }

        /// <summary>
        /// Converts the specified value to the given string format as per the property conversion rules.
        /// Multiple values are each converted to the edit string format and combined into a delimited string.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="format">The string format to use.</param>
        /// <returns>A string that represents the given value(s) in the given format.</returns>
        public string ValueToString(object value, ValueFormat format)
        {
            object cVal = ResolveValue(value, format);
            return cVal is IList lst ? ListToString(lst, format) : Convert.ToString(cVal);
        }

        /// <summary>
        /// A function to determine if the given value is considered to be null for the given format.
        /// Default implementation returns true if the value is null, is an empty list,
        /// is a string with blank spaces only or is equal to the NullString for any format.
        /// Subclasses can override this function to differentiate by the value format
        /// or to provide different or additional rules.
        /// </summary>
        /// <param name="value">The value to check for null.</param>
        /// <param name="format">The value format, for which the null check is performed.</param>
        /// <returns>True if the value is considered to be null for the given format, otherwise false.</returns>
        public virtual bool IsValueNull(object value, ValueFormat format)
        {
            if (value == null) return true;
            if (value is IList lst && lst.Count == 0) return true;
            if (value is string)
            {
                string str = ((string)value).Trim();
                return string.IsNullOrEmpty(str) || str == NullString;
            }
            return false; // non-null non-string
        }

        /// <summary>
        /// Resolves the given value or a list of values to the specified format based on the current property configuration.
        /// If the property is restricted or the value is null and the format is string based,
        /// the <c> RestrictedString</c> or <c>NullString</c> are returned respectively.
        /// If the property is multivalued it will try to convert the value to a list or parse it into a list if it's a string
        /// or just add it to a new list as is and then convert each value in the list into the given format.
        /// Otherwise it will try to convert the single value to the given format.
        /// If a custom value converter is set on the property, it will be used first before the default property conversion rules are applied.
        /// </summary>
        /// <param name="value">The value or list of values to resolve to the given format.</param>
        /// <param name="format">The format to convert the value to.</param>
        /// <returns>A value or a list of values resolved to the given format based on the property configuration.</returns>
        public object ResolveValue(object value, ValueFormat format)
        {
            if (IsRestricted())
                return format.IsString() ? RestrictedString : value;

            if (IsValueNull(value, format))
                return format.IsString() ? NullString : null;

            if (IsMultiValued)
            {
                IList lst = new List<object>();
                if (value is IList) lst = (IList)value;
                else if (value is string)
                {
                    string[] vals = ((string)value).Split(
                        ParseListSeparators, StringSplitOptions.None);
                    foreach (string val in vals)
                        if (!IsValueNull(val, format)) lst.Add(val.Trim());
                }
                else lst.Add(value);
                IList resLst = CreateList(format);
                foreach (object val in lst)
                {
                    object cval = val;
                    if (ValueConverter == null || !ValueConverter(ref cval, format))
                        cval = ConvertValue(cval, format);
                    if (!IsValueNull(cval, format)) resLst.Add(cval);
                }
                return resLst;
            }
            else
            {
                object cval = value;
                if (ValueConverter == null || !ValueConverter(ref cval, format))
                    cval = ConvertValue(cval, format);
                return cval;
            }
        }

        /// <summary>
        /// Asynchronously resolves the given value or a list of values to the specified format
        /// based on the current property configuration. If the property is restricted or the value is null
        /// and the format is string based, the <c> RestrictedString</c> or <c>NullString</c> are returned respectively.
        /// If the property is multivalued it will try to convert the value to a list or parse it into a list if it's a string
        /// or just add it to a new list as is and then convert each value in the list into the given format.
        /// Otherwise it will try to convert the single value to the given format.
        /// If a custom value converter is set on the property, it will be used first before the default property conversion rules are applied.
        /// </summary>
        /// <param name="value">The value or list of values to resolve to the given format.</param>
        /// <param name="format">The format to convert the value to.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>A value or a list of values resolved to the given format based on the property configuration.</returns>
        public async Task<object> ResolveValueAsync(object value, ValueFormat format, CancellationToken token = default)
        {
            if (IsRestricted())
                return format.IsString() ? RestrictedString : value;

            if (IsValueNull(value, format))
                return format.IsString() ? NullString : null;

            if (IsMultiValued)
            {
                IList lst = new List<object>();
                if (value is IList list) lst = list;
                else if (value is string str)
                {
                    string[] vals = str.Split(
                        ParseListSeparators, StringSplitOptions.None);
                    foreach (string val in vals)
                        if (!IsValueNull(val, format)) lst.Add(val.Trim());
                }
                else lst.Add(value);
                IList resLst = CreateList(format);
                foreach (object val in lst)
                {
                    object cval = val;
                    if (AsyncValueConverter == null || !await AsyncValueConverter(ref cval, format, token))
                        cval = await ConvertValueAsync(cval, format, token);
                    if (!IsValueNull(cval, format)) resLst.Add(cval);
                }
                return resLst;
            }
            else
            {
                object cval = value;
                if (AsyncValueConverter == null || !await AsyncValueConverter(ref cval, format, token))
                    cval = await ConvertValueAsync(cval, format, token);
                return cval;
            }
        }

        /// <summary>
        /// Creates a new list for the given format. The default implementation just returns a new untyped ArrayList.
        /// Subclasses can override it to return typed generic lists for the Transport format.
        /// </summary>
        /// <param name="format">The format to create a new list for.</param>
        /// <returns>A new list for the given format.</returns>
        protected virtual IList CreateList(ValueFormat format) => new List<object>();

        /// <summary>
        /// A delegate to support custom conversion functions. It will try to convert the value
        /// that is passed by reference to the specified format by setting the reference to the 
        /// converted value. It will return whether or not the conversion succeeded, which determines
        /// if further conversion rules need to be applied.
        /// If the delegate is only able to convert values to just one format, it should return false for other formats.
        /// </summary>
        /// <param name="value">A reference to the value to be converted.</param>
        /// <param name="format">The format to convert the value to.</param>
        /// <returns>True if the conversion succeeded, otherwise false.</returns>
        public delegate bool TryConvertValue(ref object value, ValueFormat format);

        /// <summary>
        ///  A custom value converter that can be set on the property for converting values to a given format.
        /// </summary>
        public TryConvertValue ValueConverter;

        /// <summary>
        /// A delegate to support custom async conversion functions. It will try to convert the value
        /// that is passed by reference to the specified format by setting the reference to the 
        /// converted value. It will return whether or not the conversion succeeded, which determines
        /// if further conversion rules need to be applied.
        /// If the delegate is only able to convert values to just one format, it should return false for other formats.
        /// </summary>
        /// <param name="value">A reference to the value to be converted.</param>
        /// <param name="format">The format to convert the value to.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>True if the conversion succeeded, otherwise false.</returns>
        public delegate Task<bool> TryConvertValueAsync(ref object value, ValueFormat format, CancellationToken token = default);

        /// <summary>
        ///  A custom value converter that can be set on the property for asynchronously converting values to a given format.
        /// </summary>
        public TryConvertValueAsync AsyncValueConverter;

        /// <summary>
        /// Converts a single value to a given format. The default implementation does nothing to the value,
        /// but subclasses can implement the property specific rules for each format.
        /// </summary>
        /// <param name="value">A single value to convert to the given format.</param>
        /// <param name="format">The value format to convert the value to.</param>
        /// <returns>The value converted to the given format.</returns>
        protected virtual object ConvertValue(object value, ValueFormat format) => value;

        /// <summary>
        /// Asynchronously converts a single value to a given format.
        /// The default implementation delegates it to the synchronous version,
        /// but subclasses can implement the property specific rules for each format.
        /// </summary>
        /// <param name="value">A single value to convert to the given format.</param>
        /// <param name="format">The value format to convert the value to.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>The value converted to the given format.</returns>
        protected virtual async Task<object> ConvertValueAsync(object value, ValueFormat format, CancellationToken token = default) => 
            await Task.FromResult(ConvertValue(value, format));

        #endregion

        #region Validation

        /// <summary>
        /// A callback that validates the property when it stops being edited.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The callback arguments.</param>
        protected void ValidateOnStopEditing(object sender, PropertyChangeEventArgs args)
        {
            if (this == sender && args.Change.IncludesEditing() && !Editing) Validate();
        }

        /// <summary>
        /// Returns the list of validation errors for the property.
        /// Null means that the validation has not been performed since the property value last changed.
        /// An empty list means that the validation has been performed and the value is valid.
        /// Non-empty list means that the value has been validated and is not valid if the list contains any errors.
        /// </summary>
        public ErrorList ValidationErrors { get; private set; }

        /// <summary>
        /// Gets a combined error text by concatenating all validation error messages with a new line delimiter.
        /// </summary>
        public string ErrorsText => Editable && Visible ? ValidationErrors?.ErrorsText : "";

        /// <summary>
        /// Returns if the current property value has been validated and is valid, i.e. has no validation errors.
        /// </summary>
        /// <param name="validate">True to validate the property first if needed.</param>
        /// <returns>True if the current property has been validated and is valid, otherwise false.</returns>
        public bool IsValid(bool validate)
        {
            if (validate) Validate();
            return ValidationErrors != null && !ValidationErrors.HasErrors();
        }

        /// <summary>
        /// Resets the validation status of the property to be non-validated by setting the list of validation errors to null.
        /// Fires the validation property change event as well. The validation status is reset automatically
        /// whenever the property value changes and can also be reset manually if the validation depends on external factors that have changed.
        /// </summary>
        public void ResetValidation()
        {
            ValidationErrors = null;
            FirePropertyChange(new PropertyChangeEventArgs(PropertyChange.Validation, null, null, null));
        }

        /// <summary>
        /// Validates the property if it hasn't been validated yet.
        /// </summary>
        public void Validate() { Validate(false); }

        /// <summary>
        /// Validates the property and fires a validation property change event.
        /// </summary>
        /// <param name="force">True to validate regardless of whether or not it has been already validated.</param>
        public virtual void Validate(bool force)
        {
            if (force) ResetValidation();
            if (ValidationErrors != null) return;

            ValidationErrors = parent?.NewErrorList() ?? new ErrorList();

            if (Validator != null && Editable && Visible)
            {
                if (InternalValue is IList lst && lst.Count > 0)
                    foreach (object val in lst) Validator(this, val);
                else Validator(this, InternalValue);
            }
            FirePropertyChange(new PropertyChangeEventArgs(PropertyChange.Validation, null, null, null));
        }

        /// <summary>
        /// A delegate to support custom validation functions.
        /// The delegate is multicast to allow combining multiple validation functions.
        /// </summary>
        /// <param name="prop">The data property being validated. The function can use
        /// the property configuration for validation or error messages. Result of the validation
        /// should be added to the property's validation error list.</param>
        /// <param name="value">The value to validate.</param>
        public delegate void ValueValidator(DataProperty prop, object value);

        /// <summary>
        /// A list of property validation functions. Validation functions can be added to 
        /// or removed from this list by using the standard + and - operators respectively.
        /// </summary>
        public ValueValidator Validator = ValidateRequired;

        /// <summary>
        /// A standard validation function that checks for null if the value is required.
        /// </summary>
        /// <param name="dp">Data property being validated.</param>
        /// <param name="value">The value to validate.</param>
        public static void ValidateRequired(DataProperty dp, object value)
        {
            if (dp != null && dp.Required && dp.IsValueNull(value, ValueFormat.Internal))
                dp.ValidationErrors.AddValidationError(Messages.Validation_Required, dp);
        }

        #endregion

        /// <summary>
        /// Copy value and state from another property (presumably of the same type).
        /// </summary>
        /// <param name="p">The property to copy the state from.</param>
        public virtual void CopyFrom(DataProperty p)
        {
            if (p == null) return;
            SetValue(p.InternalValue);
            Editable = p.Editable;
            Required = p.Required;
            AccessLevel = p.AccessLevel;
            Visible = p.Visible;
        }

        /// <summary>
        /// Space-delimited string with the property states.
        /// It can be used as styles or CSS classes on property-bound controls.
        /// </summary>
        /// <param name="states">The combination of property states to return.</param>
        public virtual string GetStateString(PropertyChange states)
        {
            var state = new HashSet<string>();
            if (Visible)
            {
                if (Editable)
                {
                    if (Required && states.IncludesRequired())
                        state.Add("required");
                    if (Modified == true && states.IncludesValue())
                        state.Add("modified");
                    if (ValidationErrors != null && states.IncludesValidation())
                        state.Add(IsValid(false) ? "valid" : "invalid");
                }
                else if (states.IncludesEditable())
                    state.Add("readonly");
            }
            else if (states.IncludesVisible())
                state.Add("hidden");

            return string.Join(" ", state);
        }
    }
}
