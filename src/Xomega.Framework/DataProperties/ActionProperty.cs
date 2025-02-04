// Copyright (c) 2025 Xomega.Net. All rights reserved.

using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq.Expressions;
using System.Resources;

namespace Xomega.Framework
{
    /// <summary>
    /// A property that represents an action, which can be bound to action buttons
    /// and control the state of the bound button, such visible or enabled.
    /// </summary>
    public class ActionProperty : BaseProperty
    {
        private readonly IServiceProvider serviceProvider;

        /// <summary>
        /// Constructs an action property that is not part of any data object, e.g. is defined on a view level.
        /// </summary>
        /// <param name="serviceProvider">The service provider used to obtain resource manager for the action.</param>
        /// <param name="name">The action name.</param>
        public ActionProperty(IServiceProvider serviceProvider, string name) : base(null, name)
        {
            this.serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Constructs an action property that is a part of a data object.
        /// </summary>
        /// <param name="parent">The data object the action belongs to.</param>
        /// <param name="name">The name of the action, which should be unique within the parent data object.</param>
        public ActionProperty(DataObject parent, string name) : base(parent, name)
        {
            if (parent != null) parent.AddAction(this);
        }

        /// <summary>
        /// Specifies whether or not the action is currently enabled.
        /// This is just an alias for the underlying editable flag to avoid confusion,
        /// since actions technically don't support editability.
        /// </summary>
        public bool Enabled { get => editable; set => Editable = value; }

        /// <summary>
        /// Sets the expression to use for computing whether the action is enabled.
        /// This is just an alias for setting computed editable flag to avoid confusion,
        /// since actions technically don't support editability.
        /// </summary>
        /// <param name="expression">Lambda expression used to compute if the action is enabled,
        /// or null to make the enabled flag non-computed.</param>
        /// <param name="args">Arguments for the specified expression to use for evaluation.</param>
        public void SetComputedEnabled(LambdaExpression expression, params object[] args) => SetComputedEditable(expression, args);

        /// <summary>
        /// Manually updates the property enabled (editable) flag with the computed result,
        /// in addition to automatic updates when the underlying properties change.
        /// </summary>
        public void UpdateComputedEnabed() => UpdateComputedEditable();
                
        /// <summary>
        /// Overrides action's resource key to prepend the 'Action_' prefix as needed.
        /// </summary>
        protected override string ResourceKey => (Name.StartsWith("Action_") ? "" : "Action_") + Name;

        /// <summary>
        /// Overrides the parent resource key to use underscore as a separator,
        /// in order to distinguish action resources from the ones for regular properties that are separated with a dot.
        /// </summary>
        protected override string ParentResourceKey => parent?.GetResourceKey() + "_";

        /// <summary>
        /// Overrides access to resource manager to use the action's service provider for actions that don't have a parent data object.
        /// </summary>
        protected override ResourceManager ResourceMgr => serviceProvider?.GetService<ResourceManager>() ?? base.ResourceMgr;
    }
}
