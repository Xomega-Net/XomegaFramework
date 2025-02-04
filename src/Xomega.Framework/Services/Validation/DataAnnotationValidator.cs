// Copyright (c) 2025 Xomega.Net. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Xomega.Framework.Services
{
    /// <summary>
    /// Utility class for validating objects using data annotation attributes.
    /// </summary>
    public class DataAnnotationValidator
    {
        /// <summary>
        /// Validates the specified objects and its child objects using data annotation validation attributes on their properties,
        /// and returns a list of validation results where the validation failed.
        /// </summary>
        /// <param name="serviceProvider">Service provider to use for the validation context.</param>
        /// <param name="obj">The object to validate.</param>
        /// <returns>A list of validation errors for any properties of the object or its child objects.</returns>
        public static IEnumerable<ValidationResult> GetValidationErrors(IServiceProvider serviceProvider, object obj)
        {
            List<ValidationResult> res = new List<ValidationResult>();
            if (obj == null) return res;

            // for collections validate each item individually
            IEnumerable list = obj as IEnumerable;
            if (list != null)
            {
                foreach (object item in list)
                    res.AddRange(GetValidationErrors(serviceProvider, item));
                return res;
            }

            // validate each property of the object
            var properties = TypeDescriptor.GetProperties(obj.GetType()).Cast<PropertyDescriptor>().Where(p => !p.IsReadOnly);
            foreach (var prop in properties)
            {
                var value = prop.GetValue(obj);
                var context = new ValidationContext(obj, serviceProvider, null)
                {
                    DisplayName = prop.DisplayName,
                    MemberName = prop.Name
                };
                // validate using annotated validation attributes
                var valAttributes = prop.Attributes.OfType<ValidationAttribute>();
                foreach (var valAttr in valAttributes)
                {
                    var valRes = valAttr.GetValidationResult(value, context);
                    if (valRes != ValidationResult.Success)
                       res.Add(valRes);
                }
                // validate any child properties
                res.AddRange(GetValidationErrors(serviceProvider, value));
            }
            return res;
        }
    }
}
