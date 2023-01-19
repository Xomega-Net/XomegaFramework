//---------------------------------------------------------------------------------------------
// This file was AUTO-GENERATED by "Service Implementations" Xomega.Net generator.
//
// Manual CHANGES to this file WILL BE LOST when the code is regenerated
// unless they are placed between corresponding CUSTOM_CODE_START/CUSTOM_CODE_END lines.
//---------------------------------------------------------------------------------------------

using AdventureWorks.Services.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xomega.Framework.Services;
// CUSTOM_CODE_START: add namespaces for custom code below
// CUSTOM_CODE_END

namespace AdventureWorks.Services.Entities
{
    public partial class BusinessEntityService : BaseService, IBusinessEntityService
    {
        protected AdventureWorksEntities ctx;

        public BusinessEntityService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            ctx = serviceProvider.GetService<AdventureWorksEntities>();
        }

        public virtual async Task<Output<ICollection<BusinessEntityAddress_ReadEnumOutput>>> Address_ReadEnumAsync(int _businessEntityId, CancellationToken token = default)
        {
            ICollection<BusinessEntityAddress_ReadEnumOutput> res = null;
            try
            {
                currentErrors.AbortIfHasErrors();

                // CUSTOM_CODE_START: add custom security checks for Address_ReadEnum operation below
                // CUSTOM_CODE_END
                var src = from obj in ctx.BusinessEntityAddress select obj;

                // Source filter
                src = AddClause(src, "BusinessEntityId", o => o.BusinessEntityId, _businessEntityId);

                // CUSTOM_CODE_START: add custom filter criteria to the source query for Address_ReadEnum operation below
                // src = src.Where(o => o.FieldName == VALUE);
                // CUSTOM_CODE_END

                var qry = from obj in src
                          select new BusinessEntityAddress_ReadEnumOutput() {
                              AddressId = obj.AddressId,
                              // CUSTOM_CODE_START: set the AddressType output parameter of Address_ReadEnum operation below
                              AddressType = obj.AddressTypeObject.Name, // CUSTOM_CODE_END
                              // CUSTOM_CODE_START: set the AddressLine1 output parameter of Address_ReadEnum operation below
                              AddressLine1 = obj.AddressObject.AddressLine1, // CUSTOM_CODE_END
                              // CUSTOM_CODE_START: set the AddressLine2 output parameter of Address_ReadEnum operation below
                              AddressLine2 = obj.AddressObject.AddressLine2, // CUSTOM_CODE_END
                              // CUSTOM_CODE_START: set the City output parameter of Address_ReadEnum operation below
                              City = obj.AddressObject.City, // CUSTOM_CODE_END
                              // CUSTOM_CODE_START: set the State output parameter of Address_ReadEnum operation below
                              State = obj.AddressObject.StateProvinceObject.StateProvinceCode, // CUSTOM_CODE_END
                              // CUSTOM_CODE_START: set the PostalCode output parameter of Address_ReadEnum operation below
                              PostalCode = obj.AddressObject.PostalCode, // CUSTOM_CODE_END
                              // CUSTOM_CODE_START: set the Country output parameter of Address_ReadEnum operation below
                              Country = obj.AddressObject.StateProvinceObject.CountryRegionCode, // CUSTOM_CODE_END
                          };

                // CUSTOM_CODE_START: add custom filter criteria to the result query for Address_ReadEnum operation below
                // qry = qry.Where(o => o.FieldName == VALUE);
                // CUSTOM_CODE_END

                currentErrors.AbortIfHasErrors();
                res = await qry.ToListAsync(token);
            }
            catch (Exception ex)
            {
                currentErrors.MergeWith(errorParser.FromException(ex));
            }
            return await Task.FromResult(new Output<ICollection<BusinessEntityAddress_ReadEnumOutput>>(currentErrors, res));
        }
    }
}