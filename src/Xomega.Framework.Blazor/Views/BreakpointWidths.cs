// Copyright (c) 2021 Xomega.Net. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Xomega.Framework.Blazor.Views
{
    /// <summary>
    /// Enumeration for Bootstrap breakpoints for various screen sizes.
    /// </summary>
    public enum Breakpoint
    {
        /// <summary>
        /// Default extra small screen breakpoint.
        /// </summary>
        xs,

        /// <summary>
        /// Small screen breakpoint.
        /// </summary>
        sm,

        /// <summary>
        /// Medium screen breakpoint.
        /// </summary>
        md,

        /// <summary>
        /// Large screen breakpoint.
        /// </summary>
        lg,
        
        /// <summary>
        /// Extra large screen breakpoint.
        /// </summary>
        xl,

        /// <summary>
        /// The largest screen breakpoint.
        /// </summary>
        xxl
    }

    /// <summary>
    /// Utility class for calculating Bootstrap column classes for each breakpoint
    /// for elements that take different shares of the screen at each breakpoint.
    /// </summary>
    public class BreakpointWidths
    {
        /// <summary>
        /// Default Bootstrap grid breakpoints.
        /// </summary>
        public static Dictionary<Breakpoint, int> GridDefaults = new Dictionary<Breakpoint, int>()
        {
            { Breakpoint.xs, 0},
            { Breakpoint.sm, 576 },
            { Breakpoint.md, 768 },
            { Breakpoint.lg, 992 },
            { Breakpoint.xl, 1200 },
            { Breakpoint.xxl, 1400 },
        };

        /// <summary>
        /// Default Bootstrap container breakpoints.
        /// </summary>
        public static Dictionary<Breakpoint, int> ContainerDefaults = new Dictionary<Breakpoint, int>()
        {
            { Breakpoint.xs, 0},
            { Breakpoint.sm, 540 },
            { Breakpoint.md, 720 },
            { Breakpoint.lg, 960 },
            { Breakpoint.xl, 1140 },
            { Breakpoint.xxl, 1320 },
        };

        /// <summary>
        /// Default Bootstrap modal breakpoints.
        /// </summary>
        public static Dictionary<Breakpoint, int> ModalDefaults = new Dictionary<Breakpoint, int>()
        {
            { Breakpoint.xs, 0},
            { Breakpoint.sm, 300 },
            { Breakpoint.md, 500 },
            { Breakpoint.lg, 800 },
            { Breakpoint.xl, 1140 },
            { Breakpoint.xxl, 1140 },
        };

        // a dictionary of widths at each breakpoint
        private readonly Dictionary<Breakpoint, int> widths = new Dictionary<Breakpoint, int>();

        /// <summary>
        /// Constructs widths based on Bootstrap grid defaults.
        /// </summary>
        public BreakpointWidths() : this(GridDefaults) { }

        /// <summary>
        /// Constructs new widths based on the specified widths.
        /// </summary>
        public BreakpointWidths(BreakpointWidths otherWidths) : this(otherWidths.widths) { }

        /// <summary>
        /// Constructs widths based on the specified dictionary of widths by breakpoint.
        /// </summary>
        public BreakpointWidths(Dictionary<Breakpoint, int> vals)
        {
            foreach (var bp in vals.Keys)
                widths[bp] = vals[bp];
        }

        /// <summary>
        /// Updates the breakpoint widths for the case when an element takes a certain number of columns
        /// on screen sizes that are below the specified breakpoint, and another number of columns
        /// when the screen sizes are at or above the specified breakpoint.
        /// </summary>
        /// <param name="breakpoint">The breakpoint for the before and after number of columns.</param>
        /// <param name="beforeCol">The number of columns an element takes on screens below the specified breakpoint.</param>
        /// <param name="afterCol">The number of columns an element takes on screens at or above the specified breakpoint.</param>
        public void ApplyColumns(Breakpoint breakpoint, int beforeCol, int afterCol)
        {
            foreach (Breakpoint bp in Enum.GetValues(typeof(Breakpoint)))
            {
                widths.TryGetValue(bp, out int w);
                int col = bp < breakpoint ? beforeCol : afterCol;
                widths[bp] = w * col / 12;
            }
        }

        /// <summary>
        /// Gets the width at the specified breakpoint.
        /// </summary>
        /// <param name="bp">Breakpoint for the width.</param>
        /// <returns>The width at the specified breakpoint.</returns>
        public int GetWidth(Breakpoint bp) => widths.TryGetValue(bp, out int w) ? w : 0;

        /// <summary>
        /// Gets a string of Bootstrap classes for various breakpoints that lays out the fields of a panel
        /// in the optimal number of columns at each breakpoint, taking into account the specified maximum number of columns
        /// to lay out fields in and the preferred width of the fields.
        /// </summary>
        /// <param name="maxCol">The specified maximum number of columns to lay out fields.</param>
        /// <param name="fieldWidth">The preferred width of the fields.</param>
        /// <returns>A string of Bootstrap classes for various breakpoints for the panel's field columns.</returns>
        public string GetRowCols(int maxCol, int fieldWidth)
        {
            List<string> colCls = new List<string>();
            int cols = 0;
            foreach (Breakpoint bp in Enum.GetValues(typeof(Breakpoint)))
            {
                int w = GetWidth(bp);
                int c = Math.Max(1, Math.Min(w / fieldWidth, maxCol));
                if (c == cols) continue;
                cols = c;
                string bps = bp == Breakpoint.xs ? "" : "-" + bp;
                colCls.Add($"row-cols{bps}-{cols}");
            }
            return string.Join(" ", colCls.ToArray());
        }

        /// <summary>
        /// Gets an infix for Bootstrap classes for the specified breakpoint.
        /// </summary>
        /// <param name="bp">Breakpoint for the infix</param>
        /// <returns>An infix for Bootstrap classes for the specified breakpoint.</returns>
        public string BreakpointInfix(Breakpoint bp) => bp == Breakpoint.xs ? "" : "-" + bp;

        /// <summary>
        /// For the current widths calculates breakpoints and a number of columns at those breakpoints
        /// to accommodate fields of the specified width up to provided maximum number of columns.
        /// </summary>
        /// <param name="maxCol">The maximum number of columns allowed.</param>
        /// <param name="width">The width of each field to use for calculating a number of columns.</param>
        /// <returns>BreakpointWidths with breakpoints and the width being the number of columns for those breakpoints.</returns>
        public BreakpointWidths GetCols(int maxCol, int width)
        {
            Dictionary<Breakpoint, int> bpCols = new Dictionary<Breakpoint, int>();
            int cols = 0;
            foreach (Breakpoint bp in Enum.GetValues(typeof(Breakpoint)))
            {
                int w = GetWidth(bp);
                int c = Math.Max(1, Math.Min(w / width, maxCol));
                if (c != cols)
                    bpCols[bp] = cols = c;
            }
            return new BreakpointWidths(bpCols);
        }

        /// <summary>
        /// When the current widths represent a number of columns for each breakpoint,
        /// constructs a string of Bootstrap row-cols classes for laying out panel's child fields.
        /// </summary>
        /// <returns>A string of Bootstrap row-cols classes for laying out panel's child fields.</returns>
        public string ToRowColsClass() => string.Join(" ", widths.Select(w => $"row-cols{BreakpointInfix(w.Key)}-{w.Value}"));

        /// <summary>
        /// When the current widths represent a number of columns for each breakpoint,
        /// constructs a string of Bootstrap col classes for laying out elements in the parent panel.
        /// </summary>
        /// <returns>A string of Bootstrap col classes for laying out elements in the parent panel.</returns>
        public string ToColClass() => string.Join(" ", widths.Select(w => $"col{BreakpointInfix(w.Key)}-{(w.Value == 0 ? 0 : 12 / w.Value)}"));
    }
}
