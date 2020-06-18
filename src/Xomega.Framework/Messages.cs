using System;
using System.Resources;

namespace Xomega.Framework
{
    /// <summary>
    /// Message codes, as well as a resource manager to get a (localized) message text for those.
    /// </summary>
    public static class Messages
    {
        private static readonly Lazy<ResourceManager> resourceManager =
            new Lazy<ResourceManager>(() => new ResourceManager("Xomega.Framework.Resources", typeof(Messages).Assembly));

        /// <summary>
        /// Resource manager for the current messages.
        /// </summary>
        public static ResourceManager ResourceManager
        {
            get { return resourceManager.Value; }
        }

        /// <summary>
        /// Unexpected system error occurred. Please see logs for details.
        /// </summary>
        public const string Exception_Unhandled = "Exception_Unhandled";

        /// <summary>
        /// Unsupported operator {0} for the {1}.
        /// Where {0}=Operator, {1}=Field name
        /// </summary>
        public const string Operator_NotSupported = "Operator_NotSupported";

        /// <summary>
        /// Operator {0} expects {1} value(s), but only {2} were provided for {3}.
        /// Where {0}=Operator, {1}=Expected number of valules, {2}=Supplied number of values, {3}=Field name
        /// </summary>
        public const string Operator_NumberOfValues = "Operator_NumberOfValues";

        /// <summary>
        /// {0} has an invalid date/time: {1}. Please use the following format: {2}.
        /// Where {0}=Property name, {1}=Invalid value, {2}=Valid format
        /// </summary>
        public const string Validation_DateTimeFormat = "Validation_DateTimeFormat";

        /// <summary>
        /// {0} must be a decimal number.
        /// Where {0}=Property name
        /// </summary>
        public const string Validation_DecimalFormat = "Validation_DecimalFormat";

        /// <summary>
        /// {0} contains an invalid number.
        /// Where {0}=Property name
        /// </summary>
        public const string Validation_IntegerFormat = "Validation_IntegerFormat";

        /// <summary>
        /// Invalid lookup table '{1}' specified for {0}.
        /// Where {0}=Property name, {1}=Lookup table name
        /// </summary>
        public const string Validation_InvalidLookupTable = "Validation_InvalidLookupTable";

        /// <summary>
        /// The value '{2}' for {0} should be from the '{1}' lookup table.
        /// Where {0}=Property name, {1}=Lookup table name, {2}=Value
        /// </summary>
        public const string Validation_LookupValue = "Validation_LookupValue";

        /// <summary>
        /// The value '{1}' for {0} is not available for selection.
        /// Where {0}=Property name, {1}=Value
        /// </summary>
        public const string Validation_LookupValueActive = "Validation_LookupValueActive";

        /// <summary>
        /// The values '{2}' for {0} should be from the '{1}' lookup table.
        /// Where {0}=Property name, {1}=Lookup table name, {2}=Values
        /// </summary>
        public const string Validation_LookupValues = "Validation_LookupValues";

        /// <summary>
        /// The value '{2}' for {0} should not be longer than {1} characters.
        /// Where {0}=Property name, {1}=Maximum length, {2}=Value
        /// </summary>
        public const string Validation_MaxLength = "Validation_MaxLength";

        /// <summary>
        /// The values '{2}' for {0} should not be longer than {1} characters.
        /// Where {0}=Property name, {1}=Maximum length, {2}=Values
        /// </summary>
        public const string Validation_MaxLengths = "Validation_MaxLengths";

        /// <summary>
        /// {0} cannot be greater than {1}.
        /// Where {0}=Property name, {1}=Maximum value
        /// </summary>
        public const string Validation_NumberMaximum = "Validation_NumberMaximum";

        /// <summary>
        /// {0} cannot be less than {1}.
        /// Where {0}=Property name, {1}=Minimum value
        /// </summary>
        public const string Validation_NumberMinimum = "Validation_NumberMinimum";

        /// <summary>
        /// {0} is required.
        /// Where {0}=Property name
        /// </summary>
        public const string Validation_Required = "Validation_Required";

        /// <summary>
        /// Are you sure you want to delete this object?
        /// This action cannot be undone.
        /// </summary>
        public const string View_DeleteMessage = "View_DeleteMessage";

        /// <summary>
        /// Delete Confirmation
        /// </summary>
        public const string View_DeleteTitle = "View_DeleteTitle";

        /// <summary>
        /// New {0}
        /// Where {0}=Base title
        /// </summary>
        public const string View_TitleNew = "View_TitleNew";

        /// <summary>
        /// You have unsaved changes.
        /// Do you want to discard them and close the view?
        /// </summary>
        public const string View_UnsavedMessage = "View_UnsavedMessage";

        /// <summary>
        /// Unsaved Changes
        /// </summary>
        public const string View_UnsavedTitle = "View_UnsavedTitle";
    }
}
