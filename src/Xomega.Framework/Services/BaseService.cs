// Copyright (c) 2025 Xomega.Net. All rights reserved.

using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Xomega.Framework.Operators;

namespace Xomega.Framework.Services
{
    /// <summary>
    /// A base class for service implementation classes that use Xomega Framework.
    /// </summary>
    public class BaseService : IPrincipalProvider
    {
        /// <summary>
        /// Service provider for this service
        /// </summary>
        protected IServiceProvider serviceProvider;

        /// <summary>
        /// Error list for the current operation
        /// </summary>
        protected ErrorList currentErrors;

        /// <summary>
        /// Error parser to use for handling errors and exceptions
        /// </summary>
        protected ErrorParser errorParser;

        /// <summary>
        /// The principal for the current operation
        /// </summary>
        private IPrincipal currentPrincipal;

        /// <summary>
        /// The default maximum number of rows to return in a query.
        /// </summary>
        protected int? defaultMaxRows = 1000;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public BaseService(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            currentErrors = serviceProvider.GetRequiredService<ErrorList>();
            errorParser = serviceProvider.GetRequiredService<ErrorParser>();
            operators = serviceProvider.GetService<OperatorRegistry>() ?? new OperatorRegistry();
            currentPrincipal = serviceProvider.GetCurrentPrincipal();
        }

        /// <summary>
        /// The principal for the current operation
        /// </summary>
        public IPrincipal CurrentPrincipal
        {
            get { return currentPrincipal ?? Thread.CurrentPrincipal; }
            set { currentPrincipal = value; }
        }

        #region Query building

        /// <summary>
        /// The registry to look up operators.
        /// </summary>
        private OperatorRegistry operators;

        /// <summary>
        /// Looks up operator in the current operator registry by the name and value type.
        /// This logic can be overridden in subclasses.
        /// </summary>
        /// <param name="name">Operator name to look up by.</param>
        /// <param name="valueType">The value type for the operator.</param>
        /// <returns>The operator found, or null if no operator found by the given name.</returns>
        protected virtual Operator GetOperator(string name, Type valueType)
        {
            return operators?.GetOperator(name, valueType);
        }

        /// <summary>
        /// Utility function to get a property name, possibly extracting from the property expression.
        /// </summary>
        private string GetPropertyName(string propName, Expression prop)
        {
            if (!string.IsNullOrEmpty(propName)) return propName;
            if (prop is MemberExpression me) return me.Member.Name;
            if (prop != null) return prop.ToString();
            return null;
        }

        /// <summary>
        /// Adds a Where clause to the given query for the specified property, and a field criteria structure.
        /// </summary>
        /// <typeparam name="TElement">The type of the element.</typeparam> 
        /// <typeparam name="TValue">The type of the values.</typeparam> 
        /// <param name="qry">The query to add the Where clause to.</param>
        /// <param name="propName">Property display name.</param>
        /// <param name="prop">Expression for the property accessor.</param>
        /// <param name="criteria">A field criteria structure with the operator and criteria values.</param>
        /// <returns>The query with the Where clause added, or the same query if input is invalid.</returns>
        protected virtual IQueryable<TElement> AddCriteriaClause<TElement, TValue>(IQueryable<TElement> qry, string propName,
            Expression<Func<TElement, TValue>> prop, FieldCriteria<TValue> criteria)
        {
            return AddClause(qry, propName, prop, criteria?.Operator, criteria?.Values ?? new TValue[] { } );
        }

        /// <summary>
        /// Adds a Where clause to the given query for the specified property, and a list of operators that take no values.
        /// If any of the specified operators are not valid, then adds validation errors and returns the same query.
        /// </summary>
        /// <typeparam name="TElement">The type of the element.</typeparam> 
        /// <typeparam name="TValue">The type of the values.</typeparam> 
        /// <param name="qry">The query to add the Where clause to.</param>
        /// <param name="propName">Property display name.</param>
        /// <param name="prop">Expression for the property accessor.</param>
        /// <param name="operators">A list of operator names or aliases.</param>
        /// <returns>The query with the Where clause added, or the same query if input is invalid.</returns>
        protected virtual IQueryable<TElement> AddClause<TElement, TValue>(IQueryable<TElement> qry, string propName,
            Expression<Func<TElement, TValue>> prop, ICollection<string> operators)
        {
            if (operators == null || operators.Count == 0) return qry; // assume no criteria, do nothing
            var expressions = new List<Expression>();
            foreach (string oper in operators)
            {
                Operator op = GetOperator(oper, typeof(TValue));
                if (op == null)
                    currentErrors.AddValidationError(Messages.Operator_NotSupported, oper, GetPropertyName(propName, prop.Body));
                else expressions.Add(op.GetPredicate(prop).Body);
            }
            if (expressions.Count < operators.Count) return qry; // some operators are invalid
            Expression agg = expressions.Aggregate((a, b) => Expression.Or(a, b));
            return qry.Where(Expression.Lambda<Func<TElement, bool>>(agg, prop.Parameters));
        }

        /// <summary>
        /// Adds a Where clause to the given query for the specified property, operator and values.
        /// If specified operator and values are not valid, then adds validation errors and returns the same query.
        /// </summary>
        /// <typeparam name="TElement">The type of the element.</typeparam> 
        /// <typeparam name="TValue">The type of the values.</typeparam> 
        /// <param name="qry">The query to add the Where clause to.</param>
        /// <param name="propName">Property display name.</param>
        /// <param name="prop">Expression for the property accessor.</param>
        /// <param name="oper">Operator name or alias.</param>
        /// <param name="vals">The criteria value(s).</param>
        /// <returns>The query with the Where clause added, or the same query if input is invalid.</returns>
        protected virtual IQueryable<TElement> AddClause<TElement, TValue>(IQueryable<TElement> qry, string propName,
            Expression<Func<TElement, TValue>> prop, string oper, params TValue[] vals)
        {
            if (oper == null || vals == null) return qry; // assume no criteria, do nothing
            Operator op = GetOperator(oper, typeof(TValue));
            if (op == null)
                currentErrors.AddValidationError(Messages.Operator_NotSupported, oper, GetPropertyName(propName, prop.Body));
            else if (vals.Length < op.NumberOfValues)
                currentErrors.AddValidationError(Messages.Operator_NumberOfValues,
                    oper, op.NumberOfValues, vals.Length, GetPropertyName(propName, prop.Body));
            else return qry.Where(op.GetPredicate(prop, vals));
            return qry;
        }

        /// <summary>
        /// Adds a Where clause to the given query for the specified property, operator and a list of values.
        /// If specified operator and values are not valid, then adds validation errors and returns the same query.
        /// </summary>
        /// <typeparam name="TElement">The type of the element.</typeparam> 
        /// <typeparam name="TValue">The type of the values.</typeparam> 
        /// <param name="qry">The query to add the Where clause to.</param>
        /// <param name="propName">Property display name.</param>
        /// <param name="prop">Expression for the property accessor.</param>
        /// <param name="oper">Operator name or alias.</param>
        /// <param name="vals">A list of criteria value(s).</param>
        /// <returns>The query with the Where clause added, or the same query if input is invalid.</returns>
        protected IQueryable<TElement> AddClause<TElement, TValue>(IQueryable<TElement> qry, string propName,
            Expression<Func<TElement, TValue>> prop, string oper, IEnumerable<TValue> vals)
        {
            return AddClause(qry, propName, prop, oper, vals?.ToArray());
        }


        /// <summary>
        /// Adds a Where clause to the given query for the specified nullable property, operator and a list of values.
        /// If specified operator and values are not valid, then adds validation errors and returns the same query.
        /// </summary>
        /// <typeparam name="TElement">The type of the element.</typeparam> 
        /// <typeparam name="TValue">The type of the values.</typeparam> 
        /// <param name="qry">The query to add the Where clause to.</param>
        /// <param name="propName">Property display name.</param>
        /// <param name="prop">Expression for the property accessor.</param>
        /// <param name="oper">Operator name or alias.</param>
        /// <param name="vals">A list of criteria value(s).</param>
        /// <returns>The query with the Where clause added, or the same query if input is invalid.</returns>
        protected IQueryable<TElement> AddClause<TElement, TValue>(IQueryable<TElement> qry, string propName,
            Expression<Func<TElement, TValue?>> prop, string oper, IEnumerable<TValue> vals) where TValue : struct
        {
            return AddClause(qry, propName, prop, oper, vals?.Cast<TValue?>().ToArray());
        }

        /// <summary>
        /// Adds a Where clause to the given query for the specified property and values using a default operator.
        /// </summary>
        /// <typeparam name="TElement">The type of the element.</typeparam> 
        /// <typeparam name="TValue">The type of the values.</typeparam> 
        /// <param name="qry">The query to add the Where clause to.</param>
        /// <param name="propName">Property display name.</param>
        /// <param name="prop">Expression for the property accessor.</param>
        /// <param name="vals">The criteria value(s).</param>
        /// <returns>The query with the Where clause added, or the same query if input is invalid.</returns>
        protected virtual IQueryable<TElement> AddClause<TElement, TValue>(IQueryable<TElement> qry, string propName,
            Expression<Func<TElement, TValue>> prop, params TValue[] vals)
        {
            string oper = (vals.Length == 2) ? BetweenOperator.DefaultName : EqualToOperator.DefaultName;
            return AddClause(qry, propName, prop, oper, vals);
        }

        /// <summary>
        /// Adds a Where clause to the given query for the specified property and a list of values using OneOfOperator.
        /// </summary>
        /// <typeparam name="TElement">The type of the element.</typeparam> 
        /// <typeparam name="TValue">The type of the values.</typeparam> 
        /// <param name="qry">The query to add the Where clause to.</param>
        /// <param name="propName">Property display name.</param>
        /// <param name="prop">Expression for the property accessor.</param>
        /// <param name="vals">A list of criteria value(s).</param>
        /// <returns>The query with the Where clause added, or the same query if input is invalid.</returns>
        protected virtual IQueryable<TElement> AddClause<TElement, TValue>(IQueryable<TElement> qry, string propName,
            Expression<Func<TElement, TValue>> prop, IEnumerable<TValue> vals)
        {
            return AddClause(qry, propName, prop, OneOfOperator.DefaultName, vals?.ToArray());
        }

        /// <summary>
        /// Adds a sort clause to the given query for the specified property and sort field.
        /// </summary>
        /// <typeparam name="TElement">The type of the element.</typeparam> 
        /// <typeparam name="TValue">The type of the values.</typeparam> 
        /// <param name="qry">The query to add the sort clause to.</param>
        /// <param name="prop">Expression for the property accessor.</param>
        /// <param name="sortField">The sort field definition.</param>
        /// <param name="first">True for the first sort field being added, false otherwise.</param>
        /// <returns>IOrderedQueryable with the sort clause added.</returns>
        protected virtual IOrderedQueryable<TElement> AddSortClause<TElement, TValue>(IQueryable<TElement> qry,
                Expression<Func<TElement, TValue>> prop, SortField sortField, bool first)
            => !first && qry is IOrderedQueryable<TElement> oqry ?
                sortField.IsDescending ? oqry.ThenByDescending(prop) : oqry.ThenBy(prop) :
                sortField.IsDescending ? qry.OrderByDescending(prop) : qry.OrderBy(prop);

        /// <summary>
        /// Reports an error for an unsupported sort field.
        /// </summary>
        /// <param name="sortField">The sort field that is not supported.</param>
        protected void UnknownSortFieldError(SortField sortField)
            => currentErrors.AddValidationError(Messages.SortField_NotSupported, sortField.FieldName);

        /// <summary>
        /// Adds Skip and Take clauses to the given query for the specified criteria as needed.
        /// </summary>
        /// <typeparam name="TElement">The type of the element.</typeparam> 
        /// <param name="qry">The query to add the Skip and Take clauses to.</param>
        /// <param name="criteria">Base criteria with paging params.</param>
        /// <param name="maxRows">Maximum number of rows to use when criteria has no paging.</param>
        /// <returns>A query with Skip and Take clauses added as needed.</returns>
        protected virtual IQueryable<TElement> AddSkipTake<TElement>(IQueryable<TElement> qry, SearchCriteria criteria, int? maxRows)
        {
            if (criteria?.Skip != null && criteria.Skip > 0)
                qry = qry.Skip(criteria.Skip.Value);
            var rowsToTake = criteria?.Take ?? maxRows;
            if (rowsToTake != null)
                qry = qry.Take(rowsToTake.Value);
            return qry;
        }

        /// <summary>
        /// Gets the total number of records that match the specified criteria as needed.
        /// Also adds a warning if criteria has no paging and the number of rows returned is more than the maximum number of rows.
        /// </summary>
        /// <param name="getTotalCount">A function to get the total row count.</param>
        /// <param name="criteria">Search criteria for the operation.</param>
        /// <param name="count">The number of rows retrieved with the current criteria.</param>
        /// <param name="maxRows">Maximum number of rows to use when criteria has no paging.</param>
        /// <returns>The total number of rows as needed.</returns>
        protected virtual int? GetTotal(Func<int> getTotalCount, SearchCriteria criteria, int count, int? maxRows)
        {
            int? totalCount = null;
            bool atMax = (criteria?.Skip ?? 0) == 0 && criteria?.Take == null && maxRows != null && count == maxRows.Value;
            bool lessThanPage = criteria != null && (criteria.Skip ?? 0) == 0 && criteria.Take != null && count < criteria.Take.Value;
            if (atMax || (criteria?.GetTotalCount ?? false))
            {
                totalCount = lessThanPage ? count : getTotalCount();
            }
            if (atMax && totalCount > count)
            {
                currentErrors.AddWarning(Messages.Service_MaxRows, count, totalCount);
            }
            return totalCount;
        }

        /// <summary>
        /// Gets the total number of records that match the specified criteria as needed.
        /// Also adds a warning if criteria has no paging and the number of rows returned is more than the maximum number of rows.
        /// </summary>
        /// <param name="getTotalCountAsync">A function to get the total row count.</param>
        /// <param name="criteria">Search criteria for the operation.</param>
        /// <param name="count">The number of rows retrieved with the current criteria.</param>
        /// <param name="maxRows">Maximum number of rows to use when criteria has no paging.</param>
        /// <returns>The total number of rows as needed.</returns>
        protected virtual async Task<int?> GetTotalAsync(Func<Task<int>> getTotalCountAsync, SearchCriteria criteria, int count, int? maxRows)
        {
            int? totalCount = null;
            bool atMax = (criteria?.Skip ?? 0) == 0 && criteria?.Take == null && maxRows != null && count == maxRows.Value;
            bool lessThanPage = criteria != null && (criteria.Skip ?? 0) == 0 && criteria.Take != null && count < criteria.Take.Value;
            if (atMax || (criteria?.GetTotalCount ?? false))
            {
                totalCount = lessThanPage ? count : await getTotalCountAsync();
            }
            if (atMax && totalCount > count)
            {
                currentErrors.AddWarning(Messages.Service_MaxRows, count, totalCount);
            }
            return totalCount;
        }
        #endregion
    }
}