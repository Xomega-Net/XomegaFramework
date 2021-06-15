using AdventureWorks.Services;
using System;
using System.Linq.Expressions;
using Xomega.Framework;
using Xomega.Framework.Properties;

namespace AdventureWorks.Client.Objects
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

        // perform post intialization
        protected override void OnInitialized()
        {
            base.OnInitialized();

            ProductIdProperty.SetCascadingProperty(Enumerations.Product.Attributes.ProductSubcategoryId, SubcategoryProperty);
            ProductIdProperty.CascadingMatchNulls = true; // blank subcategory will display products with no categories

            SpecialOfferIdProperty.LocalCacheLoader = new SpecialOfferProductReadListCacheLoader(ServiceProvider);
            SpecialOfferIdProperty.SetCacheLoaderParameters(Enumerations.SpecialOfferProduct.Parameters.ProductId, ProductIdProperty);

            // computed property using the entire object
            Expression<Func<SalesOrderDetailObject, object>> xPrice = sod =>
                sod.ProductIdProperty.IsNull(null) ? null : sod.ProductIdProperty.Value[Enumerations.Product.Attributes.ListPrice];
            UnitPriceProperty.SetComputedValue(xPrice, this);

            // computed property using individual property
            Expression<Func<EnumProperty, object>> xDiscount = spOf =>
                spOf.IsNull(null) ? null : spOf.Value[Enumerations.SpecialOfferProduct.Attributes.Discount];
            UnitPriceDiscountProperty.SetComputedValue(xDiscount, SpecialOfferIdProperty);

            // computed visible attribute based on discount value
            Expression<Func<PercentFractionProperty, bool>> xDiscountVisible = dp => !dp.IsNull(null) && dp.Value > 0;
            UnitPriceDiscountProperty.SetComputedVisible(xDiscountVisible, UnitPriceDiscountProperty);

            // computed total using a helper function
            Expression<Func<SalesOrderDetailObject, decimal>> xLineTotal = sod =>
                GetLineTotal(sod.UnitPriceProperty.Value, sod.UnitPriceDiscountProperty.Value, sod.OrderQtyProperty.Value);
            LineTotalProperty.SetComputedValue(xLineTotal, this);

        }

        private decimal GetLineTotal(decimal? price, decimal? discount, int? qty) =>
            (price ?? 0) * (1 - (discount ?? 0)) * (qty ?? 0);

        private void UpdateLineTotal(object sender, PropertyChangeEventArgs e)
        {
            if (e.Change.IncludesValue())
            {
                LineTotalProperty.SetValue(GetLineTotal(UnitPriceProperty.Value,
                    UnitPriceDiscountProperty.Value, OrderQtyProperty.Value));
            }
        }
    }
}