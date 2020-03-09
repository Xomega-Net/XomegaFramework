// Copyright (c) 2020 Xomega.Net. All rights reserved.

using System.Collections.Generic;

namespace Xomega.Framework
{
    /// <summary>
    /// A class that represents an event for selection of one or multiple data rows in a view.
    /// </summary>
    public class ViewSelectionEvent : ViewEvent
    {
        /// <summary>
        /// Selected data rows.
        /// </summary>
        public List<DataRow> SelectedRows { get; private set; }

        /// <summary>
        /// Constructs a view selection event.
        /// </summary>
        /// <param name="selectedRows">Selected data rows.</param>
        public ViewSelectionEvent(List<DataRow> selectedRows) : base(1 << 4)
        {
            this.SelectedRows = selectedRows;
        }
    }
}
