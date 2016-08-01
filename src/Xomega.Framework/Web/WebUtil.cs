// Copyright (c) 2010-2016 Xomega.Net. All rights reserved.

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
        /// A templated method to retrieve Xomega data objects from the session scope.
        /// If the object is not yet cached or if <paramref name="createNew"/> flag is specified,
        /// the object will be constructed and stored in the session scope with the given key.
        /// </summary>
        /// <typeparam name="T">The data object type.</typeparam>
        /// <param name="cacheKey">A string key for storing and accessing the object.</param>
        /// <param name="createNew">True to create and store the object even if it already exists in the session.</param>
        /// <returns>The data object that has been created or retrieved from the session.</returns>
        public static T GetCachedObject<T>(string cacheKey, bool createNew) where T : class, new()
        {
            if (HttpContext.Current == null || HttpContext.Current.Session == null)
                return null;

            T obj = HttpContext.Current.Session[cacheKey] as T;
            if (obj == null || createNew)
                HttpContext.Current.Session.Add(cacheKey, obj = new T());
            return obj;
        }

        /// <summary>
        /// Binds a control or all of its child controls (e.g. for a containing panel)
        /// to the given data object (or its child object)
        /// as specified by the controls' Property and ChildObject attributes.
        /// </summary>
        /// <param name="ctl">Web control to bind to the given data object.</param>
        /// <param name="obj">Data object to bind the web control to.</param>
        public static void BindToObject(Control ctl, DataObject obj)
        {
            BindToObject(ctl, obj, false);
        }

        /// <summary>
        /// Binds a control or all of its child controls (e.g. for a containing panel)
        /// to the given data object (or its child object)
        /// as specified by the controls' Property and ChildObject attributes.
        /// </summary>
        /// <param name="ctl">Control to bind to the given data object.</param>
        /// <param name="obj">Data object to bind the control to.</param>
        /// <param name="bindCurrentRow">For list objects specifies whether to bind the whole list or just the current row.</param>
        public static void BindToObject(Control ctl, DataObject obj, bool bindCurrentRow)
        {
            if (obj == null || ctl == null) return;

            AttributeCollection attr = WebPropertyBinding.GetControlAttributes(ctl);
            string childPath = attr != null ? attr[WebPropertyBinding.AttrChildObject] : null;
            IDataObject cObj = WebPropertyBinding.FindChildObject(obj, childPath);
            obj = cObj as DataObject;
            string propertyName = attr != null ? attr[WebPropertyBinding.AttrProperty] : null;
            if (obj != null && propertyName != null)
            {
                WebPropertyBinding binding = WebPropertyBinding.Create(ctl) as WebPropertyBinding;
                if (binding != null) binding.BindTo(obj[propertyName]);
                // remove attributes that are no longer needed to minimize HTML
                attr.Remove(WebPropertyBinding.AttrChildObject);
                attr.Remove(WebPropertyBinding.AttrProperty);
            }
            else if (cObj is DataListObject && !bindCurrentRow) BindToList(ctl, (DataListObject)cObj);
            else foreach (Control c in ctl.Controls)
                BindToObject(c, obj, bindCurrentRow);
        }

        /// <summary>
        /// Binds the specified control to the given data object list.
        /// </summary>
        /// <param name="ctl">Control to bind to the given data object list.</param>
        /// <param name="list">Data object list to bind the control to.</param>
        public static void BindToList(Control ctl, DataListObject list)
        {
            if (list == null || ctl == null) return;

            IListBindable listBinding = WebPropertyBinding.Create(ctl) as IListBindable;
            if (listBinding != null) listBinding.BindTo(list);
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
