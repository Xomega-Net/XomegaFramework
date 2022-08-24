// Copyright (c) 2022 Xomega.Net. All rights reserved.

using System;
using System.Linq.Expressions;

namespace Xomega.Framework.Operators
{
    /// <summary>
    /// Operators for whether a property value is greater than or equal to the specified value.
    /// </summary>
    public class GreaterThanOrEqualOperator : Operator
    {
        /// <summary>
        /// Constructs a Greater Than Or Equal operator.
        /// </summary>
        public GreaterThanOrEqualOperator() : base(1, false)
        {
        }

        /// <inheritdoc/>
        public override string[] GetNames() => new [] { "GE", "GreaterEq", "GreaterOrEqual", "GreaterThanOrEqual", "LaterOrAt" };

        /// <inheritdoc/>
        protected override Expression BuildExpression<TElement, TValue>(
                Expression<Func<TElement, TValue>> prop, params Expression<Func<TValue>>[] vals)
            => Expression.GreaterThanOrEqual(prop.Body, vals[0].Body);

        /// <inheritdoc/>
        protected override bool Match(object value, params object[] criteria)
            => (value is IComparable c) && c.CompareTo(criteria[0]) >= 0;
    }
}
