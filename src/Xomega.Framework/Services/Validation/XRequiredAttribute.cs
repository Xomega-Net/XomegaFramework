// Copyright (c) 2021 Xomega.Net. All rights reserved.

using System;
using System.ComponentModel.DataAnnotations;
using System.Resources;

namespace Xomega.Framework.Services
{
    /// <summary>
    /// Extended Required attribute that uses a Xomega.Framework message and a dependency-injected resource manager.
    /// </summary>
    public class XRequiredAttribute : RequiredAttribute
    {
        /// <summary>
        /// Validates the value of the member from the current <see cref="ValidationContext"/>.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        /// <param name="validationContext">The validation context to use.</param>
        /// <returns><see cref="ValidationResult.Success"/> if the value is valid, and a <see cref="ValidationResult"/> with the localized error otherwise.</returns>
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (IsValid(value)) return ValidationResult.Success;
            IServiceProvider svcProvider = (validationContext?.GetService(typeof(ResourceManager)) != null) ? validationContext : DI.DefaultServiceProvider;
            ResourceManager rm = (ResourceManager)svcProvider?.GetService(typeof(ResourceManager)) ?? Messages.ResourceManager;
            string[] memberNames = validationContext.MemberName != null ? new string[] { validationContext.MemberName } : null;
            return new ValidationResult(string.Format(rm.GetString(Messages.Validation_Required), validationContext.DisplayName, value), memberNames);
        }
    }
}
