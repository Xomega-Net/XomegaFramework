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
    }
}