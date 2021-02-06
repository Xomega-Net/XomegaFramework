// Copyright (c) 2021 Xomega.Net. All rights reserved.

using System;
using System.Linq.Expressions;

namespace Xomega.Framework.Operators
{
    /// <summary>
    /// Operators for wheter a property value is greater than the specified value.
    /// </summary>
    public class GreaterThanOperator : Operator
    {
        /// <summary>
        /// Constructs a Greater Than operator.
        /// </summary>
        public GreaterThanOperator() : base(1, false)
        {
        }

        /// <inheritdoc/>
        public override string[] GetNames() => new [] { "GT", "Greater", "GreaterThan", "Later" };

        /// <inheritdoc/>
        protected override Expression BuildExpression<TElement, TValue>(
                Expression<Func<TElement, TValue>> prop, params Expression<Func<TValue>>[] vals)
            => Expression.GreaterThan(prop.Body, vals[0].Body);

        /// <inheritdoc/>
        protected override bool Match(object value, params object[] criteria)
            => (value is IComparable c) && c.CompareTo(criteria[0]) > 0;
    }
}
