//---------------------------------------------------------------------------------------------
// This file was AUTO-GENERATED by "Xomega Data Objects" Xomega.Net generator.
//
// Manual CHANGES to this file WILL BE LOST when the code is regenerated.
//---------------------------------------------------------------------------------------------

using AdventureWorks.Client.Objects;
using AdventureWorks.Enumerations;
using AdventureWorks.Services;
using System;
using System.Data.Spatial;
using Xomega.Framework;
using Xomega.Framework.Properties;

namespace AdventureWorks.Client.Objects
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
        public IntegerKeyProperty SalesPersonIdProperty { get; private set; }
        public DateProperty ShipDateProperty { get; private set; }
        public EnumByteProperty StatusProperty { get; private set; }
        public IntegerKeyProperty TerritoryIdProperty { get; private set; }
        public MoneyProperty TotalDueProperty { get; private set; }

        #endregion

        #region Construction

        protected override void Initialize()
        {
            CustomerNameProperty = new TextProperty(this, CustomerName);
            CustomerNameProperty.Editable = false;
            CustomerStoreProperty = new TextProperty(this, CustomerStore);
            CustomerStoreProperty.Editable = false;
            DueDateProperty = new DateProperty(this, DueDate);
            DueDateProperty.Required = true;
            DueDateProperty.Editable = false;
            OnlineOrderFlagProperty = new EnumBoolProperty(this, OnlineOrderFlag);
            OnlineOrderFlagProperty.Required = true;
            OnlineOrderFlagProperty.Size = 10;
            OnlineOrderFlagProperty.EnumType = "yesno";
            OnlineOrderFlagProperty.Editable = false;
            OrderDateProperty = new DateProperty(this, OrderDate);
            OrderDateProperty.Required = true;
            OrderDateProperty.Editable = false;
            SalesOrderIdProperty = new IntegerKeyProperty(this, SalesOrderId);
            SalesOrderIdProperty.Required = true;
            SalesOrderIdProperty.Editable = false;
            SalesOrderNumberProperty = new TextProperty(this, SalesOrderNumber);
            SalesOrderNumberProperty.Required = true;
            SalesOrderNumberProperty.Size = 25;
            SalesOrderNumberProperty.Editable = false;
            SalesPersonIdProperty = new IntegerKeyProperty(this, SalesPersonId);
            SalesPersonIdProperty.Editable = false;
            ShipDateProperty = new DateProperty(this, ShipDate);
            ShipDateProperty.Editable = false;
            StatusProperty = new EnumByteProperty(this, Status);
            StatusProperty.Required = true;
            StatusProperty.Size = 10;
            StatusProperty.EnumType = "sales order status";
            StatusProperty.Editable = false;
            TerritoryIdProperty = new IntegerKeyProperty(this, TerritoryId);
            TerritoryIdProperty.Editable = false;
            TotalDueProperty = new MoneyProperty(this, TotalDue);
            TotalDueProperty.Required = true;
            TotalDueProperty.Editable = false;
        }

        #endregion
    }

}