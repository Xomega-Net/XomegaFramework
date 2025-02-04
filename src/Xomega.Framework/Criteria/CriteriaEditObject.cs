// Copyright (c) 2024 Xomega.Net. All rights reserved.

using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq.Expressions;
using System.Resources;
using System.Threading.Tasks;
using Xomega.Framework.Properties;

namespace Xomega.Framework.Criteria
{
    /// <summary>
    /// Data object that is used for editing criteria for a specific field.
    /// </summary>
    public class CriteriaEditObject : DataObject
    {
        private readonly CriteriaPropertyGroup group;

        private const string Value = "Value";

        /// <summary>
        /// The property that is used for selecting the operator for the criteria, if available.
        /// </summary>
        public DataProperty OperatorProperty { get; set; }

        /// <summary>
        /// The property that is used for selecting the value for the criteria.
        /// </summary>
        public DataProperty ValueProperty { get; set; }

        /// <summary>
        /// The property that is used for selecting the second value for the criteria,
        /// if needed to specify a range for the Between operators.
        /// </summary>
        public DataProperty Value2Property { get; set; }

        /// <summary>
        /// The property for storing the "and" label, which is visible only if the second value is visible.
        /// </summary>
        public DataProperty AndProperty { get; set; }

        /// <summary>
        /// The list of validation errors, if any.
        /// </summary>
        public ErrorList Errors { get; set; }

        /// <summary>
        /// The action that is used for cancelling the criteria edit.
        /// </summary>
        public ActionProperty CancelAction { get; private set; }

        /// <summary>
        /// The action that is used for adding or updating the criteria.
        /// </summary>
        public new ActionProperty AddAction { get; private set; }


        /// <summary>
        /// Constructor for the criteria edit object for the given criteria property group.
        /// </summary>
        /// <param name="serviceProvider">Service provider to use.</param>
        /// <param name="group">Underlying criteria property group to edit.</param>
        public CriteriaEditObject(IServiceProvider serviceProvider, CriteriaPropertyGroup group) : base(serviceProvider)
        {
            this.group = group;
            TrackModifications = false;

            if (group.OperatorProperty != null)
                OperatorProperty = group.OperatorProperty.Clone(this, Value + CriteriaObject.Operator);

            if (group.ValueProperty != null)
                ValueProperty = group.ValueProperty.Clone(this, Value);

            if (group.Value2Property != null)
            {
                Value2Property = group.Value2Property.Clone(this, Value + CriteriaObject.V2);

                AndProperty = new TextProperty(this, "and");
                var resMgr = ServiceProvider?.GetService<ResourceManager>() ?? Messages.ResourceManager;
                AndProperty.SetValue(resMgr.GetString(Messages.Operator_And));
                Expression<Func<DataProperty, bool>> andVisible = (p) => p.Visible;
                AndProperty.SetComputedVisible(andVisible, Value2Property);
                AndProperty.UpdateComputedVisible();
            }

            CancelAction = new ActionProperty(this, Messages.Action_Cancel);
            AddAction = new ActionProperty(this, group.HasValue() ? Messages.Action_Update : Messages.Action_Add);
            Expression<Func<DataProperty, DataProperty, bool>> addEnabled = (pOp, pVal) =>
                pOp != null && pOp.InternalValue != null || pVal != null && pVal.InternalValue != null;
            AddAction.SetComputedEnabled(addEnabled, OperatorProperty, ValueProperty);
            AddAction.UpdateComputedEnabed();

            OnInitialized();
        }

        /// <summary>
        /// Empty implementation for the abstract method from the base class.
        /// </summary>
        protected override void Initialize()
        {
        }

        /// <summary>
        /// Sets the default operator for the criteria from the underlying group.
        /// It needs to be asynchronous to allow loading the lookup table and validate the default operator.
        /// If the default operator is not valid, it is set to null.
        /// </summary>
        /// <returns>The async task.</returns>
        public virtual async Task SetDefaultOperatorAsync()
        {
            if (!group.HasValue() && group.OperatorProperty != null)
            {
                await OperatorProperty.SetValueAsync(group.GetDefaultOperator());
                if (!OperatorProperty.IsValid(true, null))
                    OperatorProperty.SetValue(null);
            }
        }

        /// <summary>
        /// Checks if the criteria has values.
        /// </summary>
        /// <returns>True if operator or value are populated.</returns>
        public virtual bool HasValues() =>
            OperatorProperty != null && !OperatorProperty.IsNull() || 
            ValueProperty != null && !ValueProperty.IsNull();

        /// <summary>
        /// Validates each property and also perform a range validation, if needed.
        /// </summary>
        /// <param name="force">True to force the validation, false otherwise.</param>
        public override void Validate(bool force)
        {
            base.Validate(force);

            var v1 = ValueProperty?.InternalValue;
            var v2 = Value2Property?.InternalValue;
            if (v1 != null && v2 != null && v1 is IComparable c1 && v2 is IComparable c2 && c1.CompareTo(c2) > 0)
            {
                validationErrorList.AddValidationError(Messages.Validation_Range);
            }
        }

        /// <summary>
        /// Validates the edits and, if valid, updates the underlying criteria with the values from this edit object.
        /// </summary>
        /// <returns>True if the values were valid, false otherwise.</returns>
        public virtual bool Update()
        {
            Validate(true);
            Errors = GetValidationErrors();
            if (Errors.HasErrors()) return false;

            group.OperatorProperty?.SetValue(OperatorProperty.InternalValue);
            group.ValueProperty?.SetValue(ValueProperty.InternalValue);
            group.Value2Property?.SetValue(Value2Property.InternalValue);
            return true;
        }
    }
}
