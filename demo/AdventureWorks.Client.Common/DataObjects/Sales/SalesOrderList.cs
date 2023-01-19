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
using Xomega.Framework.Lookup;
using Xomega.Framework.Properties;
using Xomega.Framework.Services;

namespace AdventureWorks.Client.Common.DataObjects
{
    public partial class SalesOrderList : DataListObject
    {
        #region Constants

        public const string CustomerName = "CustomerName";
        public const string CustomerStore = "CustomerStore";
        public const string DueDate = "DueDate";
        public const string OnlineOrderFlag = "OnlineOrderFlag";
        public const string OrderDate = "OrderDate";
        public const string SalesOrderId = "SalesOrderId";
        public const string SalesOrderNumber = "SalesOrderNumber";
        public const string SalesPersonId = "SalesPersonId";
        public const string ShipDate = "ShipDate";
        public const string Status = "Status";
        public const string TerritoryId = "TerritoryId";
        public const string TotalDue = "TotalDue";

        #endregion

        #region Properties

        public TextProperty CustomerNameProperty { get; private set; }
        public TextProperty CustomerStoreProperty { get; private set; }
        public DateProperty DueDateProperty { get; private set; }
        public EnumBoolProperty OnlineOrderFlagProperty { get; private set; }
        public DateProperty OrderDateProperty { get; private set; }
        public IntegerKeyProperty SalesOrderIdProperty { get; private set; }
        public TextProperty SalesOrderNumberProperty { get; private set; }
        public EnumIntProperty SalesPersonIdProperty { get; private set; }
        public DateProperty ShipDateProperty { get; private set; }
        public EnumByteProperty StatusProperty { get; private set; }
        public EnumIntProperty TerritoryIdProperty { get; private set; }
        public MoneyProperty TotalDueProperty { get; private set; }

        #endregion

        #region Actions

        public ActionProperty DetailsAction { get; private set; }
        public ActionProperty NewAction { get; private set; }

        #endregion

        #region Construction

        public SalesOrderList()
        {
        }

        public SalesOrderList(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        protected override void Initialize()
        {
            SalesOrderIdProperty = new IntegerKeyProperty(this, SalesOrderId)
            {
                Required = true,
                Editable = false,
                IsKey = true,
            };
            SalesOrderNumberProperty = new TextProperty(this, SalesOrderNumber)
            {
                Required = true,
                Size = 25,
                Editable = false,
            };
            OrderDateProperty = new DateProperty(this, OrderDate)
            {
                Required = true,
                Editable = false,
            };
            DueDateProperty = new DateProperty(this, DueDate)
            {
                Required = true,
                Editable = false,
            };
            ShipDateProperty = new DateProperty(this, ShipDate)
            {
                Editable = false,
            };
            StatusProperty = new EnumByteProperty(this, Status)
            {
                Required = true,
                EnumType = "sales order status",
                Editable = false,
            };
            OnlineOrderFlagProperty = new EnumBoolProperty(this, OnlineOrderFlag)
            {
                Required = true,
                EnumType = "yesno",
                LookupValidation = LookupValidationType.None,
                Editable = false,
            };
            CustomerStoreProperty = new TextProperty(this, CustomerStore)
            {
                Editable = false,
            };
            CustomerNameProperty = new TextProperty(this, CustomerName)
            {
                Editable = false,
            };
            SalesPersonIdProperty = new EnumIntProperty(this, SalesPersonId)
            {
                EnumType = "sales person",
                Editable = false,
            };
            TerritoryIdProperty = new EnumIntProperty(this, TerritoryId)
            {
                EnumType = "sales territory",
                Editable = false,
            };
            TotalDueProperty = new MoneyProperty(this, TotalDue)
            {
                Required = true,
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
            var output1 = await SalesOrder_ReadListAsync(options, 
                CriteriaObject?.ToDataContract<SalesOrder_ReadListInput_Criteria>(options), token);
            res.MergeWith(output1.Messages);
            return res;
        }

        #endregion

        #region Service Operations

        protected virtual async Task<Output<ICollection<SalesOrder_ReadListOutput>>> SalesOrder_ReadListAsync(object options, SalesOrder_ReadListInput_Criteria _criteria, CancellationToken token = default)
        {
            using (var s = ServiceProvider.CreateScope())
            {
                var output = await s.ServiceProvider.GetService<ISalesOrderService>().ReadListAsync(_criteria, token);

                await FromDataContractAsync(output?.Result, options, token);
                return output;
            }
        }

        #endregion
    }
}