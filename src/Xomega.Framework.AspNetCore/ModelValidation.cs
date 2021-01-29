// Copyright (c) 2021 Xomega.Net. All rights reserved.

using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Xomega.Framework.Services
{
    /// <summary>
    /// Extension methods to handle model validation errors using Xomega Framework.
    /// </summary>
    public static class ModelValidation
    {
        /// <summary>
        /// Adds model validation errors from the model state to the list of current errors.
        /// </summary>
        /// <param name="currentErrors">The list of current errors to add any model validation errors to.</param>
        /// <param name="modelState">Model state that contains model validation errors.</param>
        public static void AddModelErrors(this ErrorList currentErrors, ModelStateDictionary modelState)
        {
            foreach (var ms in modelState)
            {
                foreach (var err in ms.Value.Errors)
                {
                    if (!string.IsNullOrEmpty(err.ErrorMessage))
                        currentErrors.AddValidationError(err.ErrorMessage);
                    else if (err.Exception != null)
                        currentErrors.AddValidationError(err.Exception.Message);
                }
            }
        }
    }
}