using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Xomega.Framework
{
    /// <summary>
    /// Computed binding for updating property visibility.
    /// </summary>
    public class ComputedVisibleBinding : ComputedBinding
    {
        /// <summary>
        /// Constructs a new computed binding for updating property visibility.
        /// </summary>
        /// <param name="property">The property to update based on the computed result.</param>
        /// <param name="expression">Lambda expression used to compute the result.</param>
        /// <param name="args">Arguments for the specified expression to use for evaluation.</param>
        public ComputedVisibleBinding(BaseProperty property, LambdaExpression expression, params object[] args)
            : base(property, expression, args)
        {
            if (expression.ReturnType != typeof(bool))
                throw new Exception("Supplied expression should return a bool.");
        }

        /// <inheritdoc/>
        public override Task UpdateAsync(DataRow row, CancellationToken token)
        {
            property.Visible = (bool)ComputedValue;
            return Task.CompletedTask;
        }
    }
}
