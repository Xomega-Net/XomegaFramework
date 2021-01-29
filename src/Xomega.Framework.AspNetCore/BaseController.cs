// Copyright (c) 2021 Xomega.Net. All rights reserved.

using Microsoft.AspNetCore.Mvc;

namespace Xomega.Framework.Services
{
    /// <summary>
    /// Base class for controllers that use Xomega Framework
    /// </summary>
    public class BaseController : ControllerBase
    {
        /// <summary>
        /// Errors for the current request.
        /// </summary>
        protected readonly ErrorList currentErrors;

        /// <summary>
        /// Error parser for handling exceptions.
        /// </summary>
        protected readonly ErrorParser errorsParser;

        /// <summary>
        /// Constructs a Xomega-based controller
        /// </summary>
        /// <param name="errorList">An error list for the current errors.</param>
        /// <param name="errorParser">An injected error parser.</param>
        public BaseController(ErrorList errorList, ErrorParser errorParser)
        {
            currentErrors = errorList;
            errorsParser = errorParser;
        }
    }
}
