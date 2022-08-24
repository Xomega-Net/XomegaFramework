// Copyright (c) 2022 Xomega.Net. All rights reserved.

using System;
using System.Web;

namespace Xomega.Framework.Web
{
    /// <summary>
    /// A class for web application to track time in various operations, e.g. service calls.
    /// To use it wrap your code with the following code 
    /// using (TimeTracker.ServiceCall) { ... your service call ... }
    /// using (new TimeTracker("your category")) { ... your code ... }
    /// </summary>
    public class TimeTracker : IDisposable
    {
        /// <summary>
        /// Special category for tracking time for service calls
        /// </summary>
        public const string Service = "service";

        /// <summary>
        /// Get a new instance for tracking service call time
        /// </summary>
        public static TimeTracker ServiceCall { get { return new TimeTracker(Service); } }

        /// <summary>
        /// Get time spent in service calls
        /// </summary>
        /// <returns>time spent in service calls in milliseconds</returns>
        public static double GetServiceTime() { return GetTime(Service); }

        /// <summary>
        /// Get time spent in specific category in milliseconds
        /// </summary>
        /// <param name="category">category for which to get the time.</param>
        /// <returns>time spent in specific category in milliseconds</returns>
        public static double GetTime(string category)
        {
            object time = HttpContext.Current.Items[category];
            return (time is double) ? (double)time : 0;
        }

        /// <summary>
        /// Get total request time up to now.
        /// </summary>
        /// <returns>The total request time up to now in milliseconds</returns>
        public static double GetRequestTime() { return (DateTime.Now - HttpContext.Current.Timestamp).TotalMilliseconds; }

        private DateTime startTime;
        private string category;

        /// <summary>
        /// Constructs a new time tracker for the given category
        /// </summary>
        /// <param name="category">Category for which to track time</param>
        public TimeTracker(string category)
        {
            this.category = category;
            this.startTime = DateTime.Now;
        }

        /// <summary>
        /// Stores the time for the current category in the request context, or adds time to it.
        /// </summary>
        public void Dispose()
        {
            AddTime(category, (DateTime.Now - startTime).TotalMilliseconds);
        }

        /// <summary>
        /// Adds some time to the timer for the given category.
        /// </summary>
        /// <param name="category">Category name to add time to</param>
        /// <param name="ms">Number of milliseconds to add</param>
        public static void AddTime(string category, double ms)
        {
            if (HttpContext.Current != null)
                HttpContext.Current.Items[category] = GetTime(category) + ms;
        }
    }
}