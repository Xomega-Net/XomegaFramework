﻿// Copyright (c) 2021 Xomega.Net. All rights reserved.

namespace Xomega.Framework.Views
{
    /// <summary>
    /// Base interface for CollapsiblePanel control.
    /// </summary>
    public interface ICollapsiblePanel
    {
        /// <summary>
        /// Controls collapsed/expanded state of the control.
        /// </summary>
        bool Collapsed { get; set; }
    }
}
