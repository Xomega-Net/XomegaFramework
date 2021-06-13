// Copyright (c) 2021 Xomega.Net. All rights reserved.

using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        private readonly Dictionary<INotifyPropertyChanged, HashSet<string>> objects = new Dictionary<INotifyPropertyChanged, HashSet<string>>();
        private readonly HashSet<DataListObject> selectionLists = new HashSet<DataListObject>();
        private readonly Delegate compiledExpression;
        private readonly string rowArg = null;

        private static bool IsRowParam(ParameterExpression pe) => pe.Type == typeof(DataRow);

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
            this.property = property;

            compiledExpression = expression.Compile();

            IEnumerable<ParameterExpression> parameters = expression.Parameters;
            // last parameter can be dynamic data row not provided in the args
            if (parameters.Count() == args.Length + 1 && parameters.Last().Type == typeof(DataRow))
            {
                rowArg = parameters.Last().Name;
                parameters = parameters.Take(parameters.Count() - 1);
            }
            if (parameters.Count() != args.Length)
                throw new ArgumentException($"The number of arguments should match the number of expression parameters", nameof(args));
            for (int i = 0; i < args.Length; i++)
                namedArgs[expression.Parameters[i].Name] = args[i];

            Visit(expression);
            foreach (var p in properties.Keys)
            {
                // subscribe to both sync and async, so that we could update using appropriate mode based on IsAsyc of the event
                p.Change += Recompute;
                p.AsyncChange += RecomputeAsync;
            }
            foreach (var o in objects.Keys) o.PropertyChanged += Recompute;
            foreach (var s in selectionLists) s.SelectionChanged += RecomputeOnSelection;
        }

        /// <summary>
        /// Disposes the computed binding by unsubscribing from all property changes.
        /// </summary>
        public void Dispose()
        {
            foreach (var p in properties.Keys)
            {
                p.Change -= Recompute;
                p.AsyncChange -= RecomputeAsync;
            }
            foreach (var o in objects.Keys) o.PropertyChanged -= Recompute;
            foreach (var s in selectionLists) s.SelectionChanged -= RecomputeOnSelection;
        }

        private void Recompute(object sender, PropertyChangeEventArgs e)
        {
            if (!e.IsAsync && sender is BaseProperty bp && properties.TryGetValue(bp, out PropertyChange chg)
                && e.Change.IncludesChanges(chg))
            {
                Update(e.Row);
            }
        }

        private async Task RecomputeAsync(object sender, PropertyChangeEventArgs e, CancellationToken token)
        {
            if (e.IsAsync && sender is BaseProperty bp && properties.TryGetValue(bp, out PropertyChange chg)
                && e.Change.IncludesChanges(chg))
            {
                await UpdateAsync(e.Row, token);
            }
        }

        private void Recompute(object sender, PropertyChangedEventArgs e)
        {
            if (sender is INotifyPropertyChanged inpc && objects.TryGetValue(inpc, out HashSet<string> props) && props.Contains(e.PropertyName))
            {
                Update(null);
            }
        }

        private void RecomputeOnSelection(object sender, EventArgs e) => Update(null);

        /// <summary>
        /// Returns evaluated computed value based on the binding's expression and arguments.
        /// </summary>
        protected object GetComputedValue(DataRow row)
        {
            List<object> vals = new List<object>(namedArgs.Values);
            if (rowArg != null) vals.Add(row);
            return compiledExpression.DynamicInvoke(vals.ToArray());
        }

        /// <summary>
        /// Updates the property with the computed result, as implemented by the subclasses.
        /// </summary>
        /// <param name="row">The row in a data list to update, or null if the property is not in a data list.</param>
        public abstract void Update(DataRow row);

        /// <summary>
        /// Asynchronously updates the property with the computed result. Delegates to the synchronous method by default.
        /// </summary>
        /// <param name="row">The row in a data list to update, or null if the property is not in a data list.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>The task for the asynchronous operation.</returns>
        public async virtual Task UpdateAsync(DataRow row, CancellationToken token)
        {
            Update(row);
            await Task.CompletedTask;
        }

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
        /// Stores specified INotifyPropertyChanged object and the member
        /// to listen to the changes of.
        /// </summary>
        /// <param name="obj">The INotifyPropertyChanged to subscribe to.</param>
        /// <param name="member">The member to listen for the changes of.</param>
        protected void AddPropertyChange(INotifyPropertyChanged obj, string member)
        {
            if (!objects.TryGetValue(obj, out HashSet<string> members))
            {
                members = new HashSet<string>();
                objects[obj] = members;
            }
            members.Add(member);
        }

        /// <summary>
        /// Adds specified list object to subscribe to selection change events.
        /// </summary>
        /// <param name="list">The list object to listen for selection chang events.</param>
        protected void AddSelectionChange(DataListObject list)
        {
            if (list != null)
                selectionLists.Add(list);
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
            else if (typeof(INotifyPropertyChanged).IsAssignableFrom(node.Member.ReflectedType))
            {
                var obj = EvaluateProperty(node.Expression) as INotifyPropertyChanged;
                AddPropertyChange(obj, node.Member.Name);
            }
            if (typeof(DataListObject).IsAssignableFrom(node.Member.ReflectedType) &&
                (node.Member.Name == nameof(DataListObject.SelectedRows) ||
                 node.Member.Name == nameof(DataListObject.SelectedRowIndexes)))
            {
                var list = EvaluateProperty(node.Expression) as DataListObject;
                AddSelectionChange(list);
            }
            return base.VisitMember(node);
        }
    }
}