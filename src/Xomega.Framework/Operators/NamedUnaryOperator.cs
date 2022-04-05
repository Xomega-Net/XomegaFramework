// Copyright (c) 2021 Xomega.Net. All rights reserved.

using System;
using System.Linq.Expressions;

namespace Xomega.Framework.Operators
{
    /// <summary>
    /// Operators for whether or not a property value is in the specified range.
    /// </summary>
    public abstract class NamedUnaryOperator : Operator
    {
        // The single name of the operator
        private string name;

        /// <summary>
        /// Constructor for the named unary operator.
        /// </summary>
        /// <param name="name">The name of the operator.</param>
        public NamedUnaryOperator(string name) : base(0, false)
        {
            this.name = name;
        }

        /// <inheritdoc/>
        public override string[] GetNames() => new[] { name };

        /// <inheritdoc/>
        protected override Expression BuildExpression<TElement, TValue>(Expression<Func<TElement, TValue>> prop, params Expression<Func<TValue>>[] vals)
            => BuildUnaryExpression(prop);

        /// <summary>
        /// Builds the base predicate expression for the current operator using the specified property accessor.
        /// </summary>
        /// <typeparam name="TElement">The type of the element.</typeparam> 
        /// <typeparam name="TValue">The type of the values.</typeparam> 
        /// <param name="prop">Expression for the property accessor.</param>
        /// <returns>The base predicate expression for the specified property and values accessors.</returns>
        protected abstract Expression BuildUnaryExpression<TElement, TValue>(Expression<Func<TElement, TValue>> prop);
    }
}
