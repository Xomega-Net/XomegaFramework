// Copyright (c) 2024 Xomega.Net. All rights reserved.

using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Resources;
using System.Threading;
using System.Threading.Tasks;

namespace Xomega.Framework
{
    /// <summary>
    /// The base class for all Xomega properties that defines various additional meta-information
    /// that can be associated with a piece of data, such as description, editability, visibility,
    /// security, whether or not it is required, etc. It also provides support for notification
    /// of any changes in this type of information.
    /// Xomega properties are typically added to Xomega data objects that can serve as a data model
    /// for user interface screens.
    /// </summary>
    public partial class BaseProperty
    {
        /// <summary>
        /// The parent data object of the property if any. In rare cases the parent can be set to null
        /// and therefore should be always checked for null.
        /// </summary>
        protected DataObject parent;

        /// <summary>
        /// The column index of the property when it is part of a data list object.
        /// </summary>
        internal int Column { get; set; } = -1;

        /// <summary>
        /// Constructs a base property with the given name and a parent data object.
        /// </summary>
        /// <param name="parent">The parent data object for the property.</param>
        /// <param name="name">Property name that should be unique within the parent object.</param>
        public BaseProperty(DataObject parent, string name)
        {
            this.parent = parent;
            Name = name;
        }

        /// <summary>
        /// Returns the parent data object of the property when set, null otherwise.
        /// </summary>
        /// <returns>The parent data object of the property.</returns>
        public DataObject GetParent()
        {
            return parent;
        }

        /// <summary>
        /// A flag indicating if this a key property within its parent data object
        /// </summary>
        public bool IsKey { get; set; }

        /// <summary>
        /// Performs additional property initialization after all other properties and child objects
        /// have been already added to the parent object and would be accessible from within this method.
        /// </summary>
        public virtual void Initialize()
        {
            // the subclasses can implement the additional initialization
        }

        #region Property Description

        /// <summary>
        /// Internal property name, which should be unique within its parent object.
        /// </summary>
        public string Name { get; private set; }

        // externally set label to use
        private string label;

        /// <summary>
        /// User-friendly property label that can be used in error messages and other places
        /// to identify the property for the user.
        /// </summary>
        public string Label { get => label ?? GetLabel(); set => label = value; }

        /// <summary>
        /// Returns a user-friendly string representation of the property.
        /// </summary>
        /// <returns>The property label if available, otherwise the property name
        /// converted to words if Pascal case is used.</returns>
        public override string ToString() => Label;

        /// <summary>
        /// Gets a localized label for the current property. If no label text is available
        /// uses the property name to generate a label.
        /// </summary>
        /// <returns>A localized label for the current property.</returns>
        protected string GetLabel()
        {
            var label = ResourceMgr?.GetString(ResourceKey, ParentResourceKey);
            if (label == null) label = DataObject.StringToWords(Name);
            return label;
        }

        /// <summary>
        /// Localized access key for the current property, or null if no access key is defined.
        /// </summary>
        public virtual string AccessKey => ResourceMgr?.GetString(ResourceKey + "_AccessKey", ParentResourceKey);

        /// <summary>
        /// Sets the property label text, replacing extraneous characters.
        /// </summary>
        /// <param name="text"></param>
        public virtual void SetLabel(string text)
        {
            Label = text?.Replace("_", "")?.Trim()?.Trim(':');
        }

        /// <summary>
        /// Resource manager to be used for the property, which can be overridden in subclasses.
        /// </summary>
        protected virtual ResourceManager ResourceMgr => parent?.ServiceProvider?.GetService<ResourceManager>() ?? Messages.ResourceManager;

        /// <summary>
        /// Base resource key to be used for the property, which can be overridden in subclasses.
        /// </summary>
        protected virtual string ResourceKey => Name;

        /// <summary>
        /// Parent resource key for the property to qualify the base resource key.
        /// </summary>
        protected virtual string ParentResourceKey => parent?.GetResourceKey() + ".";

        #endregion

        #region Property Change support

        /// <summary>
        /// Generic property change event for listening to all changes to the property.
        /// </summary>
        public event EventHandler<PropertyChangeEventArgs> Change;

        /// <summary>
        /// Async property change event, which allows waiting for all async handlers to complete.
        /// </summary>
        public event Func<object, PropertyChangeEventArgs, CancellationToken, Task> AsyncChange;

        /// <summary>
        /// A method to fire a property change event.
        /// If certain property information is calculated and depends on the factors
        /// that are outside of the property's control (e.g. editability), 
        /// then this method may need to be called from outside to fire a property change event
        /// if certain conditions that affect the calculated value have changed.
        /// </summary>
        /// <param name="args">Property change event arguments.</param>
        public void FirePropertyChange(PropertyChangeEventArgs args)
        {
            Change?.Invoke(this, args);
            AsyncChange?.DynamicInvoke(this, args, default(CancellationToken));
        }

        /// <summary>
        /// Asynchronously fires the specified property change event, which allows waiting for all async handlers to complete.
        /// </summary>
        public async Task FirePropertyChangeAsync(PropertyChangeEventArgs args, CancellationToken token = default)
        {
            args.IsAsync = true;
            Change?.Invoke(this, args);
            var tasks = AsyncChange?.GetInvocationList()?.Select(d => (Task)d.DynamicInvoke(this, args, token));
            if (tasks != null)
                await Task.WhenAll(tasks);
        }

        #endregion

        #region Editability support

        /// <summary>
        /// An internal flag to allow manually making the property uneditable.
        /// The default value is true.
        /// </summary>
        protected bool editable = true;

        /// <summary>
        /// Returns a value indicating whether or not the property is editable.
        /// This value is calculated based on the internal value of the editable field,
        /// the result of the call delegated to the parent object to determine this property's editability
        /// and the value of the security access level. Controls bound to this property
        /// should update their editability based on this value.
        /// Setting this value updates the internal editable flag and fires the property change event if necessary.
        /// </summary>
        public bool Editable
        {
            get
            {
                bool b = editable;
                if (parent != null) b &= parent.IsPropertyEditable(this);
                return b && AccessLevel > AccessLevel.ReadOnly;
            }
            set => SetEditable(value, null);
        }

        /// <summary>
        /// Returns whether this property is editable for the given row or in general,
        /// if the row is null or its editability is not set for this property.
        /// </summary>
        /// <param name="row">The row, for which to check property editability.</param>
        /// <returns>True if the property is editable, false otherwise.</returns>
        public bool GetEditable(DataRow row = null) => Editable && (row?.GetEditable(this) ?? true);

        /// <summary>
        /// Sets the property editable for the specified row or in general, if the row is null.
        /// </summary>
        /// <param name="value">The value for the editable flag to set.</param>
        /// <param name="row">The row to set editability of the property for, or null to set it for the entire property.</param>
        public void SetEditable(bool value, DataRow row = null)
        {
            bool b = GetEditable(row);
            if (row == null) editable = value;
            else row.SetEditable(this, value);
            if (GetEditable(row) != b) FirePropertyChange(
                new PropertyChangeEventArgs(PropertyChange.Editable, b, value, row));
        }

        /// <summary>
        /// An internal flag that keeps track of whether or not the property is currently being edited.
        /// </summary>
        private bool editing = false;

        /// <summary>
        /// Returns whether or not the property is currently being edited by the user.
        /// Controls that are bound to this property should set this value to true or false
        /// when they gain or lose focus respectively.
        /// </summary>
        public bool Editing { get => GetEditing(); set => SetEditing(value); }

        /// <summary>
        /// Returns whether or not the property is currently being edited by the user
        /// at the specified row, if any.
        /// </summary>
        /// <param name="row">The row to check if the property is being edited, or null for the entire property.</param>
        /// <returns>True if the property is being edited at the specified row, false otherwise.</returns>
        public bool GetEditing(DataRow row = null) => row?.GetEditing(this) ?? editing;

        /// <summary>
        /// Sets whether or not the property is currently being edited at the specified row, if any.
        /// </summary>
        /// <param name="value">True if the property is being edited, false otherwise.</param>
        /// <param name="row">The row, for which the property is being edited, or null for the entire property.</param>
        public void SetEditing(bool value, DataRow row = null)
        {
            bool b = GetEditing(row);
            if (row == null) editing = value;
            else row.SetEditing(this, value);
            PropertyChange change = PropertyChange.Editing;
            if (!value) change += PropertyChange.Value;
            if (value != b) FirePropertyChange(
                new PropertyChangeEventArgs(change, b, value, row));
        }

        private ComputedBinding editableBinding;

        /// <summary>
        /// Sets the expression to use for computing whether the property is editable.
        /// </summary>
        /// <param name="expression">Lambda expression used to compute the editability,
        /// or null to make the editable flag non-computed.</param>
        /// <param name="args">Arguments for the specified expression to use for evaluation.</param>
        public void SetComputedEditable(LambdaExpression expression, params object[] args)
        {
            if (editableBinding != null) editableBinding.Dispose();
            editableBinding = expression == null ? null : new ComputedEditableBinding(this, expression, args);
        }

        /// <summary>
        /// Manually updates the property editability with the computed result,
        /// in addition to automatic updates when the underlying properties change.
        /// </summary>
        /// <param name="row">The row to update editability of the property for, or null to update it for the entire property.</param>
        public void UpdateComputedEditable(DataRow row = null)
        {
            if (editableBinding != null)
                editableBinding.Update(row);
        }
        #endregion

        #region Visibility support

        /// <summary>
        /// A internal flag to allow manually making the property invisible.
        /// The default value is true.
        /// </summary>
        private bool visible = true;

        /// <summary>
        /// Returns a value indicating whether or not the property is visible.
        /// This value is calculated based on the internal value of the visible field,
        /// the result of the call delegated to the parent object to determine this property's visibility
        /// and the value of the security access level. Controls bound to this property
        /// should update their visibility based on this value.
        /// Setting this value updates the internal visible flag and fires the property change event if necessary.
        /// </summary>
        public bool Visible
        {
            get => visible && AccessLevel > AccessLevel.None;
            set
            {
                bool oldValue = Visible;
                visible = value;
                if (Visible != oldValue) FirePropertyChange(
                    new PropertyChangeEventArgs(PropertyChange.Visible, oldValue, Visible, null));
            }
        }

        private ComputedBinding visibleBinding;

        /// <summary>
        /// Sets the expression to use for computing whether the property is visible.
        /// </summary>
        /// <param name="expression">Lambda expression used to compute the visibility,
        /// or null to make the visible flag non-computed.</param>
        /// <param name="args">Arguments for the specified expression to use for evaluation.</param>
        public void SetComputedVisible(LambdaExpression expression, params object[] args)
        {
            if (visibleBinding != null) visibleBinding.Dispose();
            visibleBinding = expression == null ? null : new ComputedVisibleBinding(this, expression, args);
        }

        /// <summary>
        /// Manually updates the property visibility with the computed result,
        /// in addition to automatic updates when the underlying properties change.
        /// </summary>
        public void UpdateComputedVisible()
        {
            if (visibleBinding != null)
                visibleBinding.Update(null);
        }
        #endregion

        #region Required support

        /// <summary>
        /// A internal flag that keeps track of whether or not the property is required.
        /// The default value is false.
        /// </summary>
        private bool required;

        /// <summary>
        /// Returns a value indicating whether or not the property is required.
        /// This value is calculated based on the internal value of the required field and
        /// the result of the call delegated to the parent object to determine if this property is required.
        /// Setting this value updates the internal required flag and fires the property change event if necessary.
        /// </summary>
        public bool Required
        {
            get => required;
            set
            {
                bool oldValue = Required;
                required = value;
                if (Required != oldValue) FirePropertyChange(
                    new PropertyChangeEventArgs(PropertyChange.Required, oldValue, Required, null));
            }
        }

        private ComputedBinding requiredBinding;

        /// <summary>
        /// Sets the expression to use for computing whether the property is required.
        /// </summary>
        /// <param name="expression">Lambda expression used to compute the required flag,
        /// or null to make the required flag non-computed.</param>
        /// <param name="args">Arguments for the specified expression to use for evaluation.</param>
        public void SetComputedRequired(LambdaExpression expression, params object[] args)
        {
            if (requiredBinding != null) requiredBinding.Dispose();
            requiredBinding = expression == null ? null : new ComputedRequiredBinding(this, expression, args);
        }

        /// <summary>
        /// Manually updates the property required flag with the computed result,
        /// in addition to automatic updates when the underlying properties change.
        /// </summary>
        /// <param name="token">Cancellation token.</param>
        /// <returns>The task for the asynchronous operation.</returns>
        public async Task UpdateComputedRequiredAsync(CancellationToken token = default)
        {
            if (requiredBinding != null)
                await requiredBinding.UpdateAsync(null, token);
        }
        #endregion

        #region Security support

        /// <summary>
        /// Internal field that stores the security access level for the property.
        /// The default value is full access.
        /// </summary>
        private AccessLevel accessLevel = AccessLevel.Full;

        /// <summary>
        /// Returns the current access level for the property.
        /// Allows setting a new access level and fires a property change event
        /// for property editability and visibility, since they both depend on the security access level.
        /// </summary>
        public AccessLevel AccessLevel
        {
            get { return accessLevel; }
            set
            {
                AccessLevel oldValue = accessLevel;
                accessLevel = value;
                FirePropertyChange(new PropertyChangeEventArgs(
                    PropertyChange.Editable + PropertyChange.Visible, oldValue, accessLevel, null));
            }
        }

        #endregion
    }
}
