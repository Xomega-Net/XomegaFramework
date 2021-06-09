// Copyright (c) 2021 Xomega.Net. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Xomega.Framework.Lookup;

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
            object val = Column < 0 ? InternalValue : row == null || row.List != parent ? null : row[Column];
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
            object oldValue = GetValue(ValueFormat.Internal, row);
            object newValue = ResolveValue(val, ValueFormat.Internal);

            if (Column < 0)
                InternalValue = newValue;
            else if (row != null && row.List == parent)
                row[Column] = newValue;

            bool sameValue = ValuesEqual(oldValue, newValue);
            // update Modified flag, make sure to not set it back from true to false
            if (GetModified(row) == null) SetModified(false, row);
            else if (!sameValue) SetModified(true, row);
            if (!sameValue) ResetValidation(row);
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
            object oldValue = GetValue(ValueFormat.Internal, row);
            object newValue = await ResolveValueAsync(val, ValueFormat.Internal, row);

            if (Column < 0)
                InternalValue = newValue;
            else if (row != null && row.List == parent)
                row[Column] = newValue;

            bool sameValue = ValuesEqual(oldValue, newValue);
            // update Modified flag, make sure to not set it back from true to false
            if (GetModified(row) == null) SetModified(false, row);
            else if (!sameValue) SetModified(true, row);
            if (!sameValue) ResetValidation(row);
            await FirePropertyChangeAsync(new PropertyChangeEventArgs(PropertyChange.Value, oldValue, newValue, row), token);
        }

        /// <summary>
        /// Compares to values for equality, using SequenceEqual for comparing lists of values.
        /// </summary>
        /// <param name="val1">The first value to compare.</param>
        /// <param name="val2">The second value to compare.</param>
        /// <returns>True, if values are equal, false otherwise.</returns>
        protected virtual bool ValuesEqual(object val1, object val2)
        {
            if (val1 is IList lst1 && val2 is IList lst2)
                return lst1.Cast<object>().SequenceEqual(lst2.Cast<object>());
            else return Equals(val1, val2);
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

        #endregion

        #region Modification support

        private bool? modified;

        /// <summary>
        /// Tracks the modification state of the property. Null means the property value has never been set.
        /// False means the value has been set only once (initialized).
        /// True means that the value has been modified since it was initialized.
        /// </summary>
        public bool? Modified { get => GetModified(null); set => SetModified(value, null); }

        /// <summary>
        /// Gets modification state of the current property for the specified row.
        /// If the row is null or this property was not set in the row,
        /// then it returns the modified flag for this property.
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        public bool? GetModified(DataRow row) => row?.GetModified(this) ?? modified;

        /// <summary>
        /// Sets the modification state of the current property for the specified row
        /// or for the entire property, if the row is null.
        /// </summary>
        /// <param name="value">The modification state to set.</param>
        /// <param name="row">The row, for which to set the value, or null to set it for the property.</param>
        public void SetModified(bool? value, DataRow row)
        {
            if (row != null) row.SetModified(this, value);
            else modified = value;
        }

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

        #region Computed value

        private ComputedBinding valueBinding;

        /// <summary>
        /// Sets the expression to use for computing property value.
        /// Makes the computed property non-editable and not required.
        /// </summary>
        /// <param name="expression">Lambda expression used to compute the value,
        /// or null to make the value non-computed.</param>
        /// <param name="args">Arguments for the specified expression to use for evaluation.</param>
        public void SetComputedValue(LambdaExpression expression, params object[] args)
        {
            if (valueBinding != null) valueBinding.Dispose();
            if (expression != null)
            {
                valueBinding = new ComputedValueBinding(this, expression, args);
                Editable = false;
                Required = false;
            }
            else valueBinding = null;
        }

        /// <summary>
        /// Manually updates the property value with the computed result,
        /// in addition to automatic updates when the underlying properties change.
        /// </summary>
        /// <param name="row">The row in a data list to update, or null if the property is not in a data list.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>The task for the asynchronous operation.</returns>
        public async Task UpdateComputedValueAsync(DataRow row = null, CancellationToken token = default)
        {
            if (valueBinding != null)
                await valueBinding.UpdateAsync(row, token);
        }

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
            if (value is string vstr)
            {
                string str = vstr.Trim();
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
                return format == ValueFormat.DisplayString ? NullString : null;

            if (IsMultiValued)
            {
                IList lst = new List<object>();
                if (value is IList vlist) lst = vlist;
                else if (value is string vstr)
                {
                    string[] vals = vstr.Split(
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
        /// <param name="row">The row in a list object or null for regular data objects.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>A value or a list of values resolved to the given format based on the property configuration.</returns>
        public async Task<object> ResolveValueAsync(object value, ValueFormat format, DataRow row, CancellationToken token = default)
        {
            if (IsRestricted())
                return format.IsString() ? RestrictedString : value;

            if (IsValueNull(value, format))
                return format == ValueFormat.DisplayString ? NullString : null;

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
                        cval = await ConvertValueAsync(cval, format, row, token);
                    if (!IsValueNull(cval, format)) resLst.Add(cval);
                }
                return resLst;
            }
            else
            {
                object cval = value;
                if (AsyncValueConverter == null || !await AsyncValueConverter(ref cval, format, token))
                    cval = await ConvertValueAsync(cval, format, row, token);
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
        /// <param name="row">The row in a list object or null for regular data objects.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>The value converted to the given format.</returns>
        protected virtual async Task<object> ConvertValueAsync(object value, ValueFormat format, DataRow row, CancellationToken token = default) =>
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
            if (this == sender && args.Change.IncludesEditing() && !GetEditing(args.Row)) Validate(args.Row);
        }

        /// <summary>
        /// A list of validation errors for the property.
        /// </summary>
        protected ErrorList validationErrors;

        /// <summary>
        /// Gets validation errors for the value of the property or a specific data row.
        /// Null means that the validation has not been performed since the property value last changed.
        /// An empty list means that the validation has been performed and the value is valid.
        /// Non-empty list means that the value has been validated and is not valid if the list contains any errors.
        /// </summary>
        /// <param name="row">The data row to get validation errors for, or null to get it for the property value.</param>
        public ErrorList GetValidationErrors(DataRow row)
        {
            if (row != null) return row.GetValidationErrors(this);
            else return validationErrors;
        }

        /// <summary>
        /// Adds the specified validation error with arguments to the property or data row, if one is specified.
        /// </summary>
        /// <param name="row">The data row to add validation error to, or null to add to the property directly.</param>
        /// <param name="error">The error code.</param>
        /// <param name="args">Error message parameters, if any.</param>
        public void AddValidationError(DataRow row, string error, params object[] args)
        {
            if (row != null) row.AddValidationError(this, error, args);
            else validationErrors.AddValidationError(error, args);
        }

        /// <summary>
        /// Returns if the current property value in the specified row, if provided, has been validated and is valid,
        /// i.e. has no validation errors.
        /// </summary>
        /// <param name="validate">True to validate the property first if needed.</param>
        /// <param name="row">The row for the value, or null for the property value.</param>
        /// <returns>True if the current property has been validated and is valid, otherwise false.</returns>
        public bool IsValid(bool validate, DataRow row)
        {
            if (validate) Validate(row);
            var errors = GetValidationErrors(row);
            return errors != null && !errors.HasErrors();
        }

        /// <summary>
        /// Resets the validation status to be non-validated for the current property or the specified row.
        /// Fires the validation property change event as well. The validation status is reset automatically
        /// whenever the property value changes and can also be reset manually if the validation depends on external factors that have changed.
        /// </summary>
        /// <param name="row">The row to reset validation for, or null to reset for the current property.</param>
        public void ResetValidation(DataRow row)
        {
            if (row != null) row.ResetValidation(this);
            else validationErrors = null;
            FirePropertyChange(new PropertyChangeEventArgs(PropertyChange.Validation, null, null, row));
        }

        /// <summary>
        /// Validates the property if it hasn't been validated yet.
        /// </summary>
        /// <param name="row">The row to validate, or null to validate the property value</param>
        public void Validate(DataRow row) { Validate(false, row); }

        /// <summary>
        /// Validates the property and fires a validation property change event.
        /// </summary>
        /// <param name="force">True to validate regardless of whether or not it has been already validated.</param>
        /// <param name="row">The row to validate, or null to validate the property value</param>
        public virtual void Validate(bool force, DataRow row)
        {
            if (force) ResetValidation(row);

            var errors = GetValidationErrors(row);
            if (errors != null)
            {
                FirePropertyChange(new PropertyChangeEventArgs(PropertyChange.Validation, null, null, row));
                return;
            }

            if (row != null) row.AddValidationError(this, null);
            else validationErrors = parent?.NewErrorList() ?? new ErrorList();

            if (Validator != null && Editable && Visible)
            {
                object curValue = GetValue(ValueFormat.Internal, row);
                if (curValue is IList lst && lst.Count > 0)
                    foreach (object val in lst) Validator(this, val, row);
                else Validator(this, curValue, row);
            }
            FirePropertyChange(new PropertyChangeEventArgs(PropertyChange.Validation, null, null, row));
        }

        /// <summary>
        /// A delegate to support custom validation functions.
        /// The delegate is multicast to allow combining multiple validation functions.
        /// </summary>
        /// <param name="prop">The data property being validated. The function can use
        /// the property configuration for validation or error messages. Result of the validation
        /// should be added to the property's validation error list.</param>
        /// <param name="value">The value to validate.</param>
        /// <param name="row">The row in a list object or null for regular data objects.</param>
        public delegate void ValueValidator(DataProperty prop, object value, DataRow row);

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
        /// <param name="row">The row in a list object or null for regular data objects.</param>
        public static void ValidateRequired(DataProperty dp, object value, DataRow row)
        {
            if (dp != null && dp.Required && dp.IsValueNull(value, ValueFormat.Internal))
                dp.AddValidationError(row, Messages.Validation_Required, dp);
        }

        /// <summary>
        /// The type of lookup table validation to perform, where applicable.
        /// Defaults to <see cref="LookupValidationType.AnyItem"/>.
        /// </summary>
        public LookupValidationType LookupValidation { get; set; } = LookupValidationType.AnyItem;

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
    }
}
