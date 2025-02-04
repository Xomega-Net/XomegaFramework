// Copyright (c) 2024 Xomega.Net. All rights reserved.

using System.Collections;
using System.Collections.Generic;
using Xomega.Framework.Operators;
using Xomega.Framework.Services;

namespace Xomega.Framework.Criteria
{
    /// <summary>
    /// A group of properties for a specific field criteria, which is part of a criteria object.
    /// </summary>
    public class CriteriaPropertyGroup
    {
        /// <summary>
        /// The name of the field this group is for.
        /// </summary>
        public string FieldName { get; set; }

        /// <summary>
        /// The operator property for the field.
        /// </summary>
        public DataProperty OperatorProperty { get; set; }

        /// <summary>
        /// The value property for the field.
        /// </summary>
        public DataProperty ValueProperty { get; set; }

        /// <summary>
        /// The second value property for the field, if needed.
        /// </summary>
        public DataProperty Value2Property { get; set; }

        /// <summary>
        /// A custom default operator for the field.
        /// </summary>
        public string DefaultOperator { get; set; }

        /// <summary>
        /// Constructs an empty property group.
        /// </summary>
        public CriteriaPropertyGroup()
        {
        }

        /// <summary>
        /// Determines if the criteria group has any values.
        /// </summary>
        /// <returns>True if either operator or the value are populated.</returns>
        public bool HasValue()
        {
            return OperatorProperty != null && !OperatorProperty.IsNull() || !ValueProperty.IsNull();
        }

        /// <summary>
        /// Clears all the values in the group.
        /// </summary>
        public void Reset()
        {
            OperatorProperty?.SetValue(null);
            ValueProperty?.SetValue(null);
            Value2Property?.SetValue(null);
        }

        /// <summary>
        /// Returns the default operator for the group. If custom operator is not set,
        /// it returns EQ for single value and In for multi-valued criteria.
        /// </summary>
        /// <returns></returns>
        public string GetDefaultOperator()
        {
            return DefaultOperator ?? (ValueProperty == null ? null :
                (ValueProperty.IsMultiValued ? OneOfOperator.DefaultName : EqualToOperator.DefaultName));
        }

        /// <summary>
        /// Converts the property group to a field criteria structure for data contracts.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public FieldCriteria<T> ToFieldCriteria<T>()
        {
            var res = new FieldCriteria<T>
            {
                Operator = OperatorProperty?.GetValue(ValueFormat.Transport)?.ToString() ?? GetDefaultOperator()
            };

            if (ValueProperty == null || ValueProperty.IsNull()) return res;

            List<T> val = new List<T>();
            if (ValueProperty.IsMultiValued)
            {
                if (ValueProperty.GetValue(ValueFormat.Transport) is IList lst)
                {
                    foreach (var v in lst)
                        val.Add((T)v);
                }
            }
            else
            {
                val.Add((T)ValueProperty.GetValue(ValueFormat.Transport));
                if (Value2Property != null && !Value2Property.IsNull())
                    val.Add((T)Value2Property.GetValue(ValueFormat.Transport));
            }
            res.Values = val?.ToArray();
            return res;
        }
    }
}
