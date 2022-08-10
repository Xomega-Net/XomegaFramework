// Copyright (c) 2021 Xomega.Net. All rights reserved.

using System.Linq;
using System.Collections.Generic;

namespace Xomega.Framework
{
    /// <summary>
    /// A structure for field criteria setting in display format.
    /// </summary>
    public struct FieldCriteriaSetting
    {
        /// <summary>Field label</summary>
        public string Label { get; set; }

        /// <summary>Operator</summary>
        public string Operator { get; set; }
        
        /// <summary>Field value in display format</summary>
        public string[] Value { get; set; }

        /// <summary>
        /// Formats current field criteria as a display string
        /// </summary>
        public override string ToString()
        {
            return Label + ":" + 
                (Operator != null ? " " + Operator : "") + 
                (Value != null && Value.Length > 0 ? " " + string.Join(" and ", Value) : "");
        }

        /// <summary>
        /// Formats a list of criteria as a display string
        /// </summary>
        /// <param name="criteria">Criteria to format</param>
        /// <returns>A formatted string for the specified criteria</returns>
        public static string ToString(List<FieldCriteriaSetting> criteria)
        {
            return criteria == null ? null : string.Join("; ", criteria.Select(s => s.ToString()).ToArray());
        }
    }
}
