// Copyright (c) 2010-2013 Xomega.Net. All rights reserved.

using System;
using System.Collections.Generic;
using System.Net;
using System.Resources;
using System.Runtime.Serialization;
using System.ServiceModel;
#if ! SILVERLIGHT && ! CLIENT_PROFILE
using System.ServiceModel.Web;
using System.Web;
#endif
namespace Xomega.Framework
{
    /// <summary>
    /// A list of error messages and utility methods to manipulate them.
    /// </summary>
    [DataContract]
    public class ErrorList
    {
        /// <summary>
        /// A static resource manager, which can be used to translate the error messages
        /// to the current language.
        /// </summary>
        public static ResourceManager ResourceManager;

        /// <summary>
        /// A singleton current error list to return in the absence of the current operation context.
        /// </summary>
        private static ErrorList current;

        private static string ErrorsKey = "Errors";

        /// <summary>
        /// Returns the error list associated with the current operation context.
        /// Service operations should use this error list to report the errors back to the caller.
        /// </summary>
        public static ErrorList Current
        {
            get
            {
                object errors;
                if (OperationContext.Current != null)
                {
                    if (!OperationContext.Current.OutgoingMessageProperties.TryGetValue(ErrorsKey, out errors))
                        OperationContext.Current.OutgoingMessageProperties.Add(ErrorsKey, errors = new ErrorList());
                }
#if ! SILVERLIGHT && ! CLIENT_PROFILE
                else if (HttpContext.Current != null)
                {
                    errors = HttpContext.Current.Items[ErrorsKey];
                    if (errors == null) HttpContext.Current.Items.Add(ErrorsKey, errors = new ErrorList());
                }
#endif
                else errors = current ?? (current = new ErrorList());
                return errors as ErrorList;
            }
        }

        /// <summary>
        /// Retrieves the error list from the specified exception if possible,
        /// otherwise constructs a new error list with the exception as the error message.
        /// </summary>
        /// <param name="ex">Exception to retrieve the error list from.</param>
        /// <returns>An error list retrieved from the exception.</returns>
        public static ErrorList FromException(Exception ex)
        {
            FaultException<ErrorList> fex = ex as FaultException<ErrorList>;
            if (fex != null) return fex.Detail;

            WebException webEx = ex as WebException;;
            webEx = webEx ?? ex.InnerException as WebException;
            if (webEx != null && webEx.Response != null && webEx.Response.GetResponseStream() != null)
            {
                try
                {
                    return (ErrorList)new DataContractSerializer(typeof(ErrorList)).ReadObject(
                        webEx.Response.GetResponseStream());
                }
                catch (Exception) {}
            }

            ErrorList err = new ErrorList();
            err.Add(new ErrorMessage("EXCEPTION", ex.ToString()));
            return err;
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
        private string GetMessage(string code, object[] parameters)
        {
            string message = null;
            if (ResourceManager != null) message = ResourceManager.GetString(code);
            if (message == null) message = code;
            return string.Format(message, parameters);
        }

        /// <summary>
        /// Adds an error to the list with the given error code and additional parameters to substitute.
        /// </summary>
        /// <param name="code">The error code.</param>
        /// <param name="parameters">An array of parameters to substitute into the message placeholders.</param>
        public void AddError(string code, params object[] parameters)
        {
            Add(new ErrorMessage(code, GetMessage(code, parameters), ErrorSeverity.Error));
        }

        /// <summary>
        /// Adds a critical error to the list with the given error code and additional parameters to substitute
        /// and aborts the current operation.
        /// </summary>
        /// <param name="code">The error code.</param>
        /// <param name="parameters">An array of parameters to substitute into the message placeholders.</param>
        public void CriticalError(string code, params object[] parameters) { CriticalError(code, true, parameters); }

        /// <summary>
        /// Adds a critical error to the list with the given error code and additional parameters to substitute
        /// and aborts the current operation with the reason being this message if required.
        /// </summary>
        /// <param name="code">The error code.</param>
        /// <param name="abort">True to abort the current operation.</param>
        /// <param name="parameters">An array of parameters to substitute into the message placeholders.</param>
        public void CriticalError(string code, bool abort, params object[] parameters)
        {
            ErrorMessage err = new ErrorMessage(code, GetMessage(code, parameters), ErrorSeverity.Critical);
            Add(err);
            if (abort) Abort(err.Message);
        }

        /// <summary>
        /// Adds a critical error to the list with the given error code and additional parameters to substitute
        /// and aborts the current operation.
        /// </summary>
        /// <param name="status">HTTP status of the operation to report.</param>
        /// <param name="code">The error code.</param>
        /// <param name="parameters">An array of parameters to substitute into the message placeholders.</param>
        public void CriticalError(HttpStatusCode status,  string code, params object[] parameters)
        {
            CriticalError(status, code, true, parameters);
        }

        /// <summary>
        /// Adds a critical error to the list with the given error code and additional parameters to substitute
        /// and aborts the current operation with the reason being this message if required.
        /// </summary>
        /// <param name="status">HTTP status of the operation to report.</param>
        /// <param name="code">The error code.</param>
        /// <param name="abort">True to abort the current operation.</param>
        /// <param name="parameters">An array of parameters to substitute into the message placeholders.</param>
        public void CriticalError(HttpStatusCode status, string code, bool abort, params object[] parameters)
        {
            Add(new ErrorMessage(code, GetMessage(code, parameters), ErrorSeverity.Critical));
            if (abort) Abort(status);
        }

        /// <summary>
        /// Adds a warning to the list with the given error code and additional parameters to substitute.
        /// </summary>
        /// <param name="code">The error code.</param>
        /// <param name="parameters">An array of parameters to substitute into the message placeholders.</param>
        public void AddWarning(string code, params object[] parameters)
        {
            Add(new ErrorMessage(code, GetMessage(code, parameters), ErrorSeverity.Warning));
        }

        /// <summary>
        /// Adds an info message to the list with the given error code and additional parameters to substitute.
        /// </summary>
        /// <param name="code">The message code.</param>
        /// <param name="parameters">An array of parameters to substitute into the message placeholders.</param>
        public void AddInfo(string code, params object[] parameters)
        {
            Add(new ErrorMessage(code, GetMessage(code, parameters), ErrorSeverity.Info));
        }

        /// <summary>
        /// Aborts the current operation with the specified HTTP error status by throwing a WebFaultException.
        /// </summary>
        /// <param name="status">The HTTP (error) status for aborted operation.</param>
        public void Abort(HttpStatusCode status)
        {
#if SILVERLIGHT || CLIENT_PROFILE
            Abort(status.ToString());
#else
            throw new WebFaultException<ErrorList>(this, status);
#endif
        }

        /// <summary>
        /// Aborts the current operation with the specified reason by throwing a FaultException.
        /// </summary>
        /// <param name="reason">The reason for aborting the operation.</param>
        public void Abort(string reason)
        {
            FaultCode cd = new FaultCode("Sender");
            throw new FaultException<ErrorList>(this, new FaultReason(reason), cd, null);
        }

        /// <summary>
        /// Aborts the current operation in the current list has any errors.
        /// </summary>
        public void AbortIfHasErrors()
        {
            if (HasErrors()) Abort(ErrorsText);
        }

        /// <summary>
        /// Aborts the current operation in the current list has any errors.
        /// </summary>
        /// <param name="status">The HTTP (error) status for aborted operation.</param>
        public void AbortIfHasErrors(HttpStatusCode status)
        {
            if (HasErrors()) Abort(status);
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
        public void Add(ErrorMessage err)
        {
            errors.Add(err);
        }

        /// <summary>
        /// Merges the current list with another error list.
        /// </summary>
        /// <param name="otherList">Another error list to merge the current list with.</param>
        public void MergeWith(ErrorList otherList)
        {
            if (otherList != null) errors.AddRange(otherList.Errors);
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
    }
}
