// Copyright (c) 2022 Xomega.Net. All rights reserved.

using System;
using System.Linq.Expressions;

namespace Xomega.Framework.Operators
{
    /// <summary>
    /// Operators for whether a property value is less than the specified value.
    /// </summary>
    public class LessThanOperator : Operator
    {
        /// <summary>
        /// Constructs a Less Than operator.
        /// </summary>
        public LessThanOperator() : base(1, false)
        {
        }

        /// <inheritdoc/>
        public override string[] GetNames() => new [] { "LT", "Less", "LessThan", "Earlier" };

        /// <inheritdoc/>
        protected override Expression BuildExpression<TElement, TValue>(
                Expression<Func<TElement, TValue>> prop, params Expression<Func<TValue>>[] vals)
            => Expression.LessThan(prop.Body, vals[0].Body);

        /// <inheritdoc/>
        protected override bool Match(object value, params object[] criteria)
            => (value is IComparable c) && c.CompareTo(criteria[0]) < 0;
    }
}
