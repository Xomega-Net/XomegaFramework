// Copyright (c) 2020 Xomega.Net. All rights reserved.

using System;
using System.Linq.Expressions;

namespace Xomega.Framework.Services
{
    /// <summary>
    /// Operator for checking if a property value is not null .
    /// </summary>
    public class IsNotNullOperator : Operator
    {
        /// <summary>
        /// Constructs an Is Not Null operator.
        /// </summary>
        public IsNotNullOperator() : base(0, false)
        {
        }

        /// <summary>
        /// Gets all known names and aliases for the current operator.
        /// </summary>
        /// <returns>An array of operator names.</returns>
        public override string[] GetNames()
        {
            return new [] { "NNL", "NotNull", "IsNotNull" };
        }

        /// <summary>
        /// Builds the base predicate expression for the current operator
        /// using the specified property and values accessors.
        /// </summary>
        /// <typeparam name="TElement">The type of the element.</typeparam> 
        /// <typeparam name="TValue">The type of the values.</typeparam> 
        /// <param name="prop">Expression for the property accessor.</param>
        /// <param name="vals">Expressions for the value accessors.</param>
        /// <returns>The base predicate expression for the specified property and values accessors.</returns>
        protected override Expression BuildExpression<TElement, TValue>(
            Expression<Func<TElement, TValue>> prop, params Expression<Func<TValue>>[] vals)
        {
            return Expression.NotEqual(prop.Body, Expression.Constant(null));
        }
    }
}
