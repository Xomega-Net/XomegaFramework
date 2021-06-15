using AdventureWorks.Services;
using System;
using System.Linq.Expressions;
using Xomega.Framework;
using Xomega.Framework.Properties;

namespace AdventureWorks.Client.Objects
{
    public class SalesOrderDetailListCustomized : SalesOrderDetailList
    {
        public SalesOrderDetailListCustomized()
        {
        }

        public SalesOrderDetailListCustomized(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        // construct properties and child objects
        protected override void Initialize()
        {
            base.Initialize();
            // add custom construction code here
        }

        // perform post intialization
        protected override void OnInitialized()
        {
            base.OnInitialized();

            ProductIdProperty.Editable = true;
            ProductIdProperty.IsKey = true;
            OrderQtyProperty.Editable = true;
            CarrierTrackingNumberProperty.Editable = true;

            SpecialOfferIdProperty.Editable = true;
            SpecialOfferIdProperty.LocalCacheLoader = new SpecialOfferProductReadListCacheLoader(ServiceProvider);
            SpecialOfferIdProperty.SetCacheLoaderParameters(Enumerations.SpecialOfferProduct.Parameters.ProductId, ProductIdProperty);

            Expression<Func<EnumIntProperty, DataRow, object>> xPrice = (prod, row) =>
                prod.IsNull(row) ? null : prod.GetValue(row)[Enumerations.Product.Attributes.ListPrice];
            UnitPriceProperty.SetComputedValue(xPrice, ProductIdProperty);

            Expression<Func<EnumProperty, DataRow, object>> xDiscount = (spOf, row) =>
                spOf.IsNull(row) ? null : spOf.GetValue(row)[Enumerations.SpecialOfferProduct.Attributes.Discount];
            UnitPriceDiscountProperty.SetComputedValue(xDiscount, SpecialOfferIdProperty);

            Expression<Func<MoneyProperty, PercentFractionProperty, SmallIntegerProperty, DataRow, decimal>> xLineTotal = 
                (price, discount, qty, row) => GetLineTotal(price.GetValue(row), discount.GetValue(row), qty.GetValue(row));
            LineTotalProperty.SetComputedValue(xLineTotal, UnitPriceProperty, UnitPriceDiscountProperty, OrderQtyProperty);

            // defer setting up NewAction enabling conditions until the parent is set, which it depends upon
            PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(Parent) && Parent != null)
                {
                    Expression<Func<DataObject, bool>> xNewEnabled = obj => !obj.IsNew && obj.Editable;
                    NewAction.SetComputedEnabled(xNewEnabled, Parent);
                }
            };
        }

        private decimal GetLineTotal(decimal? price, decimal? discount, int? qty) =>
            (price ?? 0) * (1 - (discount ?? 0)) * (qty ?? 0);
    }
}