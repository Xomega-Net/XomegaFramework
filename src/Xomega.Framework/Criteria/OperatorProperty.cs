// Copyright (c) 2024 Xomega.Net. All rights reserved.

using System;
using System.Collections;
using Xomega.Framework.Properties;

namespace Xomega.Framework.Criteria
{
    /// <summary>
    /// A specialized enumeration property that provides operators for search criteria
    /// such as Is Equal, Is Not Equal, Contains, Between etc.
    /// This property can be coupled with additional one or two data properties
    /// that supply the values for the selected operator. Some operators
    /// require no additional values (e.g. Is Null or Is Not Null), most require
    /// one additional value (e.g. Is Equal, Is Greater Than, etc.) or list of values
    /// (Is One Of, Is None Of) and some require two additional properties
    /// for ranges (e.g. Between).
    /// </summary>
    public class OperatorProperty : EnumProperty
    {
        /// <summary>
        /// The name of the operator attribute that stores the number of additional
        /// properties that the operator requires: 0, 1 or 2.
        /// </summary>
        public const string AttributeAddlProps = "addl props";

        /// <summary>
        /// The name of the operator attribute that stores 1 or 0 to indicate
        /// if the additional property can be multivalued.
        /// </summary>
        public const string AttributeMultival = "multival";

        /// <summary>
        /// The name of the operator attribute that stores a fully qualified type
        /// of the additional property, which this operator applies to.
        /// It will also apply to all subclasses of this type. Multiple types can be specified.
        /// </summary>
        public const string AttributeType = "type";

        /// <summary>
        /// The name of the operator attribute that stores a fully qualified type
        /// of the additional property, which this operator does not apply to.
        /// It won't also apply to all subclasses of this type. Multiple exclude types can be specified.
        /// Exclude types should be generally more concrete than include types.
        /// </summary>
        public const string AttributeExcludeType = "exclude type";

        /// <summary>
        /// The name of the operator attribute that stores the sort order
        /// of the operators with respect to other operators.
        /// </summary>
        public const string AttributeSortOrder = "sort order";

        /// <summary>
        /// The name of the operator attribute that stores 1 for null check operators
        /// (Is Null or Is Not Null) to enable easily hiding or showing them.
        /// </summary>
        public const string AttributeNullCheck = "null check";

        /// <summary>
        /// An instance of the first additional property that is obtained from the parent object
        /// by <see cref="AdditionalPropertyName"/> during initialization.
        /// </summary>
        private DataProperty additionalProperty;

        /// <summary>
        /// An instance of the second additional property that is obtained from the parent object
        /// by <see cref="AdditionalPropertyName2"/> during initialization.
        /// </summary>
        private DataProperty additionalProperty2;

        /// <summary>
        /// Gets or sets the name of the first additional property.
        /// If the current operator property name ends with "Operator"
        /// then the first initial property name is defaulted to the first part
        /// of the current property name before the word "Operator".
        /// </summary>
        public string AdditionalPropertyName { get; set; }

        /// <summary>
        /// Gets or sets the name of the second additional property.
        /// If the first additional property name is initialized by default
        /// than the second additional property name is defaulted to the first one
        /// with the string "2" appended at the end.
        /// </summary>
        public string AdditionalPropertyName2 { get; set; }

        /// <summary>
        /// Gets or sets a Boolean to enable or disable display of the null check operators.
        /// </summary>
        public bool HasNullCheck { get; set; }

        /// <summary>
        ///  Constructs an OperatorProperty with a given name and adds it to the parent data object under this name.
        /// </summary>
        /// <param name="parent">The parent data object to add the property to if applicable.</param>
        /// <param name="name">The property name that should be unique within the parent data object.</param>
        public OperatorProperty(DataObject parent, string name)
            : base(parent, name)
        {
            Change += OnValueChanged;
            if (name.EndsWith(CriteriaObject.Operator))
                AdditionalPropertyName = name.Substring(0, name.Length - 8);
            if (AdditionalPropertyName != null)
                AdditionalPropertyName2 = AdditionalPropertyName + CriteriaObject.V2;
            SortField = h => h[AttributeSortOrder];
            FilterFunc = IsApplicable;
            HasNullCheck = false;
        }

        /// <inheritdoc/>
        public override void CopyFrom(DataProperty p)
        {
            if (p is OperatorProperty op)
            {
                HasNullCheck = op.HasNullCheck;
            }
            base.CopyFrom(p);
        }

        /// <summary>
        /// Initializes the instances of the additional properties by the corresponding property names,
        /// which could be defaulted during current property construction and possibly overwritten
        /// during the parent object initialization before this method is called.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            if (AdditionalPropertyName != null)
                additionalProperty = parent[AdditionalPropertyName];
            if (AdditionalPropertyName2 != null)
                additionalProperty2 = parent[AdditionalPropertyName2];

            OnValueChanged(this, new PropertyChangeEventArgs(PropertyChange.All, null, null, null));
        }

        /// <summary>
        /// Overrides the base method to return the resource key of the associated property.
        /// </summary>
        protected override string ResourceKey => additionalProperty?.Name ?? base.ResourceKey;

        /// <summary>
        /// Determines if the given operator is applicable for the current additional properties
        /// by checking the first additional property type and whether or not it's multivalued
        /// and comparing it to the corresponding attributes of the given operator.
        /// This method is used as a filter function for the list of operators to display.
        /// </summary>
        /// <param name="oper">The operator to check.</param>
        /// <param name="row">The data row context, if any.</param>
        /// <returns>True if the given operator is applicable, otherwise false.</returns>
        public bool IsApplicable(Header oper, DataRow row = null)
        {
            object multiVal = oper[AttributeMultival];
            if (additionalProperty == null && multiVal != null || additionalProperty != null && (
                "0".Equals(multiVal) && additionalProperty.IsMultiValued ||
                "1".Equals(multiVal) && !additionalProperty.IsMultiValued)) return false;

            int.TryParse("" + oper[AttributeAddlProps], out int depCnt);
            if (depCnt > 0 && additionalProperty == null || depCnt > 1 && additionalProperty2 == null)
                return false;

            object nullCheck = oper[AttributeNullCheck];
            if ("1".Equals(nullCheck) && !HasNullCheck) return false;

            object type = oper[AttributeType];
            object exclType = oper[AttributeExcludeType];
            if (type == null && exclType == null) return true;
            if (additionalProperty == null) return false;

            Type propType = additionalProperty.GetType();

            // probe exclude types first
            if (exclType is IList exclTypes)
            {
                foreach (string s in exclTypes)
                    if (TypeMatches(propType, s)) return false;
            }
            else if (TypeMatches(propType, "" + exclType)) return false;

            // probe include types next
            if (type is IList types)
            {
                foreach (string s in types)
                    if (TypeMatches(propType, s)) return true;
                return false;
            }
            return TypeMatches(propType, "" + type);
        }

        /// <summary>
        /// Determines if the specified type or any of its base types match the provided name.
        /// </summary>
        /// <param name="t">The type to test.</param>
        /// <param name="name">The type name to match against.</param>
        /// <returns>True, if the type or any of its base type match the specified name.
        /// False otherwise.</returns>
        private bool TypeMatches(Type t, string name)
        {
            return (t.Name == name) || t.BaseType != null && TypeMatches(t.BaseType, name);
        }

        /// <summary>
        /// Updates the visibility and the Required flag of the additional properties
        /// based on the currently selected operator. Clears their values, if invisible.
        /// This method is triggered whenever the current operator changes.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">Property change event arguments.</param>
        protected void OnValueChanged(object sender, PropertyChangeEventArgs e)
        {
            if (!e.Change.IncludesValue() && !e.Change.IncludesVisible() || additionalProperty == null) return;
            int depCnt = 0;
            if (!IsNull()) int.TryParse("" + Value[AttributeAddlProps], out depCnt);
            additionalProperty.Visible = Visible && depCnt > 0;
            additionalProperty.Required = additionalProperty.Visible;
            if (!additionalProperty.Visible)
                additionalProperty.SetValue(null);
            if (additionalProperty2 != null)
            {
                additionalProperty2.Visible = Visible && depCnt > 1;
                additionalProperty2.Required = additionalProperty2.Visible;
                if (!additionalProperty2.Visible)
                    additionalProperty2.SetValue(null);
            }
        }
    }
}
