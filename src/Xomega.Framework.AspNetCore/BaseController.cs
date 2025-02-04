// Copyright (c) 2025 Xomega.Net. All rights reserved.

using Microsoft.AspNetCore.Mvc;

namespace Xomega.Framework.Services
{
    /// <summary>
    /// Base class for controllers that use Xomega Framework
    /// </summary>
    /// <param name="errorList">An error list for the current errors.</param>
    /// <param name="errorParser">An injected error parser.</param>
    public class BaseController(ErrorList errorList, ErrorParser errorParser) : ControllerBase
    {
        /// <summary>
        /// Errors for the current request.
        /// </summary>
        protected readonly ErrorList currentErrors = errorList;

        /// <summary>
        /// Error parser for handling exceptions.
        /// </summary>
        protected readonly ErrorParser errorsParser = errorParser;
    }
}
