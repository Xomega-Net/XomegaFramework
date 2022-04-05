// Copyright (c) 2021 Xomega.Net. All rights reserved.

using System;
using System.Linq.Expressions;

namespace Xomega.Framework.Operators
{
    /// <summary>
    /// Operators for whether a property value is less than or equal to the specified value.
    /// </summary>
    public class LessThanOrEqualOperator : Operator
    {
        /// <summary>
        /// Constructs a Less Than Or Equal operator.
        /// </summary>
        public LessThanOrEqualOperator() : base(1, false)
        {
        }

        /// <inheritdoc/>
        public override string[] GetNames() => new [] { "LE", "LessEq", "LessOrEqual", "LessThanOrEqual", "EarlierOrAt" };

        /// <inheritdoc/>
        protected override Expression BuildExpression<TElement, TValue>(
                Expression<Func<TElement, TValue>> prop, params Expression<Func<TValue>>[] vals)
            => Expression.LessThanOrEqual(prop.Body, vals[0].Body);

        /// <inheritdoc/>
        protected override bool Match(object value, params object[] criteria)
            => (value is IComparable c) && c.CompareTo(criteria[0]) <= 0;
    }
}
