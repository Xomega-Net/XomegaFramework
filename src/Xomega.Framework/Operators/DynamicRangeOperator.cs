// Copyright (c) 2022 Xomega.Net. All rights reserved.

using System;
using System.Linq.Expressions;

namespace Xomega.Framework.Operators
{
    /// <summary>
    /// Dynamically constructed operator for filtering a property by the specified range,
    /// which can be open-bounded, and include or exclude the lower or upper bounds.
    /// </summary>
    /// <typeparam name="T">The property value type that should allow Less Than and Greater Than operators.</typeparam>
    public class DynamicRangeOperator<T> : NamedUnaryOperator
    {
        /// <summary>
        /// The lower bound of the range.
        /// </summary>
        protected T from;

        /// <summary>
        /// Whether to include, exclude or disregard (null) the lower bound.
        /// </summary>
        protected bool? includeFrom;

        /// <summary>
        /// The upper bound of the range.
        /// </summary>
        protected T to;

        /// <summary>
        /// Whether to include, exclude or disregard (null) the upper bound.
        /// </summary>
        protected bool? includeTo;

        /// <summary>
        /// Constructs a dynamic range operator for the specified range.
        /// </summary>
        /// <param name="name">The name of the operator.</param>
        /// <param name="from">The lower bound of the range.</param>
        /// <param name="includeFrom">Whether to include, exclude or disregard (null) the lower bound.</param>
        /// <param name="to">The upper bound of the range.</param>
        /// <param name="includeTo">Whether to include, exclude or disregard (null) the upper bound.</param>
        public DynamicRangeOperator(string name, T from, bool? includeFrom, T to, bool? includeTo) : base(name)
        {
            this.from = from;
            this.includeFrom = includeFrom;
            this.to = to;
            this.includeTo = includeTo;
        }

        /// <inheritdoc/>
        protected override Expression BuildUnaryExpression<TElement, TValue>(Expression<Func<TElement, TValue>> prop)
        {
            // create new expressions to access members that hold the values
            // in order to have the queries generated with parameters rather than hardcoded values
            Expression lower = null;
            if (includeFrom != null)
            {
                Expression<Func<T>> eFrom = () => from;
                lower = includeFrom.Value ? Expression.GreaterThanOrEqual(prop.Body, eFrom.Body) : Expression.GreaterThan(prop.Body, eFrom.Body);
            }
            Expression upper = null;
            if (includeTo != null)
            {
                Expression<Func<T>> eTo = () => to;
                upper = includeTo.Value ? Expression.LessThanOrEqual(prop.Body, eTo.Body) : Expression.LessThan(prop.Body, eTo.Body);
            }
            return (lower == null && upper == null) ? Expression.Constant(true) :
                                    (lower == null) ? upper : 
                                    (upper == null) ? lower : Expression.AndAlso(lower, upper);
        }

        /// <inheritdoc/>
        protected override bool Match(object value, params object[] criteria)
        {
            if (!(value is IComparable cv)) return false;
            return (includeFrom == null || cv.CompareTo(from) > 0 || includeFrom.Value && Equals(value, from))
                && (includeTo == null || cv.CompareTo(to) < 0 || includeTo.Value && Equals(value, to));
        }
    }
}
