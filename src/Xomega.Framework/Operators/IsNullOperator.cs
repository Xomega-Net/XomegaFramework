// Copyright (c) 2022 Xomega.Net. All rights reserved.

using System;
using System.Linq.Expressions;

namespace Xomega.Framework.Operators
{
    /// <summary>
    /// Operator for checking if a property value is null.
    /// </summary>
    public class IsNullOperator : Operator
    {
        /// <summary>
        /// Constructs an Is Null operator.
        /// </summary>
        public IsNullOperator() : base(0, false)
        {
        }

        /// <inheritdoc/>
        public override string[] GetNames() => new [] { "NL", "Null", "IsNull" };

        /// <inheritdoc/>
        protected override Expression BuildExpression<TElement, TValue>(
                Expression<Func<TElement, TValue>> prop, params Expression<Func<TValue>>[] vals)
            => Expression.Equal(prop.Body, Expression.Constant(null));

        /// <inheritdoc/>
        protected override bool Match(object value, params object[] criteria) => value == null;
    }
}