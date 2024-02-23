// Copyright (c) 2024 Xomega.Net. All rights reserved.

using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Xomega.Framework.Operators
{
    /// <summary>
    /// Operators for whether or not a property value starts with a substring.
    /// </summary>
    public class StartsWithOperator : Operator
    {
        private static readonly MethodInfo StartsWith = typeof(string).GetMethod("StartsWith", new[] { typeof(string) });

        /// <summary>
        /// Constructs a StartsWith operator.
        /// </summary>
        public StartsWithOperator() : this(false)
        {
        }

        /// <summary>
        /// Constructs an StartsWith or Does Not Start With operator based on the specified parameter.
        /// </summary>
        /// <param name="negate">True for Does Not Contain operator, False otherwise.</param>
        public StartsWithOperator(bool negate) : base(1, negate)
        {
        }

        /// <inheritdoc/>
        public override string[] GetNames() =>
            Negate ? new [] { "NSW", "NotStart", "NotStartWith" }
                   : new [] { "SW", "Start", "StartsWith" };

        /// <inheritdoc/>
        protected override Expression BuildExpression<TElement, TValue>(
                Expression<Func<TElement, TValue>> prop, params Expression<Func<TValue>>[] vals)
            => Expression.Call(prop.Body, StartsWith, vals[0].Body);

        /// <inheritdoc/>
        protected override bool Match(object value, params object[] criteria)
            => Convert.ToString(value).StartsWith(Convert.ToString(criteria[0]));
    }
}
