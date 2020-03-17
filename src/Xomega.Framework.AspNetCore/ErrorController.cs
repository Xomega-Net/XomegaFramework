// Copyright (c) 2020 Xomega.Net. All rights reserved.

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Xomega.Framework.Services
{
    public class ErrorController : BaseController
    {
        public const string DefaultPath = "/error";

        public ErrorController(ErrorList errorList, ErrorParser errorParser) : base(errorList, errorParser)
        {
        }

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