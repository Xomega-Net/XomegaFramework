// Copyright (c) 2020 Xomega.Net. All rights reserved.

using System;
using System.Linq;
using System.Linq.Expressions;

namespace Xomega.Framework.Services
{
    /// <summary>
    /// Operators for whether or not a property value is in the specified range.
    /// </summary>
    public class BetweenOperator : Operator
    {
        /// <summary>
        /// A default name for this operator.
        /// </summary>
        public const string DefaultName = "Between";

        /// <summary>
        /// Constructs a Between operator.
        /// </summary>
        public BetweenOperator() : this(false)
        {
        }

        /// <summary>
        /// Constructs a Between or Not Between operator based on the specified parameter.
        /// </summary>
        /// <param name="negate">True for Not Between operator, False otherwise.</param>
        public BetweenOperator(bool negate) : base(2, negate)
        {
        }

        /// <summary>
        /// Gets all known names and aliases for the current operator.
        /// </summary>
        /// <returns>An array of operator names.</returns>
        public override string[] GetNames()
        {
            return Negate ? new [] { "NBW", "NotBetween" }
                          : new [] { "BW", DefaultName };
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
            TValue[] values = vals.Select(e => e.Compile()()).ToArray();
            Expression lower = Expression.GreaterThanOrEqual(prop.Body, vals[0].Body);
            Expression upper = Expression.LessThanOrEqual(prop.Body, vals[1].Body);
            return (values[0] == null) ? upper : (values[1] == null) ? lower : Expression.AndAlso(lower, upper);
        }
    }
}
