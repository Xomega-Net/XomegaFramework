// Copyright (c) 2024 Xomega.Net. All rights reserved.

using System;
using System.Linq;
using System.Linq.Expressions;

namespace Xomega.Framework.Operators
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

        /// <inheritdoc/>
        public override string[] GetNames() =>
            Negate ? new [] { "NBW", "NotBetween" }
                   : new [] { "BW", DefaultName };

        /// <inheritdoc/>
        protected override Expression BuildExpression<TElement, TValue>(
            Expression<Func<TElement, TValue>> prop, params Expression<Func<TValue>>[] vals)
        {
            TValue[] values = vals.Select(e => e.Compile()()).ToArray();
            Expression lower = Expression.GreaterThanOrEqual(prop.Body, vals[0].Body);
            Expression upper = Expression.LessThanOrEqual(prop.Body, vals[1].Body);
            return (values[0] == null) ? upper : (values[1] == null) ? lower : Expression.AndAlso(lower, upper);
        }

        /// <inheritdoc/>
        protected override bool Match(object value, params object[] criteria)
            => (value is IComparable c) && c.CompareTo(criteria[0]) >= 0 && c.CompareTo(criteria[1]) <= 0;
    }
}
