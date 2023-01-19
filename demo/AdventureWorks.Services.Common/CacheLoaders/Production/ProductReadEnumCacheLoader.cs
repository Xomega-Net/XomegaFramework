//---------------------------------------------------------------------------------------------
// This file was AUTO-GENERATED by "Lookup Cache Loaders" Xomega.Net generator.
//
// Manual CHANGES to this file WILL BE LOST when the code is regenerated.
//---------------------------------------------------------------------------------------------

using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xomega.Framework;
using Xomega.Framework.Lookup;
using Xomega.Framework.Services;

namespace AdventureWorks.Services.Common
{
    public partial class ProductReadEnumCacheLoader : LookupCacheLoader 
    {
        public ProductReadEnumCacheLoader(IServiceProvider serviceProvider)
            : base(serviceProvider, LookupCache.Global, true, "product")
        {
        }

        protected virtual async Task<Output<ICollection<Product_ReadEnumOutput>>> ReadEnumAsync(CancellationToken token = default)
        {
            using (var s = serviceProvider.CreateScope())
            {
                var svc = s.ServiceProvider.GetService<IProductService>();
                return await svc.ReadEnumAsync();
            }
        }

        protected override async Task LoadCacheAsync(string tableType, CacheUpdater updateCache, CancellationToken token = default)
        {
            Dictionary<string, Dictionary<string, Header>> data = new Dictionary<string, Dictionary<string, Header>>();
            var output = await ReadEnumAsync(token);
            if (output?.Messages != null)
                output.Messages.AbortIfHasErrors();
            else if (output?.Result == null) return;

            foreach (var row in output.Result)
            {
                string type = "product";

                if (!data.TryGetValue(type, out Dictionary<string, Header> tbl))
                {
                    data[type] = tbl = new Dictionary<string, Header>();
                }
                string id = "" + row.ProductId;
                if (!tbl.TryGetValue(id, out Header h))
                {
                    tbl[id] = h = new Header(type, id, row.Name);
                    h.IsActive = IsActive(row.IsActive);
                }
                h.AddToAttribute("product subcategory id", row.ProductSubcategoryId);
                h.AddToAttribute("product model id", row.ProductModelId);
                h.AddToAttribute("list price", row.ListPrice);
            }
            // if no data is returned we still need to update cache to mark it as loaded
            if (data.Count == 0) updateCache(new LookupTable(tableType, new List<Header>(), true));
            foreach (string type in data.Keys)
                updateCache(new LookupTable(type, data[type].Values, true));
        }
    }
}