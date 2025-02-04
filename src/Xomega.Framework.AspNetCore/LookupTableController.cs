// Copyright (c) 2025 Xomega.Net. All rights reserved.

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Xomega.Framework.Lookup;

namespace Xomega.Framework.Services
{
    /// <summary>
    /// Default controller for accessing globally cached lookup tables.
    /// </summary>
    public class LookupTableController : BaseController
    {
        private readonly LookupCache globalCache;

        /// <summary>
        /// Constructs a new lookup table controller.
        /// </summary>
        /// <param name="errorList">An error list for the current errors.</param>
        /// <param name="errorParser">An injected error parser.</param>
        /// <param name="cacheProvider">An injected instance of the cache provider.</param>
        public LookupTableController(ErrorList errorList, ErrorParser errorParser, ILookupCacheProvider cacheProvider)
            : base(errorList, errorParser)
        {
            if (cacheProvider == null) throw new ArgumentNullException(nameof(cacheProvider));
            globalCache = cacheProvider.GetLookupCache(LookupCache.Global);
        }

        /// <summary>
        /// Returns a lookup table with for the specified type.
        /// </summary>
        /// <param name="type">The type of lookup table to return.</param>
        /// <returns>Result with the lookup table data, or NotFound result with an error.</returns>
        [Route("lookup-table/{type}")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetLookupTableAsync(string type)
        {
            LookupTable tbl = await globalCache.GetLookupTableAsync(type);
            if (tbl == null)
            {
                currentErrors.AddError(ErrorType.Data, AspNetCore.Messages.LookupTableNotFound, type);
                return StatusCode((int)currentErrors.HttpStatus, new Output(currentErrors));
            }
            else
            {
                var output = new Output<LookupTable>(currentErrors, tbl);
                return StatusCode((int)output.HttpStatus, output);
            }
        }

        /// <summary>
        /// Clears lookup table for the specified type from the global cache,
        /// so that the cached table could be refreshed next time it is requested.
        /// </summary>
        /// <param name="type">The type of lookup table to clear.</param>
        [Route("lookup-table/{type}")]
        [HttpDelete]
        public void DeleteLookupTable(string type)
        {
            globalCache.RemoveLookupTable(type);
        }
    }
}