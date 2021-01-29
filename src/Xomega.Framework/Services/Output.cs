// Copyright (c) 2021 Xomega.Net. All rights reserved.

using System.Net;
using System.Runtime.Serialization;

namespace Xomega.Framework.Services
{
    /// <summary>
    /// The class that encapsulates the output of service operations,
    /// which includes any error, warning or info messages generated from the operation.
    /// </summary>
    [DataContract]
    public class Output
    {
        /// <summary>
        /// The list of error, warning or info messages generated from the operation.
        /// </summary>
        [DataMember]
        public ErrorList Messages { get; set; }
        
        /// <summary>
        /// Default construction to support deserialization
        /// </summary>
        public Output() { }

        /// <summary>
        /// Constructs a new output with the given list of messages.
        /// </summary>
        /// <param name="messageList"></param>
        public Output(ErrorList messageList)
        {
            Messages = messageList;
        }

        /// <summary>
        /// HTTP status code for the current operation, which comes from the error message list.
        /// </summary>
        public HttpStatusCode HttpStatus
        {
            get
            {
                return Messages != null ? Messages.HttpStatus : HttpStatusCode.OK;
            }
        }
    }

    /// <summary>
    /// An output of a service operation that includes a specific result,
    /// in addition to the list of generated messages.
    /// </summary>
    /// <typeparam name="T">The type of the result from the operation.</typeparam>
    [DataContract]
    public class Output<T> : Output where T : class
    {
        /// <summary>
        /// The actual result of the service operation, when it was successful.
        /// </summary>
        [DataMember]
        public T Result { get; set; }

        /// <summary>
        /// Default construction to support deserialization
        /// </summary>
        public Output() { }

        /// <summary>
        /// Constructs an output with a given message list and a specific result.
        /// </summary>
        /// <param name="messageList">The message list for the current operation.</param>
        /// <param name="result">The result of the current operation.</param>
        public Output(ErrorList messageList, T result) : base(messageList)
        {
            Result = result;
        }
    }
}
