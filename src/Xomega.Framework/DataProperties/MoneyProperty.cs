// Copyright (c) 2017 Xomega.Net. All rights reserved.

using System.Globalization;

namespace Xomega.Framework.Properties
{
    /// <summary>
    /// A decimal property that holds a monetary or currency value.
    /// </summary>
    public class MoneyProperty : DecimalProperty
    {
        /// <summary>
        ///  Constructs a MoneyProperty with a given name and adds it to the parent data object under this name.
        /// </summary>
        /// <param name="parent">The parent data object to add the property to if applicable.</param>
        /// <param name="name">The property name that should be unique within the parent data object.</param>
        public MoneyProperty(DataObject parent, string name)
            : base(parent, name)
        {
            ParseStyles = NumberStyles.Currency;
            DisplayFormat = "C";
        }
    }

    /// <summary>
    /// A money property for nonnegative amounts only.
    /// </summary>
    public class PositiveMoneyProperty : MoneyProperty
    {
        /// <summary>
        ///  Constructs a PositiveMoneyProperty with a given name and adds it to the parent data object under this name.
        /// </summary>
        /// <param name="parent">The parent data object to add the property to if applicable.</param>
        /// <param name="name">The property name that should be unique within the parent data object.</param>
        public PositiveMoneyProperty(DataObject parent, string name)
            : base(parent, name)
        {
            MinimumValue = 0;
        }
    }
}
