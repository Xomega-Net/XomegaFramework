// Copyright (c) 2020 Xomega.Net. All rights reserved.

using System;
using System.Linq;
using System.Linq.Expressions;

namespace Xomega.Framework.Services
{
    /// <summary>
    /// Operators for equality based on a list of values.
    /// </summary>
    public class OneOfOperator : Operator
    {
        /// <summary>
        /// A default name for this operator.
        /// </summary>
        public const string DefaultName = "In";

        /// <summary>
        /// Constructs an operator for multi-value equality.
        /// </summary>
        public OneOfOperator() : this(false)
        {
        }

        /// <summary>
        /// Constructs an operator for inclusion or not inclusion based on the specified parameter.
        /// </summary>
        /// <param name="negate">True for Not In operator, False otherwise.</param>
        public OneOfOperator(bool negate) : base(-1, negate)
        {
        }

        /// <summary>
        /// Gets all known names and aliases for the current operator.
        /// </summary>
        /// <returns>An array of operator names.</returns>
        public override string[] GetNames()
        {
            return Negate ? new [] { "NoneOf", "NotIn", "NIn" }
                          : new [] { "OneOf", DefaultName};
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
            return vals.Length == 0 ? Expression.Constant(true) :
                vals.Select(v => Expression.Equal(prop.Body, v.Body))
                    .Aggregate<Expression>((a, b) => Expression.Or(a, b));
        }
    }
}
