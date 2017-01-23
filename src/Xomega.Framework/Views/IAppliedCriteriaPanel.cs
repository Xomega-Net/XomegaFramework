// Copyright (c) 2017 Xomega.Net. All rights reserved.

using System.Collections.Generic;

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
        void BindTo(List<FieldCriteriaSetting> settings);
    }
}
