// Copyright (c) 2024 Xomega.Net. All rights reserved.

using System;
using System.Linq.Expressions;

namespace Xomega.Framework.Operators
{
    /// <summary>
    /// Operators for a property to be not equal to the specified value.
    /// </summary>
    public class NotEqualToOperator : Operator
    {
        /// <summary>
        /// Constructs a Not Equal To operator.
        /// </summary>
        public NotEqualToOperator() : base(1, false)
        {
        }

        /// <inheritdoc/>
        public override string[] GetNames() => new [] { "!=", "<>", "NEQ", "IsNot", "NotEqual", "NotEquals" };

        /// <inheritdoc/>
        protected override Expression BuildExpression<TElement, TValue>(
                Expression<Func<TElement, TValue>> prop, params Expression<Func<TValue>>[] vals)
            => Expression.NotEqual(prop.Body, vals[0].Body);

        /// <inheritdoc/>
        protected override bool Match(object value, params object[] criteria) => !Equals(value, criteria[0]);
    }
}
