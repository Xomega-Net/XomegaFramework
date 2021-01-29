// Copyright (c) 2021 Xomega.Net. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Resources;

namespace Xomega.Framework.Services
{
    /// <summary>
    /// Extended MaxLength attribute that validates maximum length for a single string or a list of strings,
    /// and uses Xomega.Framework messages and a dependency-injected resource manager.
    /// </summary>
    public class XMaxLengthAttribute : MaxLengthAttribute
    {
        /// <summary>
        /// Constructs a XMaxLengthAttribute with the specified maximum length.
        /// </summary>
        /// <param name="length">The maximum length to use.</param>
        public XMaxLengthAttribute(int length) : base(length) { }

        /// <summary>
        /// Validates the value of the memeber from the current <see cref="ValidationContext"/>
        /// by delegating it to the <see cref="IsValidList(ICollection, ValidationContext)"/> method.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        /// <param name="validationContext">The validation context to use.</param>
        /// <returns><see cref="ValidationResult.Success"/> if the value is valid, and a <see cref="ValidationResult"/> with the localized error otherwise.</returns>
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            return value is ICollection ? IsValidList((ICollection)value, validationContext) : IsValidList(new[] { value }, validationContext);
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
            IServiceProvider svcProvider = (validationContext?.GetService(typeof(ResourceManager)) != null) ? validationContext : DI.DefaultServiceProvider;
            ResourceManager rm = (ResourceManager)svcProvider?.GetService(typeof(ResourceManager)) ?? Messages.ResourceManager;
            List<string> invalidValues = new List<string>();
            foreach (object val in values)
            {
                if (!IsValid(val)) invalidValues.Add("'" + val + "'");
            }
            if (invalidValues.Count == 0) return ValidationResult.Success;
            string errMsg = invalidValues.Count > 1 ? Messages.Validation_MaxLengths : Messages.Validation_MaxLength;
            string[] memberNames = validationContext.MemberName != null ? new string[] { validationContext.MemberName } : null;
            return new ValidationResult(string.Format(rm.GetString(errMsg), validationContext.DisplayName, Length, string.Join(", ", invalidValues)), memberNames);
        }
    }
}
