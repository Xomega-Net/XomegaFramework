// Copyright (c) 2010-2013 Xomega.Net. All rights reserved.

using System;
using System.Collections.Generic;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace Xomega.Framework.Services
{
    /// <summary>
    /// Behavior extension element to create <see cref="HttpSessionInstanceProvider"/>.
    /// </summary>
    public class HttpSessionInstanceBehavior : BehaviorExtensionElement
    {
        /// <summary>
        /// Initializes a new instance of the HttpSessionInstanceBehavior class.
        /// </summary>
        public HttpSessionInstanceBehavior() {}

        /// <summary>
        /// Creates a behavior extension based on the current configuration settings.
        /// </summary>
        /// <returns>The behavior extension.</returns>
        protected override object CreateBehavior() { return new HttpSessionInstanceProvider(); }

        /// <summary>
        /// Gets the type of behavior.
        /// </summary>
        public override Type BehaviorType { get { return typeof(HttpSessionInstanceProvider); } }
    }

    /// <summary>
    /// An endpoint behavior that implements a new instance provider, which supports sessions
    /// for basic HTTP binding to work around the Silverlight 3 limitation.
    /// The session information should be passed via an HTTP header SessionId.
    /// To release the instance when an HTTP-based session is used an operation must set
    /// the ReleaseInstance property on the outgoing message to true
    /// (see <see cref="EntityServiceBase&lt;T&gt;.EndSession()"/> method).
    /// This behavior also implements a dispatch message inspector that updates the HTTP status code
    /// for fault replies to be 200 (OK) so that Silverlight clients could handle the fault exception.
    /// </summary>
    public class HttpSessionInstanceProvider : IInstanceProvider, IEndpointBehavior, IDispatchMessageInspector
    {
        private const string SessionIdKey = "SessionId";

        private Type svcType = null;
        private Dictionary<string, object> instances = new Dictionary<string, object>();

        #region IEndpointBehavior Members

        /// <summary>
        /// IEndpointBehavior method that is not implemented in this class.
        /// </summary>
        /// <param name="endpoint">The endpoint to modify.</param>
        /// <param name="bindingParameters">The objects that binding elements require to support the behavior.</param>
        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
        }

        /// <summary>
        /// IEndpointBehavior method that is not implemented in this class.
        /// </summary>
        /// <param name="endpoint">The endpoint that is to be customized.</param>
        /// <param name="clientRuntime">The client runtime to be customized.</param>
        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
        }

        /// <summary>
        /// Implements a modification or extension of the service across an endpoint
        /// by setting the instance provider to the current class.
        /// </summary>
        /// <param name="endpoint">The endpoint that exposes the contract.</param>
        /// <param name="endpointDispatcher">The endpoint dispatcher to be modified or extended.</param>
        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
            svcType = endpointDispatcher.DispatchRuntime.Type;
            endpointDispatcher.DispatchRuntime.InstanceProvider = this;
            endpointDispatcher.DispatchRuntime.MessageInspectors.Add(this);
        }

        /// <summary>
        /// IEndpointBehavior method that is not implemented in this class.
        /// </summary>
        /// <param name="endpoint">The endpoint to validate.</param>
        public void Validate(ServiceEndpoint endpoint)
        {
        }

        #endregion

        #region IInstanceProvider Members

        /// <summary>
        /// Returns a service object given the specified InstanceContext object.
        /// An existing object is returned for an existing session passed in the HTTP header.
        /// </summary>
        /// <param name="instanceContext">The current InstanceContext object.</param>
        /// <param name="message">The message that triggered the creation of a service object.</param>
        /// <returns>The service object.</returns>
        public object GetInstance(InstanceContext instanceContext, Message message)
        {
            return GetInstance(instanceContext);
        }

        /// <summary>
        /// Returns a service object given the specified InstanceContext object.
        /// An existing object is returned for an existing session passed in the HTTP header.
        /// </summary>
        /// <param name="instanceContext">The current InstanceContext object.</param>
        /// <returns>The service object.</returns>
        public object GetInstance(InstanceContext instanceContext)
        {
            object instance;
            string sessionId = GetIncomingSessionId();
            if (sessionId == null || !instances.TryGetValue(sessionId, out instance))
            {
                instance = Activator.CreateInstance(svcType);
                if (sessionId != null) instances.Add(sessionId, instance);
            }
            return instance;
        }

        /// <summary>
        /// Called when an InstanceContext object recycles a service object.
        /// Releases the service instance if there is no session or if an outgoing
        /// message property ReleaseInstance is set to true by the current operation.
        /// </summary>
        /// <param name="instanceContext">The service's instance context.</param>
        /// <param name="instance">The service object to be recycled.</param>
        public void ReleaseInstance(InstanceContext instanceContext, object instance)
        {
            object sessionId = null, release;
            if (OperationContext.Current != null && (
                !OperationContext.Current.OutgoingMessageProperties.TryGetValue(SessionIdKey, out sessionId) ||
                OperationContext.Current.OutgoingMessageProperties.TryGetValue("ReleaseInstance", out release) && 
                true.Equals(release)))
            {
                IDisposable disp = instance as IDisposable;
                if (disp != null) disp.Dispose();
                if (sessionId != null) instances.Remove(Convert.ToString(sessionId));
            }
        }

        #endregion

        #region IDispatchMessageInspector Members

        /// <summary>
        /// IDispatchMessageInspector method that does nothing in this class.
        /// Called after an inbound message has been received but before the message is dispatched to the intended operation.
        /// </summary>
        /// <param name="request">The request message.</param>
        /// <param name="channel">The incoming channel.</param>
        /// <param name="instanceContext">The current service instance.</param>
        /// <returns>The object used to correlate state.</returns>
        public object AfterReceiveRequest(ref Message request, IClientChannel channel, InstanceContext instanceContext)
        {
            return null;
        }

        /// <summary>
        /// IDispatchMessageInspector method that is called after the operation has returned but before the reply message is sent.
        /// It updates the HTTP status code to OK (200) to enable returning fault exceptions to Silverlight clients.
        /// </summary>
        /// <param name="reply">The reply message. This value is null if the operation is one way.</param>
        /// <param name="correlationState">The correlation object returned from the AfterReceiveRequest method.</param>
        public void BeforeSendReply(ref Message reply, object correlationState)
        {
            if (reply.IsFault)
            {
                HttpResponseMessageProperty property = new HttpResponseMessageProperty();

                // Here the response code is changed to 200.
                property.StatusCode = HttpStatusCode.OK;
                reply.Properties[HttpResponseMessageProperty.Name] = property;
            }
        }

        #endregion

        /// <summary>
        /// Retrieves the session ID from the HTTP header of the current message.
        /// </summary>
        /// <returns>The session ID or null if no session is found.</returns>
        protected string GetIncomingSessionId()
        {
            if (OperationContext.Current != null && OperationContext.Current.IncomingMessageProperties != null)
            {
                foreach (object prop in OperationContext.Current.IncomingMessageProperties.Values)
                {
                    HttpRequestMessageProperty req = prop as HttpRequestMessageProperty;
                    if (req != null)
                    {
                        string sessionId = req.Headers[SessionIdKey];
                        if (sessionId != null) OperationContext.Current.OutgoingMessageProperties.Add(SessionIdKey, sessionId);
                        return sessionId;
                    }
                }
            }
            return null;
        }
    }
}
