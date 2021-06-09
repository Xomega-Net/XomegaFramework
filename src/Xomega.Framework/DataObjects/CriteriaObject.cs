// Copyright (c) 2021 Xomega.Net. All rights reserved.

using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Specialized;
using Xomega.Framework.Properties;
using System.Threading.Tasks;
using System.Threading;

namespace Xomega.Framework
{
    /// <summary>
    /// A data object that stores search criteria with operators
    /// </summary>
    public abstract class CriteriaObject : DataObject
    {
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
        /// Returns a list of current field criteria settings.
        /// </summary>
        public List<FieldCriteriaSetting> GetFieldCriteriaSettings()
        {
            // get a map of properties
            Dictionary<string, DataProperty> map = new Dictionary<string, DataProperty>();
            foreach (DataProperty p in Properties) map.Add(p.Name, p);

            // clear the map from properties that are associated with operators
            OperatorProperty op;
            foreach (DataProperty p in Properties)
            {
                if ((op = p as OperatorProperty) == null) continue;
                if (op.AdditionalPropertyName != null)
                    map.Remove(op.AdditionalPropertyName);
                if (op.AdditionalPropertyName2 != null)
                    map.Remove(op.AdditionalPropertyName2);
            }

            // export visible non-null settings
            List<FieldCriteriaSetting> res = new List<FieldCriteriaSetting>();
            foreach (DataProperty p in map.Values)
            {
                if (p.IsNull() || !p.Visible) continue;
                if ((op = p as OperatorProperty) != null)
                {
                    List<string> value = new List<string>();
                    foreach (var apn in new string[] { op.AdditionalPropertyName, op.AdditionalPropertyName2 })
                    {
                        DataProperty v = apn != null ? this[apn] : null;
                        if (v != null && !v.IsNull() && v.Visible)
                            value.Add(v.DisplayStringValue);
                    }
                    res.Add(new FieldCriteriaSetting
                    {
                        Label = p.Label,
                        Operator = op.DisplayStringValue,
                        Value = value.ToArray()
                    });
                }
                else res.Add(new FieldCriteriaSetting
                {
                    Label = p.Label,
                    Operator = null,
                    Value = new string[] { p.DisplayStringValue }
                });
            }
            return res;
        }
    }
}
