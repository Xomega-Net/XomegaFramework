// Copyright (c) 2021 Xomega.Net. All rights reserved.

using System;
using System.Linq.Expressions;

namespace Xomega.Framework.Services
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

        /// <summary>
        /// Builds the base predicate expression for the current operator using the specified property accessor.
        /// </summary>
        /// <typeparam name="TElement">The type of the element.</typeparam> 
        /// <typeparam name="TValue">The type of the values.</typeparam> 
        /// <param name="prop">Expression for the property accessor.</param>
        /// <returns>The base predicate expression for the specified property and values accessors.</returns>
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
    }
}
