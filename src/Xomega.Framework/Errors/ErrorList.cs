// Copyright (c) 2020 Xomega.Net. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Resources;
using System.Runtime.Serialization;

namespace Xomega.Framework
{
    /// <summary>
    /// A list of error messages and utility methods to manipulate them.
    /// </summary>
    [DataContract]
    public class ErrorList
    {
        /// <summary>
        /// A resource manager, which can be used to translate the error messages to the current language.
        /// </summary>
        private ResourceManager resources;

        /// <summary>
        /// Constructs a new error list
        /// </summary>
        public ErrorList() : this(null)
        {
        }

        /// <summary>
        /// Constructs a new error list with specified resource manager
        /// </summary>
        /// <param name="resources">Resource manager to use for error messages</param>
        public ErrorList(ResourceManager resources)
        {
            this.resources = resources ?? Messages.ResourceManager;
        }

        /// <summary>
        /// Internal list of error messages.
        /// </summary>
        protected List<ErrorMessage> errors = new List<ErrorMessage>();

        /// <summary>
        /// Gets the text message based on the given error code and parameters.
        /// Uses the resource manager if set to look up the localized message by the error code.
        /// </summary>
        /// <param name="code">The error code.</param>
        /// <param name="parameters">An array of parameters to substitute into the message placeholders.</param>
        /// <returns>Localized message  for the given error code with substituted parameters.</returns>
        public string GetMessage(string code, object[] parameters)
        {
            string message = null;
            if (resources != null) message = resources.GetString(code);
            if (message == null) message = code;
            try
            {
                message = string.Format(message, parameters);
            }
            catch (Exception) { } // return message as is, if the format is bad.
            return message;
        }

        /// <summary>
        /// Adds an error to the list with the given error code and additional parameters to substitute.
        /// </summary>
        /// <param name="code">The error code.</param>
        /// <param name="parameters">An array of parameters to substitute into the message placeholders.</param>
        /// <returns>The error message added to the list for further configuration.</returns>
        public ErrorMessage AddValidationError(string code, params object[] parameters)
        {
            return Add(new ErrorMessage(ErrorType.Validation, code, GetMessage(code, parameters), ErrorSeverity.Error));
        }

        /// <summary>
        /// Adds an error to the list with the given error code and additional parameters to substitute.
        /// </summary>
        /// <param name="code">The error code.</param>
        /// <param name="parameters">An array of parameters to substitute into the message placeholders.</param>
        /// <param name="type">The type of error.</param>
        /// <returns>The error message added to the list for further configuration.</returns>
        public ErrorMessage AddError(ErrorType type, string code, params object[] parameters)
        {
            return Add(new ErrorMessage(type, code, GetMessage(code, parameters), ErrorSeverity.Error));
        }

        /// <summary>
        /// Adds a critical error to the list with the given error code and additional parameters to substitute
        /// and aborts the current operation.
        /// </summary>
        /// <param name="code">The error code.</param>
        /// <param name="parameters">An array of parameters to substitute into the message placeholders.</param>
        /// <param name="type">The type of error.</param>
        public void CriticalError(ErrorType type, string code, params object[] parameters) { CriticalError(type, code, true, parameters); }

        /// <summary>
        /// Adds a critical error to the list with the given error code and additional parameters to substitute
        /// and aborts the current operation with the reason being this message if required.
        /// </summary>
        /// <param name="code">The error code.</param>
        /// <param name="abort">True to abort the current operation.</param>
        /// <param name="parameters">An array of parameters to substitute into the message placeholders.</param>
        /// <param name="type">The type of error.</param>
        /// <returns>The error message added to the list, if not aborted.</returns>
        public ErrorMessage CriticalError(ErrorType type, string code, bool abort, params object[] parameters)
        {
            ErrorMessage err = new ErrorMessage(type, code, GetMessage(code, parameters), ErrorSeverity.Critical);
            Add(err);
            if (abort) Abort(err.Message);
            return err;
        }

        /// <summary>
        /// Adds a warning to the list with the given error code and additional parameters to substitute.
        /// </summary>
        /// <param name="code">The error code.</param>
        /// <param name="parameters">An array of parameters to substitute into the message placeholders.</param>
        /// <returns>The error message added to the list for further configuration.</returns>
        public ErrorMessage AddWarning(string code, params object[] parameters)
        {
            return Add(new ErrorMessage(ErrorType.Message, code, GetMessage(code, parameters), ErrorSeverity.Warning));
        }

        /// <summary>
        /// Adds an info message to the list with the given error code and additional parameters to substitute.
        /// </summary>
        /// <param name="code">The message code.</param>
        /// <param name="parameters">An array of parameters to substitute into the message placeholders.</param>
        /// <returns>The error message added to the list for further configuration.</returns>
        public ErrorMessage AddInfo(string code, params object[] parameters)
        {
            return Add(new ErrorMessage(ErrorType.Message, code, GetMessage(code, parameters), ErrorSeverity.Info));
        }

        /// <summary>
        /// Aborts the current operation with the specified reason by throwing an ErrorAbortException.
        /// </summary>
        /// <param name="reason">The reason for aborting the operation.</param>
        public virtual void Abort(string reason)
        {
            throw new ErrorAbortException(reason, this);
        }

        /// <summary>
        /// Aborts the current operation in the current list has any errors.
        /// </summary>
        public void AbortIfHasErrors()
        {
            if (HasErrors()) Abort(ErrorsText);
        }

        /// <summary>
        /// Checks if the current list has any errors or critical errors.
        /// </summary>
        /// <returns>True if the current list has any errors or critical errors, otherwise false.</returns>
        public bool HasErrors()
        {
            foreach (ErrorMessage e in errors) if (e.Severity > ErrorSeverity.Warning) return true;
            return false;
        }

        /// <summary>
        /// Adds the given error message to the list.
        /// </summary>
        /// <param name="err">Error message to add to the list.</param>
        public ErrorMessage Add(ErrorMessage err)
        {
            errors.Add(err);
            return err;
        }

        /// <summary>
        /// Merges the current list with another error list.
        /// </summary>
        /// <param name="otherList">Another error list to merge the current list with.</param>
        public void MergeWith(ErrorList otherList)
        {
            if (otherList != null && otherList != this)
                errors.AddRange(otherList.Errors);
        }

        /// <summary>
        /// Clears the error list.
        /// </summary>
        public void Clear() { errors.Clear(); }

        /// <summary>
        /// Returns a read-only collection of error messages from this list.
        /// </summary>
        [DataMember]
        public ICollection<ErrorMessage> Errors
        {
            get { return errors.AsReadOnly(); }
            set
            {
                // this is to support deserialization that doesn't have access to private members (e.g. in Silverlight)
                if (errors == null) errors = new List<ErrorMessage>(value);
            }
        }

        /// <summary>
        ///  Gets a combined error text by concatenating all error messages with a new line delimiter.
        /// </summary>
        public string ErrorsText
        {
            get
            {
                string errText = "";
                foreach (ErrorMessage err in errors)
                {
                    if (!string.IsNullOrEmpty(errText)) errText += "\n";
                    errText += err.Message;
                }
                return errText;
            }
        }

        // explicitly set HTTP status code
        private HttpStatusCode? httpStatus;

        /// <summary>
        /// HTTP status code associated with the current error list for REST services
        /// </summary>
        public HttpStatusCode HttpStatus
        {
            get
            {
                if (httpStatus != null) return httpStatus.Value;
                if (errors.Count == 0) return HttpStatusCode.OK;
                return errors.Max(e => e.HttpStatus);
            }
            set { httpStatus = value; }
        }
    }
}
