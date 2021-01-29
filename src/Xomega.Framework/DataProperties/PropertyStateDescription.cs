// Copyright (c) 2021 Xomega.Net. All rights reserved.

using System.Collections.Generic;
using System.Linq;

namespace Xomega.Framework
{
    /// <summary>
    /// A helper class that allows building a list of descriptions for the property state,
    /// which can be used as styles or CSS classes on property-bound controls.
    /// </summary>
    public class PropertyStateDescription
    {
        /// <summary>The string used to describe a required property.</summary>
        public string Required = "required";

        /// <summary>The string used to describe a modified property.</summary>
        public string Modified = "modified";

        /// <summary>The string used to describe a valid property.</summary>
        public string Valid = "valid";
        
        /// <summary>The string used to describe a property with a warning.</summary>
        public string Warning = "warning";

        /// <summary>The string used to describe an invalid property with an error.</summary>
        public string Invalid = "invalid";

        /// <summary>The string used to describe a read-only property.</summary>
        public string Readonly = "readonly";

        /// <summary>The string used to describe a hidden property.</summary>
        public string Hidden = "hidden";

        /// <summary>
        /// Get space-delimited string with the property states using current descriptions.
        /// </summary>
        /// <param name="property">The data property to get the states for.</param>
        /// <param name="states">The combination of property states to return.</param>
        /// <param name="row">Specific data row for list objects, or null for regular data objects.</param>
        public virtual string GetStateDescription(DataProperty property, PropertyChange states, DataRow row)
        {
            var propertyStates = GetStates(property, states, row);
            return string.Join(" ", propertyStates);
        }

        /// <summary>
        /// Get a list of property states for the given property using current descriptions.
        /// </summary>
        /// <param name="property">The data property to get the states for.</param>
        /// <param name="states">The combination of property states to return.</param>
        /// <param name="row">Specific data row for list objects, or null for regular data objects.</param>
        public virtual IEnumerable<string> GetStates(DataProperty property, PropertyChange states, DataRow row)
        {
            var state = new HashSet<string>();
            if (property.Visible)
            {
                if (property.Editable)
                {
                    if (property.Required && states.IncludesRequired())
                        state.Add(Required);
                    if (property.Modified == true && states.IncludesValue())
                        state.Add(Modified);
                    if (states.IncludesValidation())
                    {
                        var err = property.GetValidationErrors(row);
                        if (err?.Errors != null)
                        {
                            if (err.HasErrors())
                                state.Add(Invalid);
                            else if (err.Errors.Any(e => e.Severity == ErrorSeverity.Warning))
                                state.Add(Warning);
                            else state.Add(Valid);
                        }
                    }
                }
                else if (states.IncludesEditable())
                    state.Add(Readonly);
            }
            else if (states.IncludesVisible())
                state.Add(Hidden);

            return state;
        }
    }
}
