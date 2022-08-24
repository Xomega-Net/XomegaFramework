// Copyright (c) 2022 Xomega.Net. All rights reserved.

using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Xomega.Framework.Operators
{
    /// <summary>
    /// Utility class that helps construct a new dynamic range operator for date/time or numeric types
    /// from a range string that follows specific format for numeric or date/time ranges.
    /// The range format allows comma-separated optional low and high bounds surrounded by a square bracket
    /// or a parenthesis to indicate inclusion or exclusion of that bound respectively.
    /// The bounds for numeric ranges are integer or decimal numbers as appropriate.
    /// The bounds for date/time ranges can be an absolute date formatted as yyyy-MM-dd,
    /// or a relative date, which specifies the base current time (ct) or a beginning or end of a time period,
    /// e.g. 'boM' for the beginning of the current month, or 'eod' for the end of the current day.
    /// You can then add or subtract any whole number of periods to/from the base time to get the relative date,
    /// e.g. 'ct-30d' for the date 30 days before the current time.
    /// </summary>
    public static class DynamicRangeUtil
    {
        // Bases for relative dates
        private const string CurrentTime = "ct";
        private const string BeginningOf = "bo";
        private const string EndOf = "eo";

        // Time periods
        private const string Second = "s";
        private const string Minute = "m";
        private const string Hour = "h";
        private const string Day = "d";
        private const string Week = "w";
        private const string Month = "M";
        private const string Year = "y";

        private static string[] Periods = {Year, Month, Week, Day, Hour, Minute, Second};

        // Patterns for numbers and dates
        private static string PatternNumber = @"\d+(\.\d+)?";
        private static string PatternAbsoluteDate = @"\d{4}-\d{2}-\d{2}"; // yyyy-MM-dd
        private static string PatternPeriod = $"({string.Join("|", Periods)})";
        private static string PatternRelativeDate = $@"(?<base>{CurrentTime}|({BeginningOf}|{EndOf}){PatternPeriod})((?<op>[\+\-])(?<num>\d+)(?<per>{PatternPeriod}))?";
        private static string PatternDate = $"{PatternRelativeDate}|{PatternAbsoluteDate}";

        // Get a regex pattern for a range using the specified pattern for the lower and upper bounds.
        private static string GetRangePattern(string boundsPatttern)
        {
            return $@"^(?<if>\[|\()(?<from>{boundsPatttern})?,\s*(?<to>{boundsPatttern})?(?<it>\]|\))$";
        }

        /// <summary>
        /// Constructs a dynamic range operator for the specified range string and value type.
        /// </summary>
        /// <param name="range">The string that defines a date/time or numeric range using appropriate format.</param>
        /// <param name="valueType">The data type of the property being filtered.</param>
        /// <returns>A new dynamic range operator for the specified range and type,
        /// or null if the range string or the type are not valid.</returns>
        public static Operator GetRangeOperator(string range, Type valueType)
        {
            bool isDate = valueType == typeof(DateTime) || valueType == typeof(DateTime?);
            string ptnRange = GetRangePattern(isDate ? PatternDate : PatternNumber);
            Match m = Regex.Match(range, ptnRange);
            if (!m.Success) return null;

            Group from = m.Groups["from"];
            Group to = m.Groups["to"];
            bool? includeFrom = from.Success ? "[" == m.Groups["if"].Value : (bool?)null;
            bool? includeTo = to.Success ? "]" == m.Groups["it"].Value : (bool?)null;

            if (isDate)
            {
                DateTime? fromDate = GetDate(from.Value);
                DateTime? toDate = GetDate(to.Value);
                if (valueType == typeof(DateTime))
                    return new DynamicRangeOperator<DateTime>(range, fromDate.Value, includeFrom, toDate.Value, includeTo);
                if (valueType == typeof(DateTime?))
                    return new DynamicRangeOperator<DateTime?>(range, fromDate, includeFrom, toDate, includeTo);
            }
            else
            {
                double? fromNum = from.Success ? double.Parse(from.Value) : 0;
                double? toNum = to.Success ? double.Parse(to.Value) : 0;
                if (valueType == typeof(double))
                    return new DynamicRangeOperator<double>(range, fromNum.Value, includeFrom, toNum.Value, includeTo);
                if (valueType == typeof(double?))
                    return new DynamicRangeOperator<double?>(range, fromNum, includeFrom, toNum, includeTo);
                if (valueType == typeof(decimal))
                    return new DynamicRangeOperator<decimal>(range, (decimal)fromNum, includeFrom, (decimal)toNum, includeTo);
                if (valueType == typeof(decimal?))
                    return new DynamicRangeOperator<decimal?>(range, (decimal?)fromNum, includeFrom, (decimal?)toNum, includeTo);
                if (valueType == typeof(float))
                    return new DynamicRangeOperator<float>(range, (float)fromNum, includeFrom, (float)toNum, includeTo);
                if (valueType == typeof(float?))
                    return new DynamicRangeOperator<float?>(range, (float)fromNum, includeFrom, (float)toNum, includeTo);
                if (valueType == typeof(int))
                    return new DynamicRangeOperator<int>(range, (int)fromNum, includeFrom, (int)toNum, includeTo);
                if (valueType == typeof(int?))
                    return new DynamicRangeOperator<int?>(range, (int)fromNum, includeFrom, (int)toNum, includeTo);
                if (valueType == typeof(long))
                    return new DynamicRangeOperator<long>(range, (long)fromNum, includeFrom, (long)toNum, includeTo);
                if (valueType == typeof(long?))
                    return new DynamicRangeOperator<long?>(range, (long)fromNum, includeFrom, (long)toNum, includeTo);
                if (valueType == typeof(short))
                    return new DynamicRangeOperator<short>(range, (short)fromNum, includeFrom, (short)toNum, includeTo);
                if (valueType == typeof(short?))
                    return new DynamicRangeOperator<short?>(range, (short)fromNum, includeFrom, (short)toNum, includeTo);
                if (valueType == typeof(byte))
                    return new DynamicRangeOperator<byte>(range, (byte)fromNum, includeFrom, (byte)toNum, includeTo);
                if (valueType == typeof(byte?))
                    return new DynamicRangeOperator<byte?>(range, (byte)fromNum, includeFrom, (byte)toNum, includeTo);
            }
            return null;
        }

        // Get an absolute or relative date from the specified pattern, or MinValue if the pattern is blank.
        private static DateTime GetDate(string pattern)
        {
            if (pattern == "") return DateTime.MinValue;
            if (Regex.IsMatch(pattern, PatternAbsoluteDate))
            {
                return DateTime.ParseExact(pattern, "yyyy-MM-dd", CultureInfo.CurrentCulture);
            }
            Match m = Regex.Match(pattern, PatternRelativeDate);
            if (!m.Success) throw new ArgumentException("Pattern must be either a relative or an absolute date.", "pattern");
            DateTime dt = DateTime.Now;
            string baseVal = m.Groups["base"].Value;
            if (CurrentTime != baseVal)
            {
                int perIdx = Array.IndexOf(Periods, baseVal.Substring(2));
                dt = new DateTime(dt.Year,
                                  perIdx < 1 ? 1 : dt.Month,
                                  perIdx < 2 ? 1 : dt.Day,
                                  perIdx < 4 ? 0 : dt.Hour,
                                  perIdx < 5 ? 0 : dt.Minute, 0);
                if (perIdx == 2) // beginning of week
                { // make it based on current culture (e.g. Monday vs Sunday).
                    int dow = dt.DayOfWeek - CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek;
                    dow = (dow + 7) % 7;
                    dt = dt.AddDays(-dow);
                }
                if (EndOf == baseVal.Substring(0, 2))
                    dt = AddPeriod(dt, perIdx, 1);
            }
            if (m.Groups["per"].Success)
            {
                int perIdx = Array.IndexOf(Periods, m.Groups["per"].Value);
                int qty = int.Parse(m.Groups["num"].Value);
                if ("-" == m.Groups["op"].Value) qty = -qty;
                dt = AddPeriod(dt, perIdx, qty);
            }
            return dt;
        }

        // Adds the specified number of the given periods to a date/time
        // using period index in the Periods array.
        private static DateTime AddPeriod(DateTime dt, int periodIdx, int qty)
        {
            switch (periodIdx)
            {
                case 0: return dt.AddYears(qty);
                case 1: return dt.AddMonths(qty);
                case 2: return dt.AddDays(7*qty);
                case 3: return dt.AddDays(qty);
                case 4: return dt.AddHours(qty);
                case 5: return dt.AddMinutes(qty);
                case 6: return dt.AddSeconds(qty);
            }
            return dt;
        }
    }
}
