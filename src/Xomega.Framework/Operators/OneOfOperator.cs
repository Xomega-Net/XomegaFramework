// Copyright (c) 2024 Xomega.Net. All rights reserved.

using System;
using System.Linq;
using System.Linq.Expressions;

namespace Xomega.Framework.Operators
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

        /// <inheritdoc/>
        public override string[] GetNames() =>
            Negate ? new [] { "NoneOf", "NotIn", "NIn" }
                   : new [] { "OneOf", DefaultName};

        /// <inheritdoc/>
        protected override Expression BuildExpression<TElement, TValue>(
                Expression<Func<TElement, TValue>> prop, params Expression<Func<TValue>>[] vals)
            => vals.Length == 0 ? Expression.Constant(true) :
                 vals.Select(v => Expression.Equal(prop.Body, v.Body))
                     .Aggregate<Expression>((a, b) => Expression.Or(a, b));

        /// <inheritdoc/>
        protected override bool Match(object value, params object[] criteria)
            => criteria.Length == 0 || criteria.Contains(value);
    }
}
