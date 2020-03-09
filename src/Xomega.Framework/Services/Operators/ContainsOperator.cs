// Copyright (c) 2020 Xomega.Net. All rights reserved.

using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Xomega.Framework.Services
{
    /// <summary>
    /// Operators for wheter or not a property value contains a substring.
    /// </summary>
    public class ContainsOperator : Operator
    {
        private static readonly MethodInfo Contains = typeof(string).GetMethod("Contains", new[] { typeof(string) });

        /// <summary>
        /// Constructs a Contains operator.
        /// </summary>
        public ContainsOperator() : this(false)
        {
        }

        /// <summary>
        /// Constructs an Contains or Does Not Contain operator based on the specified parameter.
        /// </summary>
        /// <param name="negate">True for Does Not Contain operator, False otherwise.</param>
        public ContainsOperator(bool negate) : base(1, negate)
        {
        }

        /// <summary>
        /// Gets all known names and aliases for the current operator.
        /// </summary>
        /// <returns>An array of operator names.</returns>
        public override string[] GetNames()
        {
            return Negate ? new [] { "NCN", "NotCont", "NotContains" }
                          : new [] { "CN", "Cont", "Contains" };
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
            return Expression.Call(prop.Body, Contains, vals[0].Body);
        }
    }
}
