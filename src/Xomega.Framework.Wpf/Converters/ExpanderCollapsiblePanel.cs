// Copyright (c) 2020 Xomega.Net. All rights reserved.

using System.Windows.Controls;

namespace Xomega.Framework.Views
{
    /// <summary>
    /// An Expander wrapper implementation of ICollapsiblePanel
    /// </summary>
    public class ExpanderCollapsiblePanel : ICollapsiblePanel
    {
        private readonly Expander exp;

        /// <summary>
        /// Constructs a new ICollapsiblePanel wrapper for the given Expander
        /// </summary>
        /// <param name="expander">Expander to wrap</param>
        public ExpanderCollapsiblePanel(Expander expander)
        {
            exp = expander;
        }

        /// <summary>
        /// Controls collapsed/expanded state of the control.
        /// </summary>
        public bool Collapsed
        {
            get => !exp.IsExpanded;
            set => exp.IsExpanded = !value;
        }
    }
}
