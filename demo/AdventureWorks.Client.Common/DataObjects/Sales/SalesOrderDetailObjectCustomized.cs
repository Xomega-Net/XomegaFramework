using System;
using System.Linq.Expressions;
using AdventureWorks.Services.Common;
using AdventureWorks.Services.Common.Enumerations;
using Xomega.Framework;
using Xomega.Framework.Properties;

namespace AdventureWorks.Client.Common.DataObjects
{
    public class SalesOrderDetailObjectCustomized : SalesOrderDetailObject
    {
        public SalesOrderDetailObjectCustomized()
        {
        }

        public SalesOrderDetailObjectCustomized(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        // construct properties and child objects
        protected override void Initialize()
        {
            base.Initialize();
            // add custom construction code here
        }

        // perform post initialization
        protected override void OnInitialized()
        {
            base.OnInitialized();

            ProductIdProperty.SetCascadingProperty(Product.Attributes.ProductSubcategoryId, SubcategoryProperty);
            // configure blank subcategory to display products with no categories
            ProductIdProperty.CascadingMatchNulls = true;

            SpecialOfferIdProperty.LocalCacheLoader = new SpecialOfferProductReadEnumCacheLoader(ServiceProvider);
            SpecialOfferIdProperty.SetCacheLoaderParameters(
                SpecialOfferProduct.Parameters.ProductId, ProductIdProperty);

            // computed property using the entire object
            Expression<Func<SalesOrderDetailObject, object>> xPrice = sod => sod.ProductIdProperty.IsNull(null) ?
                null : sod.ProductIdProperty.Value[Product.Attributes.ListPrice];
            UnitPriceProperty.SetComputedValue(xPrice, this);

            // computed property using individual property
            Expression<Func<EnumProperty, object>> xDiscount = spOf =>
                spOf.IsNull(null) ? null : spOf.Value[SpecialOfferProduct.Attributes.Discount];
            UnitPriceDiscountProperty.SetComputedValue(xDiscount, SpecialOfferIdProperty);

            // computed total using a helper function
            Expression<Func<SalesOrderDetailObject, decimal>> xLineTotal = sod => GetLineTotal(
                sod.UnitPriceProperty.Value, sod.UnitPriceDiscountProperty.Value, sod.OrderQtyProperty.Value);
            LineTotalProperty.SetComputedValue(xLineTotal, this);
        }

        private static decimal GetLineTotal(decimal? price, decimal? discount, short? qty) =>
            (price ?? 0) * (1 - (discount ?? 0)) * (qty ?? 0);
    }
}