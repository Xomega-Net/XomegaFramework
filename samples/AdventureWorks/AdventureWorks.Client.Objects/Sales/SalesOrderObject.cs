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
    public partial class SalesOrderObject : DataObject
    {
        #region Constants

        public const string AccountNumber = "AccountNumber";
        public const string Comment = "Comment";
        public const string Customer = "Customer";
        public const string Detail = "Detail";
        public const string ModifiedDate = "ModifiedDate";
        public const string OnlineOrderFlag = "OnlineOrderFlag";
        public const string OrderDate = "OrderDate";
        public const string Payment = "Payment";
        public const string PurchaseOrderNumber = "PurchaseOrderNumber";
        public const string RevisionNumber = "RevisionNumber";
        public const string Sales = "Sales";
        public const string SalesOrderId = "SalesOrderId";
        public const string SalesOrderNumber = "SalesOrderNumber";
        public const string ShipDate = "ShipDate";
        public const string Status = "Status";

        #endregion

        #region Properties

        public TextProperty AccountNumberProperty { get; private set; }
        public TextProperty CommentProperty { get; private set; }
        public DateTimeProperty ModifiedDateProperty { get; private set; }
        public BooleanProperty OnlineOrderFlagProperty { get; private set; }
        public DateProperty OrderDateProperty { get; private set; }
        public TextProperty PurchaseOrderNumberProperty { get; private set; }
        public TinyIntegerProperty RevisionNumberProperty { get; private set; }
        public IntegerKeyProperty SalesOrderIdProperty { get; private set; }
        public TextProperty SalesOrderNumberProperty { get; private set; }
        public DateProperty ShipDateProperty { get; private set; }
        public EnumByteProperty StatusProperty { get; private set; }

        #endregion

        #region Child Objects

        public SalesOrderCustomerObjectCustomized CustomerObject { get { return (SalesOrderCustomerObjectCustomized)GetChildObject(Customer); } }
        public SalesOrderDetailList DetailList { get { return (SalesOrderDetailList)GetChildObject(Detail); } }
        public SalesOrderPaymentObject PaymentObject { get { return (SalesOrderPaymentObject)GetChildObject(Payment); } }
        public SalesOrderSalesObjectCustomized SalesObject { get { return (SalesOrderSalesObjectCustomized)GetChildObject(Sales); } }

        #endregion

        #region Construction

        protected override void Initialize()
        {
            AccountNumberProperty = new TextProperty(this, AccountNumber);
            AccountNumberProperty.Size = 15;
            CommentProperty = new TextProperty(this, Comment);
            CommentProperty.Size = 128;
            ModifiedDateProperty = new DateTimeProperty(this, ModifiedDate);
            ModifiedDateProperty.Required = true;
            ModifiedDateProperty.Editable = false;
            OnlineOrderFlagProperty = new BooleanProperty(this, OnlineOrderFlag);
            OnlineOrderFlagProperty.Required = true;
            OrderDateProperty = new DateProperty(this, OrderDate);
            OrderDateProperty.Required = true;
            OrderDateProperty.Editable = false;
            PurchaseOrderNumberProperty = new TextProperty(this, PurchaseOrderNumber);
            PurchaseOrderNumberProperty.Size = 25;
            RevisionNumberProperty = new TinyIntegerProperty(this, RevisionNumber);
            RevisionNumberProperty.Required = true;
            RevisionNumberProperty.Editable = false;
            SalesOrderIdProperty = new IntegerKeyProperty(this, SalesOrderId);
            SalesOrderIdProperty.Required = true;
            SalesOrderIdProperty.Editable = false;
            SalesOrderNumberProperty = new TextProperty(this, SalesOrderNumber);
            SalesOrderNumberProperty.Required = true;
            SalesOrderNumberProperty.Size = 25;
            SalesOrderNumberProperty.Editable = false;
            ShipDateProperty = new DateProperty(this, ShipDate);
            StatusProperty = new EnumByteProperty(this, Status);
            StatusProperty.Required = true;
            StatusProperty.Size = 10;
            StatusProperty.EnumType = "sales order status";
            DataObject objCustomer = new SalesOrderCustomerObjectCustomized();
            AddChildObject(Customer, objCustomer);
            DataObject objDetail = new SalesOrderDetailList();
            AddChildObject(Detail, objDetail);
            DataObject objPayment = new SalesOrderPaymentObject();
            AddChildObject(Payment, objPayment);
            DataObject objSales = new SalesOrderSalesObjectCustomized();
            AddChildObject(Sales, objSales);
        }

        #endregion
    }

}