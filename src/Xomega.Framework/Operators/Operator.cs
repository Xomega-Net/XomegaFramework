// Copyright (c) 2023 Xomega.Net. All rights reserved.

using System;
using System.Linq;
using System.Linq.Expressions;

namespace Xomega.Framework.Operators
{
    /// <summary>
    /// Base class for operators that help construct logical predicates for filtering.
    /// </summary>
    public abstract class Operator
    {
        /// <summary>
        /// Number of values the operator takes (0-2),
        /// or -1 for operators that take an unbounded list of values (e.g. IN).
        /// </summary>
        public int NumberOfValues { get; private set; }

        /// <summary>
        /// Whether the operator will negate the predicate.
        /// </summary>
        public bool Negate { get; private set; }

        /// <summary>
        /// Constructs an operator.
        /// </summary>
        /// <param name="numOfVals">The number of values the operator takes.</param>
        /// <param name="negate">True for operator negation, false otherwise.</param>
        protected Operator(int numOfVals, bool negate)
        {
            this.NumberOfValues = numOfVals;
            this.Negate = negate;
        }

        /// <summary>
        /// Gets all known names and aliases for the current operator.
        /// </summary>
        /// <returns>An array of operator names.</returns>
        public abstract string[] GetNames();

        /// <summary>
        /// Gets a predicate expression for the current operator
        /// applied to the specified property and values accessors.
        /// </summary>
        /// <typeparam name="TElement">The type of the element.</typeparam> 
        /// <typeparam name="TValue">The type of the values.</typeparam> 
        /// <param name="prop">Expression for the property accessor.</param>
        /// <param name="vals">The criteria value(s).</param>
        /// <returns>A predicate expression for the specified property and values accessors.</returns>
        public Expression<Func<TElement, bool>> GetPredicate<TElement, TValue>(
            Expression<Func<TElement, TValue>> prop, params TValue[] vals)
        {
            // validate values length upfront
            ValidateNumberOfValues(vals);

            // skip operator if all values are blank
            if (NumberOfValues != 0 && vals.Length > 0)
            {
                if (NumberOfValues > 0) vals = vals.Take(NumberOfValues).ToArray();
                if (vals.All(v => v == null)) return e => true;
            }

            // convert values to expressions for value accessors
            // to have the queries generated with parameters rather than hardcoded values
            Expression<Func<TValue>>[] eVals = vals.Select(v => (Expression<Func<TValue>>)(() => v)).ToArray();

            // build operator expression, and negate as needed
            Expression predicate = BuildExpression(prop, eVals);
            if (Negate) predicate = Expression.Not(predicate);
            return Expression.Lambda<Func<TElement, bool>>(predicate, prop.Parameters);
        }

        /// <summary>
        /// Builds the base predicate expression for the current operator
        /// using the specified property and values accessors.
        /// </summary>
        /// <typeparam name="TElement">The type of the element.</typeparam> 
        /// <typeparam name="TValue">The type of the values.</typeparam> 
        /// <param name="prop">Expression for the property accessor.</param>
        /// <param name="vals">Expressions for the value accessors.</param>
        /// <returns>The base predicate expression for the specified property and values accessors.</returns>
        protected abstract Expression BuildExpression<TElement, TValue>(
            Expression<Func<TElement, TValue>> prop, params Expression<Func<TValue>>[] vals);

        private void ValidateNumberOfValues<TValue>(TValue[] vals)
        {
            if (vals.Length < NumberOfValues) throw new ArgumentException(
                $"Operator {GetType().Name} expects {NumberOfValues} value(s), but only {vals.Length} were provided.");
        }

        /// <summary>
        /// Checks if the specified value matches the supplied criteria using the current comparison operator.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <param name="criteria">The criteria values to check against.</param>
        /// <returns>True if the given value matches the specified criteria, false otherwise.</returns>
        public bool Matches(object value, params object[] criteria)
        {
            ValidateNumberOfValues(criteria);
            bool match = Match(value, criteria);
            if (Negate) match = !match;
            return match;
        }

        /// <summary>
        /// Concrete implementation of the value matching logic for the operator by subclasses.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <param name="criteria">The criteria values to check against.</param>
        /// <returns>True if the given value matches the specified criteria, false otherwise.</returns>
        protected abstract bool Match(object value, params object[] criteria);
    }
}
