// Copyright (c) 2010 Xomega.Net. All rights reserved.

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Browser;
using System.ServiceModel;

namespace Xomega.Framework
{
    /// <summary>
    /// A client session manager for Silverlight communication to WCF services
    /// based on a basic HTTP binding, which doesn't support sessions by default.
    /// This class helps work around this issue by sending a session ID
    /// in a custom HTTP header that can be processed by a custom
    /// service instance provider
    /// (see <see cref="Xomega.Framework.Services.HttpSessionInstanceBehavior"/> class).
    /// </summary>
    public class ClientSessionManager
    {
        /// <summary>
        /// A dictionary of session IDs by the remote address of the WCF service.
        /// </summary>
        private static Dictionary<Uri, string> sessions = new Dictionary<Uri, string>();

        /// <summary>
        /// A static instance of a special web request creator that adds
        /// the session ID information as an HTTP header if applicable.
        /// </summary>
        private static RequestCreator requestCreator = new RequestCreator();

        /// <summary>
        /// Registers an unopen channel with the client session manager.
        /// When the channel is open it will be issued a new session ID,
        /// which will be used for all requests to the channel's remote address
        /// until the channel is closed.
        /// </summary>
        /// <param name="channel"></param>
        public static void Register(IClientChannel channel)
        {
            if (channel.State != CommunicationState.Created) throw new ArgumentException(
                "The client channel should not have been used when registering it with the ClientSessionManager.");
            channel.Opened += delegate(object sender, EventArgs e)
            {
                Uri uri = channel.RemoteAddress.Uri;
                sessions[uri] = Guid.NewGuid().ToString();
                WebRequest.RegisterPrefix(uri.ToString(), requestCreator);
            };
            channel.Closing += delegate(object sender, EventArgs e)
            {
                sessions.Remove(channel.RemoteAddress.Uri);
            };
        }

        /// <summary>
        /// An implementation of a web request creator that adds
        /// the session ID information as an HTTP header if applicable.
        /// </summary>
        private class RequestCreator : IWebRequestCreate
        {
            /// <summary>
            /// Creates a web request based on the given target Uri
            /// and adds a session ID header to it if the session is
            /// available for the given Uri.
            /// </summary>
            /// <param name="uri">The target Uri to create a web request for.</param>
            /// <returns>A web request with a SessionId header where applicable.</returns>
            public WebRequest Create(Uri uri)
            {
                WebRequest req = WebRequestCreator.ClientHttp.Create(uri);
                string sessionId;
                if (sessions.TryGetValue(uri, out sessionId))
                    req.Headers["SessionId"] = sessionId;
                return req;
            }
        }
    }
}
