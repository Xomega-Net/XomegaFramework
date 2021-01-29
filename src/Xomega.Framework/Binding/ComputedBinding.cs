// Copyright (c) 2021 Xomega.Net. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Xomega.Framework.Properties;

namespace Xomega.Framework
{
    /// <summary>
    /// Abstract base class for computed property bindings, which use an expression that involves other properties
    /// to compute the result, and subscribes to changes in those properties in order to update this result.
    /// </summary>
    public abstract class ComputedBinding : ExpressionVisitor
    {
        private readonly Dictionary<string, object> namedArgs = new Dictionary<string, object>();
        private readonly Dictionary<BaseProperty, PropertyChange> properties = new Dictionary<BaseProperty, PropertyChange>();
        private readonly Delegate compiledExpression;

        /// <summary>
        /// The property to update based on the computed result.
        /// </summary>
        protected BaseProperty property;

        /// <summary>
        /// Constructs a new computed binding for the property using the specified expression and concrete arguments.
        /// </summary>
        /// <param name="property">The property to update based on the computed result.</param>
        /// <param name="expression">Lambda expression used to compute the result.</param>
        /// <param name="args">Arguments for the specified expression to use for evaluation.</param>
        public ComputedBinding(BaseProperty property, LambdaExpression expression, params object[] args)
        {
            this.property = property ?? throw new ArgumentException("Property cannot be null", nameof(property));

            compiledExpression = expression.Compile();
            if (expression.Parameters.Count != args.Length)
                throw new ArgumentException($"The number of arguments should match the number of expression parameters", nameof(args));
            for (int i = 0; i < expression.Parameters.Count; i++)
                namedArgs[expression.Parameters[i].Name] = args[i];

            Visit(expression);
            foreach (var p in properties.Keys) p.AsyncChange += RecomputeAsync;
        }

        /// <summary>
        /// Disposes the computed binding by unsubscribing from all property changes.
        /// </summary>
        public void Dispose()
        {
            foreach (var p in properties.Keys) p.AsyncChange -= RecomputeAsync;
        }

        private async Task RecomputeAsync(object sender, PropertyChangeEventArgs e, CancellationToken token)
        {
            if (sender is BaseProperty bp && properties.TryGetValue(bp, out PropertyChange chg) && e.Change.IncludesChanges(chg))
            {
                await UpdateAsync(e.Row, token);
            }
        }

        /// <summary>
        /// Returns evaluated computed value based on the binding's expression and arguments.
        /// </summary>
        protected object ComputedValue => compiledExpression.DynamicInvoke(namedArgs.Values.ToArray());

        /// <summary>
        /// Updates the property with the computed result, as implemented by the subclasses.
        /// </summary>
        /// <param name="row">The row in a data list to update, or null if the property is not in a data list.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>The task for the asynchronous operation.</returns>
        public abstract Task UpdateAsync(DataRow row, CancellationToken token);

        /// <summary>
        /// Evaluates a specific property for the given sub-expression.
        /// </summary>
        /// <param name="objExpr">Sub-expression that evaluates to a data property.</param>
        /// <returns>Evaluated data property.</returns>
        protected object EvaluateProperty(Expression objExpr)
        {
            ParameterExpression param;
            if (objExpr is ParameterExpression pex) param = pex;
            else
            {
                Expression member = objExpr;
                while (member is MemberExpression mex) member = mex.Expression;
                param = member as ParameterExpression;
            }
            if (param == null)
                throw new Exception("Cannot find parameter for " + objExpr);
            else if (!namedArgs.ContainsKey(param.Name))
                throw new Exception("Cannot find argument for parameter " + param.Name);
            else
            {
                var dlgt = Expression.Lambda(objExpr, param).Compile();
                return dlgt.DynamicInvoke(namedArgs[param.Name]);
            }
        }

        /// <summary>
        /// Stores specified change to the given property to listen to,
        /// or adds the change to the stored property, if one already exists.
        /// </summary>
        /// <param name="prop">The property to listen to.</param>
        /// <param name="change">The property change to listen to.</param>
        protected void AddPropertyChange(BaseProperty prop, PropertyChange change)
        {
            if (properties.ContainsKey(prop))
                properties[prop] = properties[prop] + change;
            else properties[prop] = change;
        }

        /// <summary>
        /// Gets the change to listen to for the specified method on a data property, when the method
        /// is known to use members that raise a property change event.
        /// </summary>
        /// <param name="method">The method name.</param>
        /// <returns>Property change to listen to or null, if the method is not known to raise any property change events.</returns>
        protected virtual PropertyChange GetMethodProperties(string method)
        {
            switch (method)
            {
                case nameof(DataProperty.IsNull):
                case nameof(DataProperty.GetValue):
                case nameof(DataProperty.GetStringValue):
                case nameof(EnumProperty.GetValues): return PropertyChange.Value;
                default: return null;
            }
        }

        /// <summary>
        /// Gets the change to listen to for the specified member of a data property, when the member
        /// is known to raise a property change event.
        /// </summary>
        /// <param name="member">The member name.</param>
        /// <returns>Property change to listen to or null, if the member is not known to raise any property change events.</returns>
        protected virtual PropertyChange GetMemberProperties(string member)
        {
            switch (member)
            {
                case nameof(DataProperty.InternalValue):
                case nameof(DataProperty.DisplayStringValue):
                case nameof(DataProperty.EditStringValue):
                case nameof(DataProperty.TransportValue):
                case nameof(EnumProperty.Values):
                case nameof(EnumProperty.Value): return PropertyChange.Value;
                case nameof(BaseProperty.Editable): return PropertyChange.Editable;
                case nameof(BaseProperty.Editing): return PropertyChange.Editing;
                case nameof(BaseProperty.Visible): return PropertyChange.Visible;
                case nameof(BaseProperty.Required): return PropertyChange.Required;
                case nameof(BaseProperty.AccessLevel): return PropertyChange.Editable + PropertyChange.Visible;
                default: return null;
            }
        }

        /// <inheritdoc/>
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Object != null && node.Object.Type.IsSubclassOf(typeof(BaseProperty)))
            {
                var change = GetMethodProperties(node.Method.Name);
                if (change != null)
                {
                    var prop = EvaluateProperty(node.Object) as BaseProperty;
                    AddPropertyChange(prop, change);
                }
            }
            return base.VisitMethodCall(node);
        }

        /// <inheritdoc/>
        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.Member.ReflectedType.IsSubclassOf(typeof(BaseProperty)))
            {
                var change = GetMemberProperties(node.Member.Name);
                if (change != null)
                {
                    var prop = EvaluateProperty(node.Expression) as BaseProperty;
                    AddPropertyChange(prop, change);
                }
            }
            return base.VisitMember(node);
        }
    }
}