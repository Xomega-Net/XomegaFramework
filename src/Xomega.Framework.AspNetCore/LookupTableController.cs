// Copyright (c) 2020 Xomega.Net. All rights reserved.

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Xomega.Framework.Lookup;

namespace Xomega.Framework.Services
{
    public class LookupTableController : BaseController
    {
        private readonly LookupCache globalCache;

        public LookupTableController(ErrorList errorList, ErrorParser errorParser, ILookupCacheProvider cacheProvider)
            : base(errorList, errorParser)
        {
            if (cacheProvider == null) throw new ArgumentNullException(nameof(cacheProvider));
            globalCache = cacheProvider.GetLookupCache(LookupCache.Global);
        }

        [Route("lookup-table/{id}")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetLookupTableAsync(string id)
        {
            LookupTable tbl = await globalCache.GetLookupTableAsync(id);
            if (tbl == null)
            {
                currentErrors.AddError(ErrorType.Data, AspNetCore.Messages.LookupTableNotFound, id);
                return StatusCode((int)currentErrors.HttpStatus, new Output(currentErrors));
            }
            else
            {
                var output = new Output<LookupTable>(currentErrors, tbl);
                return StatusCode((int)output.HttpStatus, output);
            }
        }

        [Route("lookup-table/{id}")]
        [HttpDelete]
        public void DeleteLookupTable(string id)
        {
            globalCache.RemoveLookupTable(id);
        }
    }
}