// Copyright (c) 2017 Xomega.Net. All rights reserved.

namespace Xomega.Framework
{
    /// <summary>
    /// A class that represents a single view event or a combination of
    /// multiple view events for notifying of several events at once.
    /// </summary>
    public class ViewEvent
    {
        /// <summary>
        /// A static constant representing a combination of all events.
        /// </summary>
        public static readonly ViewEvent All = new ViewEvent(0xFFFF);

        /// <summary>
        /// A static constant representing a Closed event
        /// </summary>
        public static readonly ViewEvent Closed = new ViewEvent(1 << 0);

        /// <summary>
        /// A static constant representing a Saved event
        /// </summary>
        public static readonly ViewEvent Saved = new ViewEvent(1 << 1);

        /// <summary>
        /// A static constant representing a Deleted event
        /// </summary>
        public static readonly ViewEvent Deleted = new ViewEvent(1 << 2);

        /// <summary>
        /// A static constant representing a Child view event
        /// </summary>
        public static readonly ViewEvent Child = new ViewEvent(1 << 3);

        /// <summary>
        /// Internal bitmask integer representing the view event(s).
        /// </summary>
        private int events;

        /// <summary>
        /// Constructs a view event class.
        /// </summary>
        /// <param name="events">The event(s) bitmask.</param>
        protected ViewEvent(int events)
        {
            this.events = events;
        }

        /// <summary>
        /// Returns if the view was closed.
        /// </summary>
        /// <returns>True if the view was closed, false otherwise.</returns>
        public bool IsClosed(bool self = true) { return (self && !IsChild() || !self) && (events & Closed.events) > 0; }

        /// <summary>
        /// Returns if the view was saved.
        /// </summary>
        /// <returns>True if the view was saved, false otherwise.</returns>
        public bool IsSaved(bool self = true) { return (self && !IsChild() || !self) && (events & Saved.events) > 0; }

        /// <summary>
        /// Returns if the view was deleted.
        /// </summary>
        /// <returns>True if the view was deleted, false otherwise.</returns>
        public bool IsDeleted(bool self = true) { return (self && !IsChild() || !self) && (events & Deleted.events) > 0; }

        /// <summary>
        /// Returns if a child view event occured.
        /// </summary>
        /// <returns>True if a child view event occured, false otherwise.</returns>
        public bool IsChild() { return (events & Child.events) > 0; }

        /// <summary>
        /// Combines two view events and returns the event that represents the combination.
        /// </summary>
        /// <param name="lhs">Left-hand side view event.</param>
        /// <param name="rhs">Right-hand side view event.</param>
        /// <returns>The combination of the two view events.</returns>
        public static ViewEvent operator +(ViewEvent lhs, ViewEvent rhs)
        {
            return new ViewEvent(lhs.events | rhs.events);
        }

        /// <summary>
        /// Removes the right-hand side view event from the left-hand side combination of events.
        /// </summary>
        /// <param name="lhs">The combination of view events to remove the event from.</param>
        /// <param name="rhs">The view event to remove from the left-hand side combination.</param>
        /// <returns>The left-hand side view event without the right-hand side event.</returns>
        public static ViewEvent operator -(ViewEvent lhs, ViewEvent rhs)
        {
            return new ViewEvent(lhs.events & ~rhs.events);
        }
    }
}
