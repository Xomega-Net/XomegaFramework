// Copyright (c) 2017 Xomega.Net. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Collections;

namespace Xomega.Framework.Services
{
    /// <summary>
    /// Provides LINQ extension methods for IQueryable objects
    /// to allow filtering by multiple values using WhereIn and WhereNotIn constructs.
    /// </summary>
    public static class QueryableExtensions
    {
        /// <summary>
        /// Constructs a WhereIn expression for the given property selector and values.
        /// </summary>
        /// <typeparam name="TElement">The type of the element.</typeparam> 
        /// <typeparam name="TValue">The type of the values.</typeparam> 
        /// <param name="propertySelector">The property to be tested.</param> 
        /// <param name="values">The accepted values of the property.</param> 
        /// <returns>A WhereIn expression for the given property selector and values.</returns>
        private static Expression<Func<TElement, bool>> GetWhereInExpression<TElement, TValue>(Expression<Func<TElement, TValue>> propertySelector, IEnumerable<TValue> values)
        {
            ParameterExpression p = propertySelector.Parameters.Single();
            if (!values.Any()) return e => false;

            var equals = values.Select(value => (Expression)Expression.Equal(propertySelector.Body,
                Expression.Constant(value, typeof(TValue))));
            var body = equals.Aggregate<Expression>((accumulate, equal) => Expression.Or(accumulate, equal));

            return Expression.Lambda<Func<TElement, bool>>(body, p);
        }

        /// <summary> 
        /// Return the element that the specified property's value is contained in the specified values 
        /// </summary> 
        /// <typeparam name="TElement">The type of the element.</typeparam> 
        /// <typeparam name="TValue">The type of the values.</typeparam> 
        /// <param name="source">The source.</param> 
        /// <param name="propertySelector">The property to be tested.</param> 
        /// <param name="values">The accepted values of the property.</param> 
        /// <returns>The accepted elements.</returns> 
        public static IQueryable<TElement> WhereIn<TElement, TValue>(this IQueryable<TElement> source,
            Expression<Func<TElement, TValue>> propertySelector, params TValue[] values)
        {
            return source.Where(GetWhereInExpression(propertySelector, values));
        }

        /// <summary> 
        /// Return the element that the specified property's value is contained in the specified values 
        /// </summary> 
        /// <typeparam name="TElement">The type of the element.</typeparam> 
        /// <typeparam name="TValue">The type of the values.</typeparam> 
        /// <param name="source">The source.</param> 
        /// <param name="propertySelector">The property to be tested.</param> 
        /// <param name="values">The accepted values of the property.</param> 
        /// <returns>The accepted elements.</returns> 
        public static IQueryable<TElement> WhereIn<TElement, TValue>(this IQueryable<TElement> source,
            Expression<Func<TElement, TValue>> propertySelector, IEnumerable values)
        {
            return source.Where(GetWhereInExpression(propertySelector, values.Cast<TValue>()));
        }

        /// <summary> 
        /// Return the element that the specified property's value is not contained in the specified values 
        /// </summary> 
        /// <typeparam name="TElement">The type of the element.</typeparam> 
        /// <typeparam name="TValue">The type of the values.</typeparam> 
        /// <param name="source">The source.</param> 
        /// <param name="propertySelector">The property to be tested.</param> 
        /// <param name="values">The accepted values of the property.</param> 
        /// <returns>The accepted elements.</returns> 
        public static IQueryable<TElement> WhereNotIn<TElement, TValue>(this IQueryable<TElement> source,
            Expression<Func<TElement, TValue>> propertySelector, IEnumerable values)
        {
            Expression<Func<TElement, bool>> whereIn = GetWhereInExpression(propertySelector, values.Cast<TValue>());
            Expression<Func<TElement, bool>> whereNotIn = Expression.Lambda<Func<TElement, bool>>(
                Expression.Not(whereIn.Body), whereIn.Parameters.Single());
            return source.Where(whereNotIn);
        }
    }
}
