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
        /// Close
        /// </summary>
        public const string Action_Close = "Action_Close";

        /// <summary>
        /// C
        /// </summary>
        public const string Action_Close_AccessKey = "Action_Close_AccessKey";

        /// <summary>
        /// Delete
        /// </summary>
        public const string Action_Delete = "Action_Delete";

        /// <summary>
        /// t
        /// </summary>
        public const string Action_Delete_AccessKey = "Action_Delete_AccessKey";

        /// <summary>
        /// Reset
        /// </summary>
        public const string Action_Reset = "Action_Reset";

        /// <summary>
        /// R
        /// </summary>
        public const string Action_Reset_AccessKey = "Action_Reset_AccessKey";

        /// <summary>
        /// Save
        /// </summary>
        public const string Action_Save = "Action_Save";

        /// <summary>
        /// S
        /// </summary>
        public const string Action_Save_AccessKey = "Action_Save_AccessKey";

        /// <summary>
        /// Search
        /// </summary>
        public const string Action_Search = "Action_Search";

        /// <summary>
        /// S
        /// </summary>
        public const string Action_Search_AccessKey = "Action_Search_AccessKey";

        /// <summary>
        /// Select
        /// </summary>
        public const string Action_Select = "Action_Select";

        /// <summary>
        /// S
        /// </summary>
        public const string Action_Select_AccessKey = "Action_Select_AccessKey";

        /// <summary>
        /// Unexpected system error occurred. Please see logs for details.
        /// </summary>
        public const string Exception_Unhandled = "Exception_Unhandled";

        /// <summary>
        /// Your session has expired. Please log in again.
        /// </summary>
        public const string Login_SessionExpired = "Login_SessionExpired";

        /// <summary>
        /// Unsupported operator {0} for the {1}.
        /// Where {0}=Operator, {1}=Field name
        /// </summary>
        public const string Operator_NotSupported = "Operator_NotSupported";

        /// <summary>
        /// Operator {0} expects {1} value(s), but only {2} were provided for {3}.
        /// Where {0}=Operator, {1}=Expected number of values, {2}=Supplied number of values, {3}=Field name
        /// </summary>
        public const string Operator_NumberOfValues = "Operator_NumberOfValues";

        /// <summary>
        /// First Page
        /// </summary>
        public const string Pager_First = "Pager_First";

        /// <summary>
        /// Last Page
        /// </summary>
        public const string Pager_Last = "Pager_Last";

        /// <summary>
        /// Next Page
        /// </summary>
        public const string Pager_Next = "Pager_Next";

        /// <summary>
        /// Items per page
        /// </summary>
        public const string Pager_PageSize = "Pager_PageSize";

        /// <summary>
        /// Previous Page
        /// </summary>
        public const string Pager_Prev = "Pager_Prev";

        /// <summary>
        /// Showing {0} to {1} of {2} items.
        /// Where {0}=Lower range boundary, {1}=Upper range boundary, {2}=Total number of items
        /// </summary>
        public const string Pager_Summary = "Pager_Summary";

        /// <summary>
        /// Add Selected
        /// </summary>
        public const string PickList_Add = "PickList_Add";

        /// <summary>
        /// Add All
        /// </summary>
        public const string PickList_AddAll = "PickList_AddAll";

        /// <summary>
        /// Available {0}
        /// Where {0}=Property name
        /// </summary>
        public const string PickList_Available = "PickList_Available";

        /// <summary>
        /// Remove Selected
        /// </summary>
        public const string PickList_Remove = "PickList_Remove";

        /// <summary>
        /// Remove All
        /// </summary>
        public const string PickList_RemoveAll = "PickList_RemoveAll";

        /// <summary>
        /// Selected {0}
        /// Where {0}=Property name
        /// </summary>
        public const string PickList_Selected = "PickList_Selected";

        /// <summary>
        /// Select {0}...
        /// Where {0}=Property name
        /// </summary>
        public const string Select_RequiredOption = "Select_RequiredOption";

        /// <summary>
        /// The service returned an empty response with the following status: {0} - {1}
        /// Where {0}=Status code, {1}=Status description
        /// </summary>
        public const string Service_EmptyResponse = "Service_EmptyResponse";

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
        /// {0} must be less than {1}.
        /// Where {0}=Property name, {1}=Maximum value
        /// </summary>
        public const string Validation_NumberMaximumExcl = "Validation_NumberMaximumExcl";

        /// <summary>
        /// {0} cannot be less than {1}.
        /// Where {0}=Property name, {1}=Minimum value
        /// </summary>
        public const string Validation_NumberMinimum = "Validation_NumberMinimum";

        /// <summary>
        /// {0} must be greater than {1}.
        /// Where {0}=Property name, {1}=Minimum value
        /// </summary>
        public const string Validation_NumberMinimumExcl = "Validation_NumberMinimumExcl";

        /// <summary>
        /// {0} is required.
        /// Where {0}=Property name
        /// </summary>
        public const string Validation_Required = "Validation_Required";

        /// <summary>
        /// Search Criteria
        /// </summary>
        public const string View_Criteria = "View_Criteria";

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
        /// Please review the following error.
        /// </summary>
        public const string View_Error = "View_Error";

        /// <summary>
        /// Please review the following errors.
        /// </summary>
        public const string View_Errors = "View_Errors";

        /// <summary>
        /// Please review the following message.
        /// </summary>
        public const string View_Message = "View_Message";

        /// <summary>
        /// Please review the following messages.
        /// </summary>
        public const string View_Messages = "View_Messages";

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

        /// <summary>
        /// Please review the following warning.
        /// </summary>
        public const string View_Warning = "View_Warning";

        /// <summary>
        /// Please review the following warnings.
        /// </summary>
        public const string View_Warnings = "View_Warnings";
    }
}
