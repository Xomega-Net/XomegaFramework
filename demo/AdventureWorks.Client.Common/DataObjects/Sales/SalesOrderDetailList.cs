//---------------------------------------------------------------------------------------------
// This file was AUTO-GENERATED by "Xomega Data Objects" Xomega.Net generator.
//
// Manual CHANGES to this file WILL BE LOST when the code is regenerated.
//---------------------------------------------------------------------------------------------

using AdventureWorks.Services.Common;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xomega.Framework;
using Xomega.Framework.Properties;
using Xomega.Framework.Services;

namespace AdventureWorks.Client.Common.DataObjects
{
    public partial class SalesOrderDetailList : DataListObject
    {
        #region Constants

        public const string CarrierTrackingNumber = "CarrierTrackingNumber";
        public const string LineTotal = "LineTotal";
        public const string OrderQty = "OrderQty";
        public const string ProductId = "ProductId";
        public const string SalesOrderDetailId = "SalesOrderDetailId";
        public const string SpecialOfferId = "SpecialOfferId";
        public const string UnitPrice = "UnitPrice";
        public const string UnitPriceDiscount = "UnitPriceDiscount";

        #endregion

        #region Properties

        public TextProperty CarrierTrackingNumberProperty { get; private set; }
        public MoneyProperty LineTotalProperty { get; private set; }
        public SmallIntegerProperty OrderQtyProperty { get; private set; }
        public EnumIntProperty ProductIdProperty { get; private set; }
        public IntegerKeyProperty SalesOrderDetailIdProperty { get; private set; }
        public EnumIntProperty SpecialOfferIdProperty { get; private set; }
        public MoneyProperty UnitPriceProperty { get; private set; }
        public PercentFractionProperty UnitPriceDiscountProperty { get; private set; }

        #endregion

        #region Actions

        public ActionProperty DetailsAction { get; private set; }
        public ActionProperty NewAction { get; private set; }

        #endregion

        #region Construction

        public SalesOrderDetailList()
        {
        }

        public SalesOrderDetailList(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        protected override void Initialize()
        {
            SalesOrderDetailIdProperty = new IntegerKeyProperty(this, SalesOrderDetailId)
            {
                Required = true,
                Editable = false,
                IsKey = true,
            };
            ProductIdProperty = new EnumIntProperty(this, ProductId)
            {
                Required = true,
                EnumType = "product",
                Editable = false,
            };
            OrderQtyProperty = new SmallIntegerProperty(this, OrderQty)
            {
                Required = true,
                Editable = false,
            };
            UnitPriceProperty = new MoneyProperty(this, UnitPrice)
            {
                Required = true,
                Editable = false,
            };
            UnitPriceDiscountProperty = new PercentFractionProperty(this, UnitPriceDiscount)
            {
                Required = true,
                Editable = false,
            };
            SpecialOfferIdProperty = new EnumIntProperty(this, SpecialOfferId)
            {
                Required = true,
                EnumType = "special offer",
                Editable = false,
            };
            LineTotalProperty = new MoneyProperty(this, LineTotal)
            {
                Required = true,
                Editable = false,
            };
            CarrierTrackingNumberProperty = new TextProperty(this, CarrierTrackingNumber)
            {
                Size = 25,
                Editable = false,
            };
            DetailsAction = new ActionProperty(this, "Details");
            NewAction = new ActionProperty(this, "New");
        }

        #endregion

        #region CRUD Operations

        protected override async Task<ErrorList> DoReadAsync(object options, CancellationToken token = default)
        {
            var res = new ErrorList();
            var output1 = await SalesOrder_Detail_ReadListAsync(options, 
                Parent == null ? default : (int)(Parent as SalesOrderObject).SalesOrderIdProperty.TransportValue, token);
            res.MergeWith(output1.Messages);
            return res;
        }

        #endregion

        #region Service Operations

        protected virtual async Task<Output<ICollection<SalesOrderDetail_ReadListOutput>>> SalesOrder_Detail_ReadListAsync(object options, int _salesOrderId, CancellationToken token = default)
        {
            using (var s = ServiceProvider.CreateScope())
            {
                var output = await s.ServiceProvider.GetService<ISalesOrderService>().Detail_ReadListAsync(_salesOrderId, token);

                await FromDataContractAsync(output?.Result, options, token);
                return output;
            }
        }

        #endregion
    }
}