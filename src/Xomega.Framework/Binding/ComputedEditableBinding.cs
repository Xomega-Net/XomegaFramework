// Copyright (c) 2024 Xomega.Net. All rights reserved.

using System;
using System.Linq.Expressions;

namespace Xomega.Framework
{
    /// <summary>
    /// Computed binding for updating property editability.
    /// </summary>
    public class ComputedEditableBinding : ComputedBinding
    {
        /// <summary>
        /// Constructs a new computed binding for updating property editability.
        /// </summary>
        /// <param name="property">The property to update based on the computed result.</param>
        /// <param name="expression">Lambda expression used to compute the result.</param>
        /// <param name="args">Arguments for the specified expression to use for evaluation.</param>
        public ComputedEditableBinding(BaseProperty property, LambdaExpression expression, params object[] args)
            : base(property, expression, args)
        {
            if (property == null) throw new ArgumentException("Property cannot be null", nameof(property));
            if (expression.ReturnType != typeof(bool))
                throw new Exception("Supplied expression should return a bool.");
        }

        /// <inheritdoc/>
        public override void Update(DataRow row)
        {
            var computed = (bool)GetComputedValue(row);
            property.SetEditable(computed, row);
        }
    }
}
