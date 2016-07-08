// Copyright (c) 2016 Xomega.Net. All rights reserved.

using System.Web.UI;

namespace Xomega.Framework.Web
{
    /// <summary>
    /// Base class for CollapsiblePanel control.
    /// </summary>
    public abstract class CollapsiblePanelBase : UserControl, INamingContainer
    {
        /// <summary>
        /// Controls collapsed/expanded state of the control.
        /// </summary>
        public abstract bool Collapsed { get; set; }
    }
}
