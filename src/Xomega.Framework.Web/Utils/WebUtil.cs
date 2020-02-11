// Copyright (c) 2019 Xomega.Net. All rights reserved.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Xomega.Framework.Web
{
    /// <summary>
    /// A class that contains a set of static utility methods to support 
    /// usage of Xomega Framework in ASP.NET web applications.
    /// </summary>
    public class WebUtil
    {
        /// <summary>
        /// Determines if the current request uses a POST method. This is different
        /// than IsPostBack flag, which is false when you do Server.Transfer().
        /// </summary>
        /// <returns>True, if the current request uses a POST method, otherwise false.</returns>
        public static bool IsPostRequest()
        {
            return (HttpContext.Current != null && HttpContext.Current.Request.HttpMethod == "POST");
        }

        /// <summary>
        /// Get the current execution path including the query string.
        /// </summary>
        /// <returns>The current execution path including the query string.</returns>
        public static string GetCurrentPath()
        {
            if (HttpContext.Current == null) return null;

            HttpRequest request = HttpContext.Current.Request;
            string path = request.CurrentExecutionFilePath;
            if (request.QueryString.Count > 0) path += "?" + request.QueryString;
            return path;
        }

        /// <summary>
        /// A utility method to add or remove the specific CSS class to/from the existing list of CSS classes.
        /// </summary>
        /// <param name="cssClasses">A space-delimited list of CSS classes.</param>
        /// <param name="cssClass">The CSS class to add or remove if needed.</param>
        /// <param name="add">True to add, false to remove the given class.</param>
        /// <returns>Updated list of CSS classes with the given class added or removed.</returns>
        public static string AddOrRemoveClass(string cssClasses, string cssClass, bool add)
        {
            List<string> classes = (cssClasses ?? "").Split(new char[] { ' ' },
                StringSplitOptions.RemoveEmptyEntries).ToList();
            if (add && !classes.Contains(cssClass)) classes.Add(cssClass);
            else if (!add && classes.Contains(cssClass)) classes.Remove(cssClass);
            return string.Join(" ", classes.ToArray());
        }

        /// <summary>
        /// Finds a parent udpate panel for the given control.
        /// </summary>
        /// <param name="ctl">Control to find parent update panel for</param>
        /// <returns>Parent update panel</returns>
        public static UpdatePanel FindParentUpdatePanel(Control ctl)
        {
            Control p = ctl.Parent;
            while (p != null && p as UpdatePanel == null && p.Parent != null) p = p.Parent;
            return p as UpdatePanel;
        }

        /// <summary>
        /// Sets visibility for control that may be made visible during an async task
        /// by calling the corresponding method on the control's <see cref="WebPage"/>,
        /// which tracks this control to set its visibility during the proper life cycle event,
        /// in order to work around issues with extenders' visibility.
        /// </summary>
        /// <param name="control">Control that may be made visible during an async task.</param>
        /// <param name="visible">True to make it visible, false otherwise.</param>
        public static void SetControlVisible(Control control, bool visible)
        {
            if (control?.Page is WebPage wp)
                wp.SetControlVisible(control, visible);
            else if (control != null) control.Visible = visible;
        }

        /// <summary>
        /// Adds a query string from NameValueCollection object to the url.
        /// </summary>
        public static string AddQueryString(string url, NameValueCollection nvc)
        {
            NameValueCollection query = HttpUtility.ParseQueryString(string.Empty);
            foreach (string k in nvc.Keys) query[k] = nvc[k];
            return (url == null ? "" : url.Split('?')[0]) + (query.Keys.Count == 0 ? "" : "?" + query.ToString());
        }

        /// <summary>Script to run code on document ready.</summary>
        private const string Script_OnDocumentReady = "$(document).ready(function() {{ {0} }});";

        /// <summary>
        /// Utility function to register a startup script.
        /// </summary>
        /// <param name="ctl">The control that is registering the client script block.</param>
        /// <param name="key">Unique key to use with the control ID when registering the script.</param>
        /// <param name="script">JavaScript text with placeholders.</param>
        /// <param name="args">Arguments for the placeholders.</param>
        public static void RegisterStartupScript(Control ctl, string key, string script, params object[] args)
        {
            if (!script.EndsWith(";")) script += ";";
            string formattedScript = args.Length == 0 ? script : string.Format(script, args);

            UpdatePanel upl = FindParentUpdatePanel(ctl);
            if (upl != null)
            {
                ScriptManager.RegisterStartupScript(
                    ctl,
                    ctl.GetType(),
                    ctl.ClientID + "_" + key,
                    formattedScript,
                    true);
            }
            else
            {
                ctl.Page.ClientScript.RegisterStartupScript(
                    ctl.Page.GetType(),
                    ctl.Page.ClientID + "_" + key,
                    string.Format(Script_OnDocumentReady, formattedScript),
                    true);
            }
        }
    }
}
