// Copyright (c) 2025 Xomega.Net. All rights reserved.

using System;
using System.Linq.Expressions;

namespace Xomega.Framework.Operators
{
    /// <summary>
    /// Operator for filtering properties by a single value.
    /// </summary>
    public class EqualToOperator : Operator
    {
        /// <summary>
        /// A default name for this operator.
        /// </summary>
        public const string DefaultName = "EQ";

        /// <summary>
        /// Constructs an Equal To operator.
        /// </summary>
        public EqualToOperator() : base(1, false)
        {
        }

        /// <inheritdoc/>
        public override string[] GetNames() => new [] { DefaultName, "=", "==", "Is", "Equal", "Equals" };

        /// <inheritdoc/>
        protected override Expression BuildExpression<TElement, TValue>(
                Expression<Func<TElement, TValue>> prop, params Expression<Func<TValue>>[] vals)
            => Expression.Equal(prop.Body, vals[0].Body);

        /// <inheritdoc/>
        protected override bool Match(object value, params object[] criteria) => Equals(value, criteria[0]);
    }
}
