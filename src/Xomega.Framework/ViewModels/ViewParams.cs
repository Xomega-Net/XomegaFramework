// Copyright (c) 2019 Xomega.Net. All rights reserved.

namespace Xomega.Framework.Views
{
    /// <summary>
    /// Definition of common view parameters and their possible values
    /// </summary>
    public class ViewParams
    {
        /// <summary>
        /// Parameter indicating action to perform
        /// </summary>
        public static class Action
        {
            /// <summary>
            /// Action parameter name
            /// </summary>
            public const string Param = "_action";

            /// <summary>
            /// Action to create a new object
            /// </summary>
            public const string Create = "create";

            /// <summary>
            /// Action to initiate search on activation
            /// </summary>
            public const string Search = "search";

            /// <summary>
            /// Action to activate for selection
            /// </summary>
            public const string Select = "select";
        }

        /// <summary>
        /// Query parameter indicating specific source link on the parent that invoked this view
        /// </summary>
        public const string QuerySource = "_source";

        /// <summary>
        /// Parameter indicating selection mode to set, if any
        /// </summary>
        public static class SelectionMode
        {
            /// <summary>
            /// Selection mode parameter name
            /// </summary>
            public const string Param = "_selection";

            /// <summary>
            /// Single selection mode
            /// </summary>
            public const string Single = DataListObject.SelectionModeSingle;

            /// <summary>
            /// Multiple selection mode
            /// </summary>
            public const string Multiple = DataListObject.SelectionModeSingle;
        }

        /// <summary>
        /// Parameter for view display modes
        /// </summary>
        public static class Mode
        {
            /// <summary>
            /// Mode parameter name
            /// </summary>
            public const string Param = "_mode";

            /// <summary>
            /// Mode to open views in a popup dialog.
            /// </summary>
            public const string Popup = "popup";

            /// <summary>
            /// Mode to open views inline as master-details.
            /// </summary>
            public const string Inline = "inline";
        }
    }
}
