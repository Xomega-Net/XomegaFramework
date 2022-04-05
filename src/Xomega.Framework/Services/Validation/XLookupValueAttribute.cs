// Copyright (c) 2021 Xomega.Net. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Resources;
using Xomega.Framework.Lookup;

namespace Xomega.Framework.Services
{
    /// <summary>
    /// A validation attribute that validates a single value or a list of values against the specified lookup table,
    /// and uses Xomega.Framework messages and a dependency-injected resource manager.
    /// </summary>
    public class XLookupValueAttribute : ValidationAttribute
    {
        /// <summary>
        /// The name of the lookup table to validate the values against.
        /// </summary>
        public string LookupTable { get; private set; }

        /// <summary>
        /// The cache type to use. Defaults to <see cref="LookupCache.Global"/>
        /// </summary>
        public string CacheType { get; set; } = LookupCache.Global;

        /// <summary>
        /// The type of validation to perform. Defaults to <see cref="LookupValidationType.AnyItem"/>
        /// </summary>
        public LookupValidationType ValidationType { get; set; } = LookupValidationType.AnyItem;

        /// <summary>
        /// Constructs a XLookupValueAttribute for the specified lookup table.
        /// </summary>
        /// <param name="lookupTable">The name of the lookup table to validate the values against.</param>
        public XLookupValueAttribute(string lookupTable) : base(Messages.Validation_LookupValue)
        {
            LookupTable = lookupTable;
        }

        /// <summary>
        /// Validates the value of the member from the current <see cref="ValidationContext"/>
        /// by delegating it to the <see cref="IsValidList(ICollection, ValidationContext)"/> method.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        /// <param name="validationContext">The validation context to use.</param>
        /// <returns><see cref="ValidationResult.Success"/> if the value is valid, and a <see cref="ValidationResult"/> with the localized error otherwise.</returns>
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            return value is ICollection col ? IsValidList(col, validationContext) : IsValidList(new[] { value }, validationContext);
        }

        /// <summary>
        /// Validates the specified list of values using the current <see cref="ValidationContext"/>.
        /// </summary>
        /// <param name="values">The list of values to validate.</param>
        /// <param name="validationContext">The validation context to use.</param>
        /// <returns><see cref="ValidationResult.Success"/> if all the values in the list are valid,
        /// and a <see cref="ValidationResult"/> with the localized error otherwise.</returns>
        protected ValidationResult IsValidList(ICollection values, ValidationContext validationContext)
        {
            if (ValidationType == LookupValidationType.None) return ValidationResult.Success;

            IServiceProvider svcProvider = (validationContext?.GetService(typeof(ResourceManager)) != null) ? validationContext : DI.DefaultServiceProvider;
            ResourceManager rm = (ResourceManager)svcProvider?.GetService(typeof(ResourceManager)) ?? Messages.ResourceManager;
            LookupCache cache = LookupCache.Get(svcProvider, CacheType);
            LookupTable table = cache?.GetLookupTable(LookupTable);
            if (table == null) throw new InvalidOperationException(
                string.Format(rm.GetString(Messages.Validation_InvalidLookupTable), validationContext.DisplayName, LookupTable));
            string[] memberNames = validationContext.MemberName != null ? new string[] { validationContext.MemberName } : null;
            List<string> invalidValues = new List<string>();
            foreach(object val in values)
            {
                string strVal = val?.ToString();
                if (val is bool) strVal = strVal.ToLower();
                if (string.IsNullOrEmpty(strVal)) continue;
                Header h = table.LookupById(strVal);
                if (h == null || ValidationType == LookupValidationType.ActiveItem && !h.IsActive)
                    invalidValues.Add("'" + strVal + "'");
            }
            if (invalidValues.Count == 0) return ValidationResult.Success;
            string errMsg = invalidValues.Count > 1 ? Messages.Validation_LookupValues : Messages.Validation_LookupValue;
            return new ValidationResult(string.Format(rm.GetString(errMsg), validationContext.DisplayName, LookupTable, string.Join(", ", invalidValues)), memberNames);
        }
    }
}
