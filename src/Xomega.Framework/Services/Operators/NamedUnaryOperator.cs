// Copyright (c) 2020 Xomega.Net. All rights reserved.

using System;
using System.Linq.Expressions;

namespace Xomega.Framework.Services
{
    /// <summary>
    /// Operators for wheter or not a property value is in the specified range.
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

        /// <summary>
        /// Gets all known names and aliases for the current operator.
        /// </summary>
        /// <returns>An array of operator names.</returns>
        public override string[] GetNames() { return new[] { name }; }

        /// <summary>
        /// Builds the base predicate expression for the current operator
        /// using the specified property and values accessors.
        /// </summary>
        /// <typeparam name="TElement">The type of the element.</typeparam> 
        /// <typeparam name="TValue">The type of the values.</typeparam> 
        /// <param name="prop">Expression for the property accessor.</param>
        /// <param name="vals">Expressions for the value accessors.</param>
        /// <returns>The base predicate expression for the specified property and values accessors.</returns>
        protected override Expression BuildExpression<TElement, TValue>(Expression<Func<TElement, TValue>> prop, params Expression<Func<TValue>>[] vals)
        {
            return BuildUnaryExpression(prop);
        }

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
