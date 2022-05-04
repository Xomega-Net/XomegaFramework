// Copyright (c) 2021 Xomega.Net. All rights reserved.

using System;
using System.Linq.Expressions;

namespace Xomega.Framework.Operators
{
    /// <summary>
    /// Operator for checking if a property value is not null.
    /// </summary>
    public class IsNotNullOperator : Operator
    {
        /// <summary>
        /// Constructs an Is Not Null operator.
        /// </summary>
        public IsNotNullOperator() : base(0, false)
        {
        }

        /// <inheritdoc/>
        public override string[] GetNames() => new [] { "NNL", "NotNull", "IsNotNull" };

        /// <inheritdoc/>
        protected override Expression BuildExpression<TElement, TValue>(
                Expression<Func<TElement, TValue>> prop, params Expression<Func<TValue>>[] vals)
            => Expression.NotEqual(prop.Body, Expression.Constant(null));

        /// <inheritdoc/>
        protected override bool Match(object value, params object[] criteria) => value != null;
    }
}
