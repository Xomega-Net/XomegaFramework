// Copyright (c) 2023 Xomega.Net. All rights reserved.

namespace Xomega.Framework.Properties
{
    /// <summary>
    /// A property that holds a decimal value represented as percent.
    /// </summary>
    public class PercentProperty : DecimalProperty
    {
        /// <summary>
        ///  Constructs a PercentProperty with a given name and adds it to the parent data object under this name.
        /// </summary>
        /// <param name="parent">The parent data object to add the property to if applicable.</param>
        /// <param name="name">The property name that should be unique within the parent data object.</param>
        public PercentProperty(DataObject parent, string name)
            : base(parent, name)
        {
            DisplayFormat = "P";
        }
    }

    /// <summary>
    /// A percent property that allows only fractions between 0 and 1 (100%).
    /// </summary>
    public class PercentFractionProperty : PercentProperty
    {
        /// <summary>
        ///  Constructs a PercentFractionProperty with a given name and adds it to the parent data object under this name.
        /// </summary>
        /// <param name="parent">The parent data object to add the property to if applicable.</param>
        /// <param name="name">The property name that should be unique within the parent data object.</param>
        public PercentFractionProperty(DataObject parent, string name)
            : base(parent, name)
        {
            MinimumValue = 0;
            MaximumValue = 1;
        }
    }
}
