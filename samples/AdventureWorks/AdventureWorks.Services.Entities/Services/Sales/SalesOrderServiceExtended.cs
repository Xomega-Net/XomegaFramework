using System;
using System.Linq;
using Xomega.Framework;

namespace AdventureWorks.Services.Entities
{
    public partial class SalesOrderService
    {
        // add methods that can be used in the inline custom code of the generated service

        protected PaymentInfo GetPaymentInfo(SalesOrder obj)
        {
            PaymentInfo res = new PaymentInfo()
            {
                CreditCard = new PaymentInfo_CreditCard
                {
                    CreditCardApprovalCode = obj.CreditCardApprovalCode
                },
                DueDate = obj.DueDate,
                SubTotal = obj.SubTotal,
                Freight = obj.Freight,
                TaxAmt = obj.TaxAmt,
                TotalDue = obj.TotalDue,
            };
            if (obj.CreditCardIdObject != null)
                res.CreditCard.CreditCardId = obj.CreditCardIdObject.CreditCardId;
            if (obj.ShipMethodIdObject != null)
                res.ShipMethodId = obj.ShipMethodIdObject.ShipMethodId;
            if (obj.CurrencyRateIdObject != null)
                res.CurrencyRate = "$1 = " + obj.CurrencyRateIdObject.EndOfDayRate
                                     + " " + obj.CurrencyRateIdObject.ToCurrencyCodeObject.Name;
            return res;
        }

        protected void UpdatePayment(SalesOrder obj, PaymentUpdate _data)
        {
            if (_data == null)
            {
                currentErrors.AddValidationError("Payment information is required.");
                return;
            }
            obj.DueDate = _data.DueDate;
            obj.ShipMethodIdObject = ctx.ShipMethod.Find(_data.ShipMethodId);
            if (obj.ShipMethodIdObject == null)
                currentErrors.AddError(ErrorType.Data, "Cannot find ShipMethod with ID {0}.", _data.ShipMethodId);
            if (_data.CreditCard != null)
            {
                obj.CreditCardApprovalCode = _data.CreditCard.CreditCardApprovalCode;
                obj.CreditCardIdObject = ctx.CreditCard.Find(_data.CreditCard.CreditCardId);
                if (obj.CreditCardIdObject == null)
                    currentErrors.AddError(ErrorType.Data, "Cannot find CreditCard with ID {0}.", _data.CreditCard.CreditCardId);
            }
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

        protected void UpdateSalesInfo(SalesOrder obj, SalesInfo _data)
        {
            if (_data == null)
            {
                currentErrors.AddValidationError("Sales information is required.");
                return;
            }
            obj.TerritoryIdObject = ctx.SalesTerritory.Find(_data.TerritoryId);
            if (_data.TerritoryId != null && obj.TerritoryIdObject == null)
                currentErrors.AddError(ErrorType.Data, "Cannot find Sales Territory with ID {0}.", _data.TerritoryId);
            obj.SalesPersonIdObject = ctx.SalesPerson.Find(_data.SalesPersonId);
            if (_data.SalesPersonId != null && obj.SalesPersonIdObject == null)
                currentErrors.AddError(ErrorType.Data, "Cannot find Sales Person with ID {0}.", _data.SalesPersonId);
            // remove sales reason that are not in the provided list
            obj.ReasonObjectList.Where(r => _data.SalesReason == null || !_data.SalesReason.Contains(r.SalesReasonId))
                .ToList().ForEach(r => obj.ReasonObjectList.Remove(r));
            if (_data.SalesReason != null)
            {
                // add sales reasons from provided list that don't exist yet
                _data.SalesReason.Where(rId => obj.ReasonObjectList.Where(r => r.SalesReasonId == rId).ToList().Count == 0)
                    .ToList().ForEach(rId => obj.ReasonObjectList.Add(new SalesOrderReason()
                    {
                        SalesOrderId = obj.SalesOrderId,
                        SalesReasonId = rId,
                        ModifiedDate = DateTime.Now
                    }));
            }
        }

        protected CustomerInfo GetCustomerInfo(SalesOrder obj)
        {
            CustomerInfo res = new CustomerInfo();
            if (obj.CustomerIdObject != null)
            {
                res.CustomerId = obj.CustomerIdObject.CustomerId;
                res.AccountNumber = obj.CustomerIdObject.AccountNumber;
                res.PersonId = obj.CustomerIdObject.PersonIdObject == null ? null :
                         (int?)obj.CustomerIdObject.PersonIdObject.BusinessEntityId;
                res.PersonName = obj.CustomerIdObject.PersonIdObject == null ? null :
                                 obj.CustomerIdObject.PersonIdObject.LastName + ", " +
                                 obj.CustomerIdObject.PersonIdObject.FirstName;
                res.StoreId = obj.CustomerIdObject.StoreIdObject == null ? null :
                        (int?)obj.CustomerIdObject.StoreIdObject.BusinessEntityId;
                res.StoreName = obj.CustomerIdObject.StoreIdObject == null ? null :
                                obj.CustomerIdObject.StoreIdObject.Name;
                res.TerritoryId = obj.CustomerIdObject.TerritoryIdObject == null ? null :
                            (int?)obj.CustomerIdObject.TerritoryIdObject.TerritoryId;
            };
            if (obj.BillToAddressIdObject != null)
                res.BillingAddress = new AddressKey { AddressId = obj.BillToAddressIdObject.AddressId };
            if (obj.ShipToAddressIdObject != null)
                res.ShippingAddress = new AddressKey { AddressId = obj.ShipToAddressIdObject.AddressId };
            return res;
        }

        protected void UpdateCustomer(SalesOrder obj, CustomerUpdate _data)
        {
            if (_data == null)
            {
                currentErrors.AddValidationError("Customer information is required.");
                return;
            }
            obj.CustomerIdObject = ctx.Customer.Find(_data.CustomerId);
            if (obj.CustomerIdObject == null)
                currentErrors.AddError(ErrorType.Data,
                    "Invalid value {0} for parameter CustomerId. Cannot find the corresponding Customer object.", _data.CustomerId);
            if (_data.BillingAddress != null)
            {
                obj.BillToAddressIdObject = ctx.Address.Find(_data.BillingAddress.AddressId);
                if (_data.BillingAddress.AddressId != 0 && obj.BillToAddressIdObject == null)
                    currentErrors.AddError(ErrorType.Data,
                        "Invalid value {0} for Bill-To AddressId. Cannot find the corresponding Address object.", _data.BillingAddress.AddressId);
            }
            if (_data.ShippingAddress != null)
            {
                obj.ShipToAddressIdObject = ctx.Address.Find(_data.ShippingAddress.AddressId);
                if (_data.ShippingAddress.AddressId != 0 && obj.ShipToAddressIdObject == null)
                    currentErrors.AddError(ErrorType.Data,
                        "Invalid value {0} for Ship-To AddressId. Cannot find the corresponding Address object.", _data.ShippingAddress.AddressId);
            }
        }
    }
}