// Copyright (c) 2010-2013 Xomega.Net. All rights reserved.

namespace Xomega.Framework
{
    /// <summary>
    /// A class that represents different formats that data property values can be converted to.
    /// </summary>
    public class ValueFormat
    {
        /// <summary>
        /// The format in which values are stored internally in data properties.
        /// The format is typically typed, that is an integer would be stored as an <c>int</c>.
        /// Whenever a value is set on a data property, it will always try to convert it 
        /// to the internal format first. If it fails to convert it, it may store it as is.
        /// For multivalued data properties, each value in the list will be converted to an internal format.
        /// </summary>
        public static readonly ValueFormat Internal = new ValueFormat();

        /// <summary>
        /// The format in which data property values are transported between layers
        /// during a service call. The format is typically typed and may or may not be
        /// the same as the internal format. For example, we may want to store a resolved
        /// <c>Header</c> object internally, but send only the ID part in a service call.
        /// </summary>
        public static readonly ValueFormat Transport = new ValueFormat();

        /// <summary>
        /// The string format in which the user inputs the value. It may or may not be the same
        /// as the format in which the value is displayed to the user when it's not editable.
        /// </summary>
        public static readonly ValueFormat EditString = new ValueFormat();

        /// <summary>
        /// The string format in which the value is displayed to the user when it's not editable.
        /// When internal value is an object such as <c>Header</c>, the display string may
        /// consist of a combination of several of its parts (see <see cref="Header.ToString(string)"/>).
        /// </summary>
        public static readonly ValueFormat DisplayString = new ValueFormat();

        /// <summary>
        /// Checks if the current format is one of the string formats.
        /// </summary>
        /// <returns>True this is a string format, otherwise false</returns>
        public virtual bool IsString()
        {
            return this == EditString || this == DisplayString;
        }

        /// <summary>
        /// Checks if the current format is one of the typed formats.
        /// </summary>
        /// <returns>True this is a typed format, otherwise false</returns>
        public virtual bool IsTyped()
        {
            return this == Internal || this == Transport;
        }

        #region Construction / Shutdown

        /// <summary>
        /// Protected constructor to allow defining additional value formats through subclasses.
        /// </summary>
        protected ValueFormat()
        {
            if (!startedUp) StartUp();
        }

        /// <summary>
        /// Finalizer that calls application shutdown hook.
        /// </summary>
        ~ValueFormat()
        {
            // The following is commented out since it seems to be unsafe
            // to shut down instrumentation in a garbage collection thread.
            // Need to come up with a way of calling ShutDown from a non-daemon thread.

            // make sure shutdown is called just once
            //if (!shutDown) ShutDown();
        }

        /// <summary>
        /// A flag to track the startup status.
        /// </summary>
        protected static bool startedUp = false;

        /// <summary>
        /// A startup hook that Dotfuscator can set the Setup attribute for.
        /// </summary>
        protected static void StartUp()
        {
            startedUp = true;
        }

        /// <summary>
        /// A flag to track the shutdown status.
        /// </summary>
        protected static bool shutDown = false;

        /// <summary>
        /// A shutdown hook that Dotfuscator can set the Teardown attribute for.
        /// </summary>
        protected static void ShutDown() { shutDown = true; }

        #endregion
    }
}
