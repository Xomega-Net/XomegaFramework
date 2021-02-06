// Copyright (c) 2021 Xomega.Net. All rights reserved.

using System;
using System.Collections.Generic;

namespace Xomega.Framework.Operators
{
    /// <summary>
    /// A registry of all known operators.
    /// </summary>
    public class OperatorRegistry
    {
        private Dictionary<string, Operator> registry = new Dictionary<string, Operator>();

        /// <summary>
        /// Constructs a registry and registers default operators.
        /// </summary>
        public OperatorRegistry() : this(true) { }

        /// <summary>
        /// Constructs a registry and registers default operators based on the specified flag.
        /// </summary>
        /// <param name="registerDefaults">True to register default operators, False otherwise.</param>
        public OperatorRegistry(bool registerDefaults)
        {
            if (registerDefaults)
            {
                Register(new Operator[] {
                    new IsNullOperator(), new IsNotNullOperator(),
                    new EqualToOperator(), new NotEqualToOperator(),
                    new OneOfOperator(false), new OneOfOperator(true),
                    new ContainsOperator(false), new ContainsOperator(true),
                    new StartsWithOperator(false), new StartsWithOperator(true),
                    new LessThanOperator(), new LessThanOrEqualOperator(),
                    new GreaterThanOperator(), new GreaterThanOrEqualOperator(),
                    new BetweenOperator(false), new BetweenOperator(true),
                });
            }
        }

        /// <summary>
        /// Registers supplied list of operators under the names returned by their GetNames method.
        /// </summary>
        /// <param name="operators">The list of operators to register.</param>
        public virtual void Register(params Operator[] operators)
        {
            foreach (Operator op in operators)
            {
                foreach (string name in op.GetNames())
                {
                    if (name != null) registry[name.ToUpper()] = op;
                }
            }
        }

        /// <summary>
        /// Looks up the operator by its registered name.
        /// </summary>
        /// <param name="name">Operator name to look up by.</param>
        /// <param name="valueType">The value type for the operator.</param>
        /// <returns>The operator found, or null if no operator found by the given name.</returns>
        public virtual Operator GetOperator(string name, Type valueType)
        {
            Operator op = null;
            if (name != null) // look up operator in the registry
                registry.TryGetValue(name.ToUpper(), out op);

            if (op == null) // try constructing a dynamic range operator
                op = DynamicRangeUtil.GetRangeOperator(name, valueType);
            return op;
        }
    }
}
