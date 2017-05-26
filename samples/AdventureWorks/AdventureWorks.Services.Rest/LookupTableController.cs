using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using Xomega.Framework.Lookup;

namespace AdventureWorks.Services.Rest
{
    public class LookupTableController : ApiController
    {
        private LookupCache globalCache;

        public LookupTableController(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null) throw new ArgumentNullException("serviceProvider");
            globalCache = LookupCache.Get(serviceProvider, LookupCache.Global);
        }

        [Route("lookup-table/{id}")]
        [HttpGet]
        [AllowAnonymous]
        public HttpResponseMessage Get(string id)
        {
            LookupTable tbl = globalCache.GetLookupTable(id);
            var response = tbl == null ?
                Request.CreateResponse<string>(HttpStatusCode.NotFound,
                    string.Format("Lookup table '{0}' is not found in the global lookup cache.", id)) :
                Request.CreateResponse<LookupTable>(HttpStatusCode.OK, tbl);
            response.Headers.CacheControl = new CacheControlHeaderValue()
            {
                Public = true,
                MaxAge = new TimeSpan(0, 0, 30)
            };
            return response;
        }

        [Route("lookup-table/{id}")]
        [HttpDelete]
        public void Delete(string id)
        {
            globalCache.RemoveLookupTable(id);
        }
    }
}