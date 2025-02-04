// Copyright (c) 2025 Xomega.Net. All rights reserved.

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Xomega.Framework.Services
{
    /// <summary>
    /// Default controller for unhandled exceptions in REST services,
    /// which formats them and returns as Xomega <see cref="Output"/> structure.
    /// </summary>
    public class ErrorController : BaseController
    {
        /// <summary>
        /// Default path for the error controller.
        /// </summary>
        public const string DefaultPath = "/error";

        /// <summary>
        /// Constructs a new error controller with injected services.
        /// </summary>
        /// <param name="errorList">An error list for the current errors.</param>
        /// <param name="errorParser">An injected error parser.</param>
        public ErrorController(ErrorList errorList, ErrorParser errorParser) : base(errorList, errorParser)
        {
        }

        /// <summary>
        /// Outputs unhandled exception as an error list in the <see cref="Output"/> structure.
        /// </summary>
        /// <returns></returns>
        [Route(DefaultPath)]
        [AllowAnonymous]
        [HttpGet]
        public IActionResult OutputExceptionErrors()
        {
            Exception ex = HttpContext.Features.Get<IExceptionHandlerFeature>()?.Error;
            currentErrors.MergeWith(errorsParser.FromException(ex));
            return StatusCode((int)currentErrors.HttpStatus, new Output(currentErrors));
        }
    }
}