// Copyright (c) 2025 Xomega.Net. All rights reserved.

using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Xomega.Framework.Operators
{
    /// <summary>
    /// Operators for whether or not a property value contains a substring.
    /// </summary>
    public class ContainsOperator : Operator
    {
        private static readonly MethodInfo Contains = typeof(string).GetMethod("Contains", new[] { typeof(string) });

        /// <summary>
        /// Constructs a Contains operator.
        /// </summary>
        public ContainsOperator() : this(false)
        {
        }

        /// <summary>
        /// Constructs an Contains or Does Not Contain operator based on the specified parameter.
        /// </summary>
        /// <param name="negate">True for Does Not Contain operator, False otherwise.</param>
        public ContainsOperator(bool negate) : base(1, negate)
        {
        }

        /// <inheritdoc/>
        public override string[] GetNames() =>
            Negate ? new [] { "NCN", "NotCont", "NotContains" }
                   : new [] { "CN", "Cont", "Contains" };

        /// <inheritdoc/>
        protected override Expression BuildExpression<TElement, TValue>(
                Expression<Func<TElement, TValue>> prop, params Expression<Func<TValue>>[] vals)
            => Expression.Call(prop.Body, Contains, vals[0].Body);

        /// <inheritdoc/>
        protected override bool Match(object value, params object[] criteria)
            => Convert.ToString(value).Contains(Convert.ToString(criteria[0]));
    }
}