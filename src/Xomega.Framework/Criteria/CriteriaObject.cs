// Copyright (c) 2025 Xomega.Net. All rights reserved.

using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Threading;
using System.Collections;
using System.Reflection;
using Xomega.Framework.Services;

namespace Xomega.Framework.Criteria
{
    /// <summary>
    /// A data object that stores search criteria with operators
    /// </summary>
    public abstract class CriteriaObject : DataObject
    {
        /// <summary>
        /// Suffix for the operator property name.
        /// </summary>
        public const string Operator = "Operator";

        /// <summary>
        /// Suffix for the property name of the second criteria value (end in a range).
        /// </summary>
        public const string V2 = "2";

        /// <summary>
        /// Name of the property that stores the field to be used for criteria.
        /// </summary>
        public const string FieldSelector = "CriteriaFieldSelector";

        /// <summary>
        /// Constructs a new criteria object
        /// </summary>
        protected CriteriaObject()
        {
        }

        /// <summary>
        /// Constructs a new criteria object with a service provider
        /// </summary>
        protected CriteriaObject(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        /// <inheritdoc/>
        protected override string ResetName => Messages.Action_ResetAll;

        /// <summary>
        /// A map of criteria property groups for each field criteria.
        /// </summary>
        public Dictionary<string, CriteriaPropertyGroup> CriteriaFieldGroups { get; private set; } = new Dictionary<string, CriteriaPropertyGroup>();

        /// <summary>
        /// Adds specified criteria property group to the criteria object.
        /// </summary>
        /// <param name="criteriaField">Criteria property group for a field.</param>
        protected void AddCriteriaPropertyGroup(CriteriaPropertyGroup criteriaField)
        {
            CriteriaFieldGroups.Add(criteriaField.FieldName, criteriaField);
        }

        /// <summary>
        /// Data property for selecting a field to add or edit criteria for.
        /// </summary>
        public DataProperty FieldSelectorProperty { get; set; }

        /// <summary>
        /// Data object for separately editing criteria for the selected field.
        /// </summary>
        public CriteriaEditObject FieldEditObject { get; set; }


        /// <summary>
        /// Initializes the FieldSelectorProperty and its listeners.
        /// </summary>
        protected override void OnInitialized()
        {
            FieldSelectorProperty = new DataProperty(this, FieldSelector)
            {
                AsyncItemsProvider = async (input, row, token) =>
                    await Task.FromResult(CriteriaFieldGroups
                    .Select(g => g.Value.FieldName)
                    .Where(f => !StaticFields.Contains(f))),
                ValueConverter = (ref object value, ValueFormat format) =>
                {
                    if (format == ValueFormat.DisplayString)
                    {
                        value = this[Convert.ToString(value)]?.Label;
                        return true;
                    }
                    return false;
                }
            };

            FieldSelectorProperty.AsyncChange += async (sender, e, t) =>
            {
                if (!e.Change.IncludesValue()) return;

                if (CriteriaFieldGroups.TryGetValue(FieldSelectorProperty.EditStringValue, out CriteriaPropertyGroup group))
                {
                    FieldEditObject = CreateEditObject(group);
                    await FieldEditObject.SetDefaultOperatorAsync();
                }
                else FieldEditObject = null;
            };
            base.OnInitialized();
        }

        /// <summary>
        /// Creates a new criteria edit object for editing the given criteria property group.
        /// Subclasses can override this method to return a different type of object.
        /// </summary>
        /// <param name="group">Criteria property group to edit.</param>
        /// <returns>A criteria edit object constructed for the given group.</returns>
        protected virtual CriteriaEditObject CreateEditObject(CriteriaPropertyGroup group)
            => new CriteriaEditObject(ServiceProvider, group);

        /// <summary>
        /// Cancels the current edit and resets the field selector.
        /// </summary>
        public void CancelEdit() => FieldSelectorProperty?.SetValue(null);

        /// <summary>
        /// Resets the current edit object to its default state.
        /// </summary>
        public void ResetEdit() => FieldEditObject?.ResetData();

        /// <summary>
        /// Applies the data in the current edit object to the underlying criteria group.
        /// </summary>
        public void ApplyEdit()
        {
            if (FieldEditObject != null && FieldEditObject.Update())
            {
                FieldSelectorProperty.SetValue(null);
            }
        }

        /// <summary>
        /// Validates the current criteria object and stores validation errors.
        /// If the current criteria edit object has any values, it validates it first.
        /// If the edits are valid, it auto-applies them before validating the criteria object.
        /// This allows clicking Search without having to click Add/Update first.
        /// If the edits are not valid, a special validation error is added to the criteria object
        /// to prevent the caller from running the search.
        /// </summary>
        /// <param name="force">True to force validation, false otherwise.</param>
        public override void Validate(bool force)
        {
            if (FieldEditObject != null && FieldEditObject.HasValues())
            {
                FieldEditObject.Validate(force);
                FieldEditObject.Errors = FieldEditObject.GetValidationErrors();
                if (FieldEditObject.Errors.HasErrors())
                {
                    validationErrorList = NewErrorList();
                    validationErrorList.AddValidationError(Messages.CriteriaEditObject_ValidationErrors);
                    return;
                }
                else ApplyEdit();
            }

            base.Validate(force);
        }

        /// <summary>
        /// Sets values from the given collection and adjusts values for operators
        /// </summary>
        /// <param name="nvc">Collection to set values from</param>
        public override void SetValues(NameValueCollection nvc)
        {
            base.SetValues(nvc);
            AdjustOperators();
        }

        /// <inheritdoc/>
        public async override Task SetValuesAsync(NameValueCollection nvc, CancellationToken token)
        {
            await base.SetValuesAsync(nvc, token);
            AdjustOperators();
        }

        private void AdjustOperators()
        {
            // clear operators, for which associated properties are blank
            foreach (DataProperty p in Properties.Where(p => p is OperatorProperty).ToList())
            {
                OperatorProperty op = p as OperatorProperty;
                bool isBlank = true;
                foreach (string nm in new string[] { op.AdditionalPropertyName, op.AdditionalPropertyName2 })
                {
                    if (nm != null && HasProperty(nm) && !this[nm].IsNull()) isBlank = false;
                }
                if (isBlank) op.SetValue(null);
            }
        }

        /// <summary>
        /// Determines if any criteria are populated
        /// </summary>
        /// <returns>True if there exists a non-null criteria value</returns>
        public bool HasCriteria()
        {
            return Properties.Where(p => !(p is OperatorProperty)).ToList().Exists(p => !p.IsNull());
        }

        /// <summary>
        /// Selects specified field for editing its criteria.
        /// </summary>
        /// <param name="field"></param>
        public void EditCriteria(string field)
            => FieldSelectorProperty?.SetValue(field);

        /// <summary>
        /// Resets criteria for the specified field.
        /// </summary>
        /// <param name="field"></param>
        public void ResetCriteria(string field)
        {
            if (CriteriaFieldGroups.TryGetValue(field, out CriteriaPropertyGroup critGrp))
                critGrp.Reset();
        }

        /// <summary>
        /// Returns a list of fields that are edited statically and cannot be selected by the field selector
        /// or edited by the criteria edit object. Subclasses can override this property to return the list of static fields.
        /// </summary>
        public virtual string[] StaticFields => new string[] { };

        /// <summary>
        /// Returns a list of criteria display structures for each field of the current criteria object that has values specified.
        /// </summary>
        /// <param name="nonStaticOnly">True to return non-static fields only, false to return all populated fields.</param>
        /// <returns>A list of criteria display structures.</returns>
        public List<FieldCriteriaDisplay> GetCriteriaDisplays(bool nonStaticOnly)
        {
            List<FieldCriteriaDisplay> displays = new List<FieldCriteriaDisplay>();
            var resMgr = ResourceMgr ?? Messages.ResourceManager;
            var and = resMgr.GetString(Messages.Operator_And);
            foreach (var grp in CriteriaFieldGroups.Values.Where(g => 
                g.HasValue() && (!nonStaticOnly || !StaticFields.Contains(g.FieldName))))
            {
                FieldCriteriaDisplay display = new FieldCriteriaDisplay()
                {
                    Field = grp.FieldName,
                    Label = this[grp.FieldName].Label,
                    Operator = grp.OperatorProperty?.DisplayStringValue
                };
                List<string> values = new List<string>();
                if (!grp.ValueProperty.IsNull())
                {
                    var val = grp.ValueProperty.ResolveValue(grp.ValueProperty.InternalValue, ValueFormat.DisplayString);
                    if (val is IList l)
                        foreach (var v in l)
                            values.Add(Convert.ToString(v));
                    else values.Add(Convert.ToString(val));
                }
                if (grp.Value2Property != null && !grp.Value2Property.IsNull())
                {
                    values.Add(grp.Value2Property.DisplayStringValue);
                    display.And = and;
                }
                display.Value = values.ToArray();
                displays.Add(display);
            }
            return displays;
        }

        /// <summary>
        /// Overrides the base method to populate FieldCriteria properties of the specified data contract
        /// from the corresponding field groups of the current criteria object.
        /// </summary>
        /// <param name="dataContract"></param>
        /// <param name="options"></param>
        public override void ToDataContract(object dataContract, object options)
        {
            if (dataContract == null) return;
            var props = dataContract.GetType().GetProperties()
                .Where(p => p.PropertyType.IsGenericType &&
                       p.PropertyType.GetGenericTypeDefinition() == typeof(FieldCriteria<>));
            foreach (PropertyInfo pi in props)
            {
                if (!CriteriaFieldGroups.TryGetValue(pi.Name, out var crit) || !crit.HasValue()) continue;

                var fldCrit = typeof(CriteriaPropertyGroup).GetMethod("ToFieldCriteria")
                    .MakeGenericMethod(pi.PropertyType.GenericTypeArguments[0]).Invoke(crit, new object[0] );
                pi.SetValue(dataContract, fldCrit);
            }
        }
    }
}
