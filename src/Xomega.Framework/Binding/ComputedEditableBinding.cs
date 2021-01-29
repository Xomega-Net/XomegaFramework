// Copyright (c) 2021 Xomega.Net. All rights reserved.

using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Xomega.Framework
{
    /// <summary>
    /// Computed binding for updating property editability.
    /// </summary>
    public class ComputedEditableBinding : ComputedBinding
    {
        /// <summary>
        /// Constructs a new computed binding for updating property editability.
        /// </summary>
        /// <param name="property">The property to update based on the computed result.</param>
        /// <param name="expression">Lambda expression used to compute the result.</param>
        /// <param name="args">Arguments for the specified expression to use for evaluation.</param>
        public ComputedEditableBinding(BaseProperty property, LambdaExpression expression, params object[] args)
            : base(property, expression, args)
        {
            if (expression.ReturnType != typeof(bool))
                throw new Exception("Supplied expression should return a bool.");
        }

        /// <inheritdoc/>
        public override Task UpdateAsync(DataRow row, CancellationToken token)
        {
            property.Editable = (bool)ComputedValue;
            return Task.CompletedTask;
        }
    }
}
