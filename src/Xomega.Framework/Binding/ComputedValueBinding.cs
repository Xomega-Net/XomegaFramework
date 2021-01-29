// Copyright (c) 2021 Xomega.Net. All rights reserved.

using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Xomega.Framework
{
    /// <summary>
    /// Computed binding for updating property value.
    /// </summary>
    public class ComputedValueBinding : ComputedBinding
    {
        /// <summary>
        /// Constructs a new computed binding for updating property value.
        /// </summary>
        /// <param name="property">The property to update based on the computed result.</param>
        /// <param name="expression">Lambda expression used to compute the result.</param>
        /// <param name="args">Arguments for the specified expression to use for evaluation.</param>
        public ComputedValueBinding(DataProperty property, LambdaExpression expression, params object[] args)
            : base(property, expression, args)
        {
        }

        /// <inheritdoc/>
        public override async Task UpdateAsync(DataRow row, CancellationToken token)
        {
            if (property is DataProperty dp)
                await dp.SetValueAsync(ComputedValue, row);
        }
    }
}
