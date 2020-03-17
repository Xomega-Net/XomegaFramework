// Copyright (c) 2020 Xomega.Net. All rights reserved.

using Microsoft.AspNetCore.Mvc;

namespace Xomega.Framework.Services
{
    /// <summary>
    /// Current principal provider based on the current HTTP context.
    /// </summary>
    public class BaseController : ControllerBase
    {
        protected readonly ErrorList currentErrors;
        protected readonly ErrorParser errorsParser;

        public BaseController(ErrorList errorList, ErrorParser errorParser)
        {
            currentErrors = errorList;
            errorsParser = errorParser;
        }
    }
}
