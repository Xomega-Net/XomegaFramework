// Copyright (c) 2024 Xomega.Net. All rights reserved.

using System.Collections.Generic;
using Xomega.Framework.Criteria;

namespace Xomega.Framework.Views
{
    /// <summary>
    /// Interface for AppliedCriteriaPanel controls.
    /// </summary>
    public interface IAppliedCriteriaPanel
    {
        /// <summary>
        /// Binds this panel to the specified applied criteria
        /// </summary>
        void BindTo(List<FieldCriteriaDisplay> settings);
    }
}
