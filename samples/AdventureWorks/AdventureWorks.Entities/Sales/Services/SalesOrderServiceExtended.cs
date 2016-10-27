using AdventureWorks.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using Xomega.Framework;

namespace AdventureWorks.Entities.Services
{
    public partial class SalesOrderService
    {
        // add methods that can be used in the inline custom code of the generated service

        protected PaymentInfo GetPaymentInfo(SalesOrder obj)
        {
            PaymentInfo res = new PaymentInfo()
            {
                CreditCardApprovalCode = obj.CreditCardApprovalCode,
                DueDate = obj.DueDate,
                SubTotal = obj.SubTotal,
                Freight = obj.Freight,
                TaxAmt = obj.TaxAmt,
                TotalDue = obj.TotalDue,
            };
            if (obj.CreditCardIdObject != null)
                res.CreditCardId = obj.CreditCardIdObject.CreditCardId;
            if (obj.ShipMethodIdObject != null)
                res.ShipMethodId = obj.ShipMethodIdObject.ShipMethodId;
            if (obj.CurrencyRateIdObject != null)
                res.CurrencyRate = "$1 = " + obj.CurrencyRateIdObject.EndOfDayRate 
                                     + " " + obj.CurrencyRateIdObject.ToCurrencyCodeObject.Name;
            return res;
        }

        protected void UpdatePayment(AdventureWorksEntities ctx, SalesOrder obj, PaymentUpdate _data)
        {
            obj.DueDate = _data.DueDate;
            obj.CreditCardApprovalCode = _data.CreditCardApprovalCode;
            obj.ShipMethodIdObject = ctx.ShipMethod.Find(_data.ShipMethodId);
            if (obj.ShipMethodIdObject == null)
                ErrorList.Current.AddError("Cannot find ShipMethod with ID {0}.", _data.ShipMethodId);
            obj.CreditCardIdObject = ctx.CreditCard.Find(_data.CreditCardId);
            if (_data.CreditCardId != null && obj.CreditCardIdObject == null)
                ErrorList.Current.AddError("Cannot find CreditCard with ID {0}.", _data.CreditCardId);
        }

        protected SalesInfo GetSalesInfo(SalesOrder obj)
        {
            SalesInfo res = new SalesInfo();
            if (obj.SalesPersonIdObject != null)
                res.SalesPersonId = obj.SalesPersonIdObject.BusinessEntityId;
            if (obj.TerritoryIdObject != null)
                res.TerritoryId = obj.TerritoryIdObject.TerritoryId;
            // select a list of sales reason IDs from the child list
            res.SalesReason = obj.ReasonObjectList.Select(r => r.SalesReasonId).ToList();
            return res;
        }

        protected void UpdateSales(AdventureWorksEntities ctx, SalesOrder obj, SalesInfo _data)
        {
            obj.TerritoryIdObject = ctx.SalesTerritory.Find(_data.TerritoryId);
            if (_data.TerritoryId != null && obj.TerritoryIdObject == null)
                ErrorList.Current.AddError("Cannot find Sales Territory with ID {0}.", _data.TerritoryId);
            obj.SalesPersonIdObject = ctx.SalesPerson.Find(_data.SalesPersonId);
            if (_data.SalesPersonId != null && obj.SalesPersonIdObject == null)
                ErrorList.Current.AddError("Cannot find Sales Person with ID {0}.", _data.SalesPersonId);
            // remove sales reason that are not in the provided list
            obj.ReasonObjectList.Where(r => _data.SalesReason == null || !_data.SalesReason.Contains(r.SalesReasonId))
                .ToList().ForEach(r => obj.ReasonObjectList.Remove(r));
            if (_data.SalesReason != null)
            {
                // add sales reasons from provided list that don't exist yet
                _data.SalesReason.Where(rId => obj.ReasonObjectList.Where(r => r.SalesReasonId == rId).ToList().Count == 0)
                    .ToList().ForEach(rId => obj.ReasonObjectList.Add(new SalesOrderReason()
                    {
                        SalesOrderObject = obj,
                        SalesReasonId = rId,
                        ModifiedDate = DateTime.Now
                    }));
            }
        }
    }
}