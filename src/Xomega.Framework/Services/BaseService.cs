// Copyright (c) 2019 Xomega.Net. All rights reserved.

using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Resources;
using System.Security.Principal;
using System.Threading;

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
        /// Default constructor.
        /// </summary>
        public BaseService(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            currentErrors = serviceProvider.GetService<ErrorList>() ?? new ErrorList(serviceProvider.GetService<ResourceManager>());
            errorParser = serviceProvider.GetService<ErrorParser>() ?? new ErrorParser();
            operators = serviceProvider.GetService<OperatorRegistry>() ?? new OperatorRegistry();
            currentPrincipal = serviceProvider.GetService<IPrincipal>();
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
            MemberExpression me = prop as MemberExpression;
            if (me != null) return me.Member.Name;
            if (prop != null) return prop.ToString();
            return null;
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

        #endregion
    }
}