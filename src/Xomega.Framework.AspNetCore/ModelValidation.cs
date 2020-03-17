// Copyright (c) 2020 Xomega.Net. All rights reserved.

using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Xomega.Framework.Services
{
    public static class ModelValidation
    {
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