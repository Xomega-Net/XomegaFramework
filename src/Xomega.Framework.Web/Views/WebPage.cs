// Copyright (c) 2020 Xomega.Net. All rights reserved.

using System;
using System.Collections.Generic;
using System.Web.UI;

namespace Xomega.Framework.Web
{
    /// <summary>
    /// Base class for web pages.
    /// It provides a workaround for issues with extender controls in async pages,
    /// when their visibility changes in the async task from false to true,
    /// e.g. their view becomes visible during an async button click event.
    /// </summary>
    public class WebPage : Page
    {
        // See https://forums.asp.net/p/2163988/6293999.aspx?p=True&t=637169211245961059 for the issue details.

        // A list to track controls that should be hidden, but must be made visible temporarily
        // for their extenders to work.
        private readonly HashSet<Control> ControlsToHide = new HashSet<Control>();

        /// <summary>
        /// Overrides OnPreRender in Async mode to make all hidden registered controls visible temporarily,
        /// so that the extenders would be registered in their OnPreRender methods,
        /// which is called for visible controls only.
        /// </summary>
        protected override void OnPreRender(EventArgs e)
        {
            if (IsAsync)
            {
                // Make hidden controls visible to allow OnPreRender calls for their extender controls.
                foreach (var ctl in ControlsToHide)
                    ctl.Visible = true;

                // Register a callback to make them hidden again on PreRender completion,
                // unless the async code made them visible via the SetControlVisible,
                // which would remove them from the tracking list.
                PreRenderComplete += (sender, evt) => {
                    foreach (var ctl in ControlsToHide)
                        ctl.Visible = false;
                };
            }
            base.OnPreRender(e);
        }

        /// <summary>
        /// Sets visibility for control that may be made visible during an async task.
        /// Tracks this control to set its visibility during the proper life cycle event,
        /// in order to work around issues with extenders' visibility.
        /// </summary>
        /// <param name="control">Control that may be made visible during an async task.</param>
        /// <param name="visible">True to make it visible, false otherwise.</param>
        public void SetControlVisible(Control control, bool visible)
        {
            if (control == null) return;
            if (IsAsync)
            {
                if (visible)
                    ControlsToHide.Remove(control);
                else ControlsToHide.Add(control);
            }
            control.Visible = visible;
        }
    }
}