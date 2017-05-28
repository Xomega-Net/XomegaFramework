var __extends = (this && this.__extends) || (function () {
    var extendStatics = Object.setPrototypeOf ||
        ({ __proto__: [] } instanceof Array && function (d, b) { d.__proto__ = b; }) ||
        function (d, b) { for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p]; };
    return function (d, b) {
        extendStatics(d, b);
        function __() { this.constructor = d; }
        d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
    };
})();
// Copyright (c) 2017 Xomega.Net. All rights reserved.
var xomega;
(function (xomega) {
    // A list of error messages and utility methods to manipulate them.
    var ErrorList = (function () {
        // constructs a new error list object
        function ErrorList() {
            var _this = this;
            this.Errors = ko.observableArray();
            this.ErrorsText = ko.computed(function () {
                return _this.Errors().map(function (err) { return err.Message; }).join("\n");
            }, this);
        }
        // Retrieves the error list from the specified exception if possible,
        // otherwise constructs a new error list with the exception as the error message.
        // <param name="ex">Exception to retrieve the error list from.</param>
        // <returns>An error list retrieved from the exception.</returns>
        ErrorList.fromError = function (err) {
            var errList = new ErrorList();
            var errors = err["__errors__"];
            if (errors !== null)
                errList.Errors(errors);
            else
                errList.addError(err.name, err.message);
            return errList;
        };
        // Deserializes an ErrorList object from JSON that contains a serialized Xomega Framework ErrorList.
        ErrorList.fromJSON = function (obj) {
            var data = obj.Errors.map(function (val, idx, arr) { return xomega.ErrorMessage.fromJSON(val); });
            var lst = new ErrorList();
            ko.utils.arrayPushAll(lst.Errors, data);
            return lst;
        };
        // Constructs an ErrorList object from an error response to a jQuery AJAX request.
        ErrorList.fromErrorResponse = function (xhr, errorThrow) {
            if (xhr instanceof ErrorList)
                return xhr;
            if ($.type(xhr) === 'error')
                return ErrorList.fromError(xhr);
            var json = xhr.responseJSON;
            if (json && json.Errors)
                return ErrorList.fromJSON(json);
            var errLst = new ErrorList();
            if (errLst.fromExceptionJSON(json))
                return errLst;
            if (errLst.fromOAuthError(json))
                return errLst;
            errLst.Errors.push(new xomega.ErrorMessage(errorThrow, json && json.Message ? json.Message : (xhr.responseText ? xhr.responseText : errorThrow), xomega.ErrorSeverity.Error));
            return errLst;
        };
        // Populates the current error list from the exception JSON returned by the server.
        ErrorList.prototype.fromExceptionJSON = function (json) {
            if (json && json.ExceptionType) {
                this.Errors.push(new xomega.ErrorMessage(json.ExceptionType, json.ExceptionMessage, xomega.ErrorSeverity.Error));
                if (json.InnerException)
                    this.fromExceptionJSON(json.InnerException);
                return true;
            }
            return false;
        };
        // Populates the current error list from an OAuth error JSON returned by the server.
        ErrorList.prototype.fromOAuthError = function (json) {
            if (json && json.error && json.error_description) {
                this.Errors.push(new xomega.ErrorMessage(json.error, json.error_description, xomega.ErrorSeverity.Error));
                return true;
            }
            return false;
        };
        // Gets the text message based on the given error code and parameters.
        ErrorList.prototype.getMessage = function (code) {
            var params = [];
            for (var _i = 1; _i < arguments.length; _i++) {
                params[_i - 1] = arguments[_i];
            }
            return xomega.format(code, params);
        };
        // Adds an error to the list with the given error code and additional parameters to substitute.
        ErrorList.prototype.addError = function (code) {
            var params = [];
            for (var _i = 1; _i < arguments.length; _i++) {
                params[_i - 1] = arguments[_i];
            }
            this.Errors.push(new xomega.ErrorMessage(code, this.getMessage(code, params), xomega.ErrorSeverity.Error));
        };
        // Adds an error to the list with the given error code and additional parameters to substitute.
        ErrorList.prototype.addWarning = function (code) {
            var params = [];
            for (var _i = 1; _i < arguments.length; _i++) {
                params[_i - 1] = arguments[_i];
            }
            this.Errors.push(new xomega.ErrorMessage(code, this.getMessage(code, params), xomega.ErrorSeverity.Warning));
        };
        // Adds a critical error to the list with the given error code and additional parameters to substitute
        // and aborts the current operation with the reason being this message if required.
        ErrorList.prototype.criticalError = function (code, abort) {
            var params = [];
            for (var _i = 2; _i < arguments.length; _i++) {
                params[_i - 2] = arguments[_i];
            }
            var errMsg = new xomega.ErrorMessage(code, this.getMessage(code, params), xomega.ErrorSeverity.Critical);
            this.Errors.push(errMsg);
            if (abort)
                this.abort(errMsg.Message);
        };
        // Aborts the current operation with the specified reason by throwing an error.
        ErrorList.prototype.abort = function (reason) {
            var err = new Error(reason);
            err["__errors__"] = this.Errors();
            throw err;
        };
        // Checks if the current list has any errors or critical errors.
        ErrorList.prototype.hasErrors = function () {
            return this.Errors().some(function (err) { return err.Severity > xomega.ErrorSeverity.Warning; });
        };
        // Aborts the current operation in the current list has any errors.
        ErrorList.prototype.abortIfHasErrors = function () {
            if (this.hasErrors())
                this.abort(this.ErrorsText());
        };
        // Merges the current list with another error list.
        ErrorList.prototype.mergeWith = function (otherList) {
            if (otherList != null)
                ko.utils.arrayPushAll(this.Errors, otherList.Errors());
        };
        return ErrorList;
    }());
    xomega.ErrorList = ErrorList;
})(xomega || (xomega = {}));
// Copyright (c) 2017 Xomega.Net. All rights reserved.
var xomega;
(function (xomega) {
    // An error message that consists of an error code, a text message and the severity.
    // Error messages are typically added to an error list and can be serialized
    // to allow sending them in a service call.
    var ErrorMessage = (function () {
        // Constructs an error message with a given code, message and severity.
        function ErrorMessage(code, message, severity) {
            this.Code = code;
            this.Message = message;
            this.Severity = severity;
        }
        // Deserializes an ErrorMessage object from JSON that contains a serialized Xomega Framework ErrorMessage.
        ErrorMessage.fromJSON = function (obj) {
            return new ErrorMessage(obj.Code, obj.Message, obj.Severity);
        };
        return ErrorMessage;
    }());
    xomega.ErrorMessage = ErrorMessage;
})(xomega || (xomega = {}));
// Copyright (c) 2017 Xomega.Net. All rights reserved.
var xomega;
(function (xomega) {
    // Error severity possible values.
    var ErrorSeverity;
    (function (ErrorSeverity) {
        // Information message that can be displayed to the user.
        ErrorSeverity[ErrorSeverity["Info"] = 0] = "Info";
        // A warning that may be displayed to the user for the confirmation before proceeding,
        // if supported by the current execution context.
        ErrorSeverity[ErrorSeverity["Warning"] = 1] = "Warning";
        // An error, that will be displayed to the user with the other errors. It doesn't stop
        // the execution flow, but prevents the operation from successfully completing.
        ErrorSeverity[ErrorSeverity["Error"] = 2] = "Error";
        // A critical error, which stops the execution immediately and returns a fault to the user.
        ErrorSeverity[ErrorSeverity["Critical"] = 3] = "Critical";
    })(ErrorSeverity = xomega.ErrorSeverity || (xomega.ErrorSeverity = {}));
})(xomega || (xomega = {}));
// Copyright (c) 2017 Xomega.Net. All rights reserved.
var xomega;
(function (xomega) {
    var AuthManager = (function () {
        function AuthManager() {
            // Observable flag indicating if the user is logged in
            this.LoggedIn = ko.observable(false);
        }
        Object.defineProperty(AuthManager, "Current", {
            // Static getter for the current authentication manager
            get: function () {
                if (!AuthManager._current)
                    AuthManager._current = new AuthManager();
                return AuthManager._current;
            },
            // Static setter for the current authentication manager
            set: function (value) { AuthManager._current = value; },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(AuthManager.prototype, "accessToken", {
            // Getter for the locally stored access token
            get: function () { return sessionStorage.getItem(AuthManager.AccessTokenKey); },
            // Setter for the locally stored access token
            set: function (value) {
                if (value)
                    sessionStorage.setItem(AuthManager.AccessTokenKey, value);
                else
                    sessionStorage.removeItem(AuthManager.AccessTokenKey);
            },
            enumerable: true,
            configurable: true
        });
        // Signs in the current user with the specified security token and claims
        AuthManager.prototype.signIn = function (token, claims) {
            this.accessToken = token;
            this.Claims = claims;
            this.LoggedIn(true);
        };
        // Signs the current user out
        AuthManager.prototype.signOut = function () {
            this.accessToken = null;
            this.Claims = null;
            this.LoggedIn(false);
        };
        // Constructs or enhances provided headers with a proper Authorization header.
        AuthManager.prototype.getHeaders = function (headers) {
            var res = headers || {};
            var accessToken = this.accessToken;
            if (accessToken) {
                res.Authorization = 'Bearer ' + accessToken;
            }
            return res;
        };
        AuthManager.prototype.createAjaxRequest = function () {
            var req = {};
            req.url = AuthManager.ApiRoot;
            req.contentType = 'application/json';
            req.headers = this.getHeaders(null);
            return req;
        };
        // Handles Unathorized (401) server response (e.g. due to an expired token) by signing the user out
        AuthManager.prototype.handleUnauthorizedResponse = function () {
            var aMgr = this;
            $(document).ajaxError(function (event, jqxhr, settings, thrownError) {
                if (jqxhr.status == 401)
                    aMgr.signOut();
            });
        };
        // Build loging URL for the current routing instruction
        AuthManager.prototype.getLoginUrl = function (instruction) {
            var returnUrl = instruction.fragment;
            if (instruction.queryString)
                returnUrl += '?' + instruction.queryString;
            return AuthManager.LoginPath + '?' + AuthManager.ReturnParam + '=' + encodeURIComponent(returnUrl);
        };
        // Perform a role based security check for a specified route configuration
        AuthManager.prototype.isRouteAllowed = function (routeCfg) {
            if (this.LoggedIn()) {
                if (Array.isArray(routeCfg.roles) && this.Claims.role)
                    return routeCfg.roles.indexOf(this.Claims.role) >= 0;
                return true;
            }
            return routeCfg.allowAnonymous;
        };
        // Secures routing by checking if the user is logged in and the route is allowed.
        AuthManager.guardRoute = function (instance, instruction) {
            var aMgr = AuthManager.Current;
            if (aMgr.LoggedIn())
                return aMgr.isRouteAllowed(instruction.config);
            else if (instruction.config.route == AuthManager.LoginPath)
                return true;
            else
                return aMgr.getLoginUrl(instruction);
        };
        // Utility function to process menu items (routes) recursively
        AuthManager.forEachItem = function (item, func, ctx) {
            if (!item)
                return;
            if (Array.isArray(item)) {
                item.forEach(function (i) { AuthManager.forEachItem(i, func, ctx); }, ctx);
            }
            else {
                // process children first, to handle parents' dependencies on children
                this.forEachItem(item.items, func, ctx);
                $.proxy(func, ctx)(item);
            }
        };
        // Sets up an allowed computed for the given menu item to control its visibility.
        // The parent item is not allowed if none of its children are allowed.
        AuthManager.prototype.setUpAllowed = function (item) {
            if (item.route != null) {
                item.allowed = ko.computed(function () { return this.isRouteAllowed(item); }, this);
            }
            else {
                item.allowed = ko.computed(function () {
                    return !Array.isArray(item.items) || item.items.some(function (i) { return i.allowed(); });
                }, this);
            }
        };
        return AuthManager;
    }());
    // The root URL for web API
    AuthManager.ApiRoot = '/';
    // The key to use for storing access token
    AuthManager.AccessTokenKey = 'access_token';
    // The path (route) to the login view
    AuthManager.LoginPath = 'login';
    // Query parameter for the return URL after the login
    AuthManager.ReturnParam = 'return';
    xomega.AuthManager = AuthManager;
})(xomega || (xomega = {}));
// Copyright (c) 2017 Xomega.Net. All rights reserved.
var xomega;
(function (xomega) {
    // Format string using specified positional parameters
    function format(str) {
        var params = [];
        for (var _i = 1; _i < arguments.length; _i++) {
            params[_i - 1] = arguments[_i];
        }
        var p = getParams(params);
        return str.replace(/\{\{|\}\}|\{(\w+)\}/g, function (m, n) {
            if (m == "{{") {
                return "{";
            }
            if (m == "}}") {
                return "}";
            }
            return p[n];
        });
    }
    xomega.format = format;
    // Extract an array of parameters from the given array to account for cases when parameters
    // are passed as an array to another method that expects them to be passed as individual values
    function getParams(params) {
        return (params && params.length == 1 && $.isArray(params[0])) ? getParams(params[0]) : params;
    }
    xomega.getParams = getParams;
    // simple utility function to convert a string to CamelCase
    function toCamelCase(str) {
        return str.replace(/(^| |\.|-|_|\/)(.)/g, function (match, g1, g2) {
            return g2.toUpperCase();
        });
    }
    xomega.toCamelCase = toCamelCase;
    // turns the given value into an observable using specified default value
    function makeObservable(val, def) {
        return ko.isObservable(val) ? val : ko.observable(val || def);
    }
    xomega.makeObservable = makeObservable;
})(xomega || (xomega = {}));
// Copyright (c) 2017 Xomega.Net. All rights reserved.
var xomega;
(function (xomega) {
    // A general-purpose class that represents the header information of any object,
    // that includes the most relevant fields to identify the object and any additional attributes
    // that can be used for filtering or to support various display options.
    // The Type string of a header determines the class of objects it represents.
    // It has also a string based internal ID and a Text field for display purposes.
    // It can also have any number of additional named attributes that can hold any value or a list of values.
    var Header = (function () {
        // Constructs a valid header of the given type with the specified ID and text.
        function Header(type, id, text) {
            // A flag indicating if the header was properly constructed with both ID and the text.
            // This is typically False if the header was not found in the corresponding lookup table
            // and therefore was merely constructed from the user input.
            this.isValid = true;
            // A flag indicating if the header is currently active.
            // Typically, only the active headers can be selected by the user,
            // but the code can still look up and display an inactive header.
            this.isActive = true;
            // Default format to use when converting the header to a string. By default, it displays the header ID.
            this.defaultFormat = Header.fieldId;
            // Arbibtrary additional attributes
            this.attr = {};
            this.type = type;
            this.id = '' + id;
            this.text = '' + text;
        }
        /// Compares this header with another header for equality by values.
        /// Two headers are considered equal if they have the same type and the same ID and validity.
        Header.prototype.equals = function (h) {
            if (!h)
                return false;
            return this.type == h.type && this.id == h.id && this.isValid == h.isValid;
        };
        // Deserializes a Header object from JSON that contains a serialized Xomega Framework Header.
        Header.fromJSON = function (obj) {
            var h = new Header(obj.Type, obj.Id, obj.Text);
            h.defaultFormat = obj.DefaultFormat;
            h.isActive = obj.IsActive;
            // DataContractSerializer returns an array of Key/Value pairs
            if ($.isArray(obj.attributes)) {
                for (var i = 0; i < obj.attributes.length; i++) {
                    h.attr[obj.attributes[i].Key] = obj.attributes[i].Value;
                }
            }
            else
                h.attr = obj.attributes;
            return h;
        };
        // Returns a string representation of the header based on the specified format.
        // The format string can use the field names in curly braces.
        Header.prototype.toString = function (fmt) {
            if (fmt === void 0) { fmt = this.defaultFormat; }
            // for performance purposes check standard fields first
            if (fmt === Header.fieldId || !this.isValid)
                return this.id;
            if (fmt === Header.fieldText)
                return this.text;
            var hdr = this;
            return fmt.replace(/\[\[|\]\]|\[(i|t|a:)(.*?)\]/g, function (m) {
                var n = [];
                for (var _i = 1; _i < arguments.length; _i++) {
                    n[_i - 1] = arguments[_i];
                }
                if (m === "[[")
                    return "[";
                if (m === "]]")
                    return "]";
                if (n[1] == null || n[1] == "") {
                    if (n[0] === "i")
                        return hdr.id;
                    if (n[0] === "t")
                        return hdr.text;
                }
                else if (n[0] === "a:") {
                    var res = "";
                    var attr = hdr.getAttribute(n[1]);
                    if ($.isArray(attr)) {
                        return attr.join(", ");
                    }
                    return attr;
                }
                return m;
            });
        };
        // Constructs a deep clone of the current header.
        Header.prototype.clone = function () {
            var h = new Header(this.type, this.id, this.text);
            return $.extend(true, h, this);
        };
        // Returns a value of the given named attribute.
        Header.prototype.getAttribute = function (attribute) {
            return this.attr[attribute];
        };
        // Sets the attribute value if it has never been set. Otherwise adds a value
        // to the list of values of the given attribute unless it already has such a value.
        // If the current attribute value is not a list, it creates a list and adds it to the list first.
        Header.prototype.addToAttribute = function (attribute, value) {
            var curVal = this.attr[attribute];
            if (curVal == null && value != null) {
                this.attr[attribute] = value;
                return;
            }
            if (value == null || value == curVal)
                return;
            var lst;
            if ($.isArray(curVal))
                lst = curVal;
            else {
                lst = new Array();
                if (curVal != null)
                    lst.push(curVal);
                this.attr[attribute] = lst;
            }
            if (lst.indexOf(value) < 0)
                lst.push(value);
        };
        return Header;
    }());
    // A constant that represents the ID field when used as part of the display format.
    Header.fieldId = "[i]";
    // A constant that represents the Text field when used as part of the display format.
    Header.fieldText = "[t]";
    // A constant that represents a named attribute when used as part of the display format.
    // The placeholder {0} should be replaced with the attribute name by calling
    // format(attrPattern, attrName);
    Header.attrPattern = "[a:{0}]";
    xomega.Header = Header;
})(xomega || (xomega = {}));
// Copyright (c) 2017 Xomega.Net. All rights reserved.
// Copyright (c) 2017 Xomega.Net. All rights reserved.
var xomega;
(function (xomega) {
    // A class that represents a cache of lookup tables by their types.
    var LookupCache = (function () {
        function LookupCache() {
            // A cache of lookup tables by type.
            this.cache = {};
            // A dictionary by lookup table type of listeners
            // waiting to be notified when the lookup table is loaded.
            this.notifyQueues = {};
        }
        // A subroutine for loading the lookup table if it's not loaded.
        LookupCache.prototype.loadLookupTable = function (type, onReadyCallback) {
            // Protection from queuing up listeners for a table type that is not supported,
            // which will never be notified thereby creating a memory leak.
            if (LookupCache.cacheLoaders.every(function (cl) { return !cl.isSupported(type); })) {
                delete this.notifyQueues[type];
                return;
            }
            var notify = this.notifyQueues[type];
            if (notify != null) {
                // The table is already being loaded, so just add the listener to the queue to be notified.
                if (onReadyCallback != null && notify.indexOf(onReadyCallback) < 0)
                    notify.push(onReadyCallback);
            }
            else {
                notify = new Array();
                if (onReadyCallback != null)
                    notify.push(onReadyCallback);
                this.notifyQueues[type] = notify;
                for (var i = LookupCache.cacheLoaders.length - 1; i >= 0; i--)
                    if (LookupCache.cacheLoaders[i].isSupported(type)) {
                        LookupCache.cacheLoaders[i].load(this, type);
                        return;
                    }
            }
        };
        // Gets a lookup table of the specified type from the cache.
        LookupCache.prototype.getLookupTable = function (type, onReadyCallback) {
            if (type == null)
                return null;
            var tbl = this.cache[type];
            if (tbl == null) {
                this.loadLookupTable(type, onReadyCallback);
                tbl = this.cache[type];
            }
            return tbl;
        };
        // Removes the lookup table of the specified type from the cache.
        // This method can be used to trigger reloading of the lookup table next time it is requested.
        LookupCache.prototype.removeLookupTable = function (type) {
            delete this.cache[type];
            delete this.notifyQueues[type];
        };
        // Stores the given lookup table in the current cache under the table's type.
        // The lookup table and its type should not be null.
        LookupCache.prototype.cacheLookupTable = function (table) {
            if (table == null || table.type == null)
                return;
            this.cache[table.type] = table;
            var notify = this.notifyQueues[table.type];
            if (notify != null) {
                for (var i = 0; i < notify.length; i++)
                    notify[i](table.type);
            }
            delete this.notifyQueues[table.type];
        };
        return LookupCache;
    }());
    // Static current instance of the lookup cache.
    LookupCache.current = new LookupCache();
    // Static list of registered lookup cache loaders.
    LookupCache.cacheLoaders = new Array();
    xomega.LookupCache = LookupCache;
})(xomega || (xomega = {}));
// Copyright (c) 2017 Xomega.Net. All rights reserved.
var xomega;
(function (xomega) {
    // A self-indexing lookup table. The data set for the table is based on a list of values.
    // The table allows looking up values based on any string represenation of the value as defined 
    // by the format string that you pass in.
    // If the data is not indexed by that format, the table will build and cache the index first.
    var LookupTable = (function () {
        // Constructs a new lookup table from the specified data set.
        function LookupTable(type, data, caseSensitive) {
            // Errors that occurred during loading of this lookup table
            this.errors = new xomega.ErrorList();
            // Indexed data by key format that is used to get the key.
            this.indexedData = {};
            this.type = type;
            this.data = data;
            this.caseSensitive = caseSensitive;
            for (var i = 0; i < data.length; i++)
                data[i].type = type;
        }
        // Constructs a lookup table of the given type from the specified errors
        LookupTable.fromErrors = function (type, errors) {
            var res = new LookupTable(type, [], false);
            res.errors = errors;
            return res;
        };
        // Deserializes a LookupTable object from JSON that contains a serialized Xomega Framework LookupTable.
        LookupTable.fromJSON = function (obj) {
            var data = obj.data.map(function (val, idx, arr) { return xomega.Header.fromJSON(val); });
            var tbl = new LookupTable(obj.Type, data, obj.caseSensitive);
            return tbl;
        };
        // Get a copy of the table values filtered by the supplied function.
        // Only values that match the filter will be cloned, which is better for performance.
        LookupTable.prototype.getValues = function (filterFunc, thisArg) {
            if (filterFunc === void 0) { filterFunc = null; }
            if (thisArg === void 0) { thisArg = null; }
            var lst = this.data;
            if (filterFunc != null)
                lst = lst.filter(filterFunc, thisArg);
            return lst.map(function (h) { return h.clone(); });
        };
        // Looks up a Header item by the id field.
        LookupTable.prototype.lookupById = function (id) {
            return this.lookupByFormat(xomega.Header.fieldId, id);
        };
        // Looks up an item in the table by a value of the item string representation
        // specified by the supplied format parameter. If the table is not indexed
        // by the given format, it builds such an index first.
        // If multiple items have the same value for the given format, then only the
        // first one will be returned and the rest of them will be stored in an attribute
        // with a name composed from the '_grp:' constant and the format string.
        LookupTable.prototype.lookupByFormat = function (fmt, value) {
            var tbl = this.indexedData[fmt];
            if (typeof tbl === "undefined")
                tbl = this.buildIndexedTable(fmt);
            var res = tbl[this.caseSensitive ? value : value.toUpperCase()];
            if (res != null)
                return res; // res.clone(); is safer, but worse performing
            return null;
        };
        // Clears all indexes in the table.
        // The indexes will be rebuilt as needed at the first subsequent attempt to look up a value by any format.
        LookupTable.prototype.resetIndexes = function () {
            this.indexedData = {};
        };
        // Clears an index for the given format. The index will be rebuilt at the next attempt
        // to look up a value by this format.
        LookupTable.prototype.clearIndex = function (fmt) {
            delete this.indexedData[fmt];
        };
        // Builds an index for the specified format.
        LookupTable.prototype.buildIndexedTable = function (format) {
            var tbl = {};
            for (var i = 0; i < this.data.length; i++) {
                var h = this.data[i];
                if (h == null)
                    continue;
                var key = h.toString(format);
                if (!this.caseSensitive)
                    key = key.toLocaleUpperCase();
                var h1 = tbl[key];
                if (typeof h1 !== "undefined")
                    h1.addToAttribute("_grp:" + format, h);
                else
                    tbl[key] = h;
            }
            this.indexedData[format] = tbl;
            return tbl;
        };
        return LookupTable;
    }());
    xomega.LookupTable = LookupTable;
})(xomega || (xomega = {}));
// Copyright (c) 2017 Xomega.Net. All rights reserved.
var xomega;
(function (xomega) {
    // Implementation of a cache loader that loads the data from the Xomega Framework based
    // lookup cache that is exposed as a RESTful web service.
    var XomegaCacheLoader = (function () {
        function XomegaCacheLoader() {
        }
        // Implements isSupported to return true for any table type.
        XomegaCacheLoader.prototype.isSupported = function (tableType) {
            return true;
        };
        // Loads the lookup table data
        XomegaCacheLoader.prototype.load = function (cache, tableType) {
            var req = xomega.AuthManager.Current.createAjaxRequest();
            req.url += xomega.format(XomegaCacheLoader.uriTemplate, tableType);
            req.success = function (data, textStatus, jqXHR) {
                var tbl;
                if (data == null) {
                    // do nothing if the table is already loaded by another loader
                    if (cache.getLookupTable(tableType))
                        return;
                    var err = new xomega.ErrorList();
                    err.addError(xomega.format("Lookup table '{0}' is not found.", tableType));
                    tbl = xomega.LookupTable.fromErrors(tableType, err);
                }
                else
                    tbl = xomega.LookupTable.fromJSON(data);
                cache.cacheLookupTable(tbl);
            };
            req.error = function (jqXHR, textStatus, errorThrow) {
                // do nothing if the table is already loaded by another loader
                if (cache.getLookupTable(tableType))
                    return;
                var errLst = new xomega.ErrorList();
                errLst.Errors.push(new xomega.ErrorMessage(errorThrow, jqXHR.responseText, xomega.ErrorSeverity.Error));
                console.error(jqXHR.responseText);
                cache.cacheLookupTable(xomega.LookupTable.fromErrors(tableType, errLst));
            };
            $.ajax(req);
        };
        return XomegaCacheLoader;
    }());
    XomegaCacheLoader.uriTemplate = "lookup-table/{0}";
    xomega.XomegaCacheLoader = XomegaCacheLoader;
    xomega.LookupCache.cacheLoaders.push(new XomegaCacheLoader());
})(xomega || (xomega = {}));
// Copyright (c) 2017 Xomega.Net. All rights reserved.
/// <reference path="ILookupCacheLoader.ts"/>
/// <reference path="LookupCache.ts"/>
/// <reference path="LookupTable.ts"/>
var xomega;
(function (xomega) {
    // A base class for the lookup cache loader implementations.
    // It is designed to support cache loaders that either explicitly specify the table types
    // they can load or load all their lookup tables at once during the first time they run,
    // which will determine their supported table types.
    var BaseLookupCacheLoader = (function () {
        // Initializes base parameters of the lookup cache loader.
        function BaseLookupCacheLoader(caseSensitive) {
            var tableTypes = [];
            for (var _i = 1; _i < arguments.length; _i++) {
                tableTypes[_i - 1] = arguments[_i];
            }
            this.caseSensitive = caseSensitive;
            if (tableTypes != null && tableTypes.length > 0) {
                this.supportedTypes = tableTypes;
            }
        }
        // Determines if the given cache type and table type are supported by the current cache loader.
        BaseLookupCacheLoader.prototype.isSupported = function (tableType) {
            return this.supportedTypes == null || this.supportedTypes.indexOf(tableType) >= 0;
        };
        // Loads a lookup table for the specified type into the given lookup cache.
        // Implementation of the corresponding interface method.
        BaseLookupCacheLoader.prototype.load = function (cache, tableType) {
            var _this = this;
            if (!this.isSupported(tableType))
                return;
            this.loadCache(tableType, function (table) {
                // do nothing if the table is already loaded by another loader
                if (cache.getLookupTable(tableType))
                    return;
                cache.cacheLookupTable(table);
                // ensure supportedTypes gets populated
                if (_this.supportedTypes == null)
                    _this.supportedTypes = new Array();
                if (_this.supportedTypes.indexOf(table.type) < 0)
                    _this.supportedTypes.push(table.type);
            });
        };
        // Subroutine implemented by subclasses to perform the actual loading
        // of the lookup table and storing it in the cache using the provided updateCache delegate.
        // The loading process can be either synchronous or asynchronous.
        BaseLookupCacheLoader.prototype.loadCache = function (tableType, updateCache) {
        };
        return BaseLookupCacheLoader;
    }());
    xomega.BaseLookupCacheLoader = BaseLookupCacheLoader;
})(xomega || (xomega = {}));
// Copyright (c) 2017 Xomega.Net. All rights reserved.
var xomega;
(function (xomega) {
    // The base class for all Xomega properties that defines various additional meta-information
    // that can be associated with a piece of data, such as description, editability, visibility,
    // security, whether or not it is required, etc. It also provides support for notification
    // of any changes in this type of information.
    // Xomega properties are typically added to Xomega data objects that can serve as a data model
    // for user interface screens.
    var BaseProperty = (function () {
        // Constructs a base property
        function BaseProperty() {
            var _this = this;
            // An internal flag to allow manually making the property uneditable.
            // The default value is true.
            this.editable = ko.observable(true);
            // A internal flag to allow manually making the property invisible.
            // The default value is true.
            this.visible = ko.observable(true);
            // A internal flag that keeps track of whether or not the property is required.
            // The default value is false.
            this.required = ko.observable(false);
            // The parent data object of the property if any. In rare cases the parent can be set to null
            // and therefore should be always checked for null.
            this.Parent = ko.observable();
            // Returns the current access level for the property.
            // Allows setting a new access level and fires a property change event
            // for property editability and visibility, since they both depend on the security access level.
            this.AccessLevel = ko.observable(xomega.AccessLevel.Full);
            // initialize computed observables in the constructor after the parent is set
            this.Editable = ko.computed({
                read: function () {
                    var al = _this.AccessLevel();
                    return _this.editable()
                        && (_this.Parent() == null || _this.Parent().isPropertyEditable(_this))
                        && (al > xomega.AccessLevel.ReadOnly);
                },
                write: function (value) { _this.editable(value); },
                owner: this
            });
            this.Visible = ko.computed({
                read: function () {
                    var al = _this.AccessLevel();
                    return _this.visible()
                        && (_this.Parent() == null || _this.Parent().isPropertyVisible(_this))
                        && (al > xomega.AccessLevel.None);
                },
                write: function (value) { _this.visible(value); },
                owner: this
            });
            this.Required = ko.computed({
                read: function () {
                    return _this.required() && _this.Editable() && _this.Visible()
                        && (_this.Parent() == null || _this.Parent().isPropertyVisible(_this));
                },
                write: function (value) { _this.required(value); },
                owner: this
            });
        }
        // Performs additional property initialization after all other properties and child objects
        // have been already added to the parent object and would be accessible from within this method.
        BaseProperty.prototype.onInitialized = function () {
            // the subclasses can implement the additional initialization
        };
        // implementation of the IInitializable
        BaseProperty.prototype.setName = function (name) {
            this.Name = name;
        };
        // Returns a user-friendly string representation of the property.
        BaseProperty.prototype.toString = function () {
            if (this.Label != null)
                return this.Label;
            // convert Pascal case to words
            var res = this.Name.replace(/([a-z])([A-Z])/, "$1 $2");
            res = res.replace(/([A-Z][A-Z])([A-Z])([a-z])/, "$1 $2$3");
            return res;
        };
        return BaseProperty;
    }());
    xomega.BaseProperty = BaseProperty;
})(xomega || (xomega = {}));
// Copyright (c) 2017 Xomega.Net. All rights reserved.
// Copyright (c) 2017 Xomega.Net. All rights reserved.
// Copyright (c) 2017 Xomega.Net. All rights reserved.
// Copyright (c) 2017 Xomega.Net. All rights reserved.
// Copyright (c) 2017 Xomega.Net. All rights reserved.
// Copyright (c) 2017 Xomega.Net. All rights reserved.
var xomega;
(function (xomega) {
    // Enumeration for different security access levels, which can be associated with properties,
    // data objects or other elements that require security.
    // The access level enumeration constants are listed in the ascending order, so that they can be compared
    // using the standard 'greater than', 'less than' and 'equals' operators.
    var AccessLevel;
    (function (AccessLevel) {
        // The constant indicating no access to the given element.
        // The user can neither view nor modify the element.
        AccessLevel[AccessLevel["None"] = 0] = "None";
        // The constant indicating view/read only access to the given element.
        // The user can view the element, but not modify it.
        AccessLevel[AccessLevel["ReadOnly"] = 1] = "ReadOnly";
        // The constant indicating full access to the given element.
        // The user can both view and modify the element.
        AccessLevel[AccessLevel["Full"] = 2] = "Full";
    })(AccessLevel = xomega.AccessLevel || (xomega.AccessLevel = {}));
})(xomega || (xomega = {}));
// Copyright (c) 2017 Xomega.Net. All rights reserved.
var xomega;
(function (xomega) {
    // Enumeration that represents different formats that data property values can be converted to.
    var ValueFormat;
    (function (ValueFormat) {
        // The format in which values are stored internally in data properties.
        // The format is typically typed, that is an integer would be stored as an <c>int</c>.
        // Whenever a value is set on a data property, it will always try to convert it 
        // to the internal format first. If it fails to convert it, it may store it as is.
        // For multivalued data properties, each value in the list will be converted to an internal format.
        ValueFormat[ValueFormat["Internal"] = 0] = "Internal";
        // The format in which data property values are transported between layers
        // during a service call. The format is typically typed and may or may not be
        // the same as the internal format. For example, we may want to store a resolved
        // <c>Header</c> object internally, but send only the ID part in a service call.
        ValueFormat[ValueFormat["Transport"] = 1] = "Transport";
        // The string format in which the user inputs the value. It may or may not be the same
        // as the format in which the value is displayed to the user when it's not editable.
        ValueFormat[ValueFormat["EditString"] = 2] = "EditString";
        // The string format in which the value is displayed to the user when it's not editable.
        // When internal value is an object such as <c>Header</c>, the display string may
        // consist of a combination of several of its parts.
        ValueFormat[ValueFormat["DisplayString"] = 3] = "DisplayString";
    })(ValueFormat = xomega.ValueFormat || (xomega.ValueFormat = {}));
})(xomega || (xomega = {}));
// Copyright (c) 2017 Xomega.Net. All rights reserved.
var xomega;
(function (xomega) {
    // A base class for properties that contain a piece of data.
    // The data could be a single value or a list of values based on the property's <c>IsMultiValued</c> flag.
    // While the member to store the value is untyped, the actual values stored in the property
    // are always converted to the internal format whenever possible, which would be typed.
    // Data property also provides support for value conversion, validation and modification tracking.
    // It can also provide a list of possible values (items) where applicable.
    var DataProperty = (function (_super) {
        __extends(DataProperty, _super);
        // Constructs a data property
        function DataProperty() {
            var _this = _super.call(this) || this;
            // Gets or sets the modification state of the property. Null means the property value has never been set.
            // False means the value has been set only once (initialized).
            // True means that the value has been modified since it was initialized.
            _this.Modified = ko.observable();
            // Gets or sets whether the property contains multiple values (a list) or a single value.
            _this.IsMultiValued = false;
            // Gets or sets the string to display when the property value is null.
            // Setting such string as a value will be considered as setting the value to null.
            // The default is empty string.
            _this.NullString = "";
            // Gets or sets the string to display when the property value is restricted and not allowed to be viewed (e.g. N/A).
            // The default is empty string.
            _this.RestrictedString = "";
            // Gets or sets the separators to use for multivalued properties to parse the list of values from the input string.
            // The default is comma, semicolon and a new line.
            _this.ParseListSeparators = /;|,|\r\n/;
            // Gets or sets the separator to use for multivalued properties to combine the list of values into a display string.
            // The default is comma with a space.
            _this.DisplayListSeparator = ", ";
            // The list of validation errors for the property.
            _this.ValidationErrors = new xomega.ErrorList();
            // Validation status of the property
            _this.Validated = false;
            // A list of property validators. Default is the required validator
            _this.Validators = new Array(DataProperty.validateRequired);
            // Observable list of possible values for the data property
            _this.PossibleValues = ko.observableArray();
            // An array of items that the property is waiting for before it is ready to handle data
            _this.waitingFor = [];
            _this.value = ko.observable();
            _this.value.equalityComparer = function (a, b) {
                if (a && !b || !a && b)
                    return false;
                if (!a && !b && typeof a === typeof b)
                    return true; // account for undefined/null/false difference
                if (a && $.isFunction(a.equals))
                    return a.equals(b);
                if (b && $.isFunction(b.equals))
                    return b.equals(a);
                return a === b;
            };
            _this.InternalValue = ko.computed({
                read: function () { return _this.value(); },
                write: function (value) {
                    var newVal = _this.resolveValue(value, xomega.ValueFormat.Internal);
                    _this.value(newVal);
                },
                owner: _this
            });
            _this.DisplayStringValue = ko.computed({
                read: function () {
                    return _this.resolveValue(_this.InternalValue(), xomega.ValueFormat.DisplayString);
                },
                write: function (value) { _this.InternalValue(value); },
                owner: _this
            });
            _this.EditStringValue = ko.computed({
                read: function () {
                    return _this.resolveValue(_this.InternalValue(), xomega.ValueFormat.EditString);
                },
                write: function (value) { _this.InternalValue(value); },
                owner: _this
            });
            _this.TransportValue = ko.computed({
                read: function () {
                    return _this.resolveValue(_this.InternalValue(), xomega.ValueFormat.Transport);
                },
                write: function (value) { _this.InternalValue(value); },
                owner: _this
            });
            // subscribe at the end, since validation uses InternalValue, which needs to be updated first
            _this.value.subscribe(function (newVal) {
                // set modified to false the first time the value is populated,
                // and to true when it is subsequently changed
                if (_this.Modified() == null)
                    _this.Modified(false);
                else {
                    _this.Modified(true);
                    _this.validate(true); // don't validate unmodified value
                }
            }, _this);
            return _this;
        }
        // Checks if the current format is one of the string formats.
        DataProperty.isStringFormat = function (format) {
            return format === xomega.ValueFormat.EditString || format === xomega.ValueFormat.DisplayString;
        };
        // Checks if the current format is one of the typed formats.
        DataProperty.isTypedFormat = function (format) {
            return format === xomega.ValueFormat.Internal || format === xomega.ValueFormat.Transport;
        };
        // Performs additional property initialization after all other properties and child objects
        // have been already added to the parent object and would be accessible from within this method.
        DataProperty.prototype.onInitialized = function () {
            // if init changed property settings that affect any calculated values (e.g. NullString) between 
            // construction and now, then we need to notify all dependents to update their cached latest value.
            this.InternalValue.notifySubscribers(this.value());
            this.updateValueList();
        };
        // reset the data property to the default value
        DataProperty.prototype.reset = function () {
            this.InternalValue(null);
            this.ValidationErrors.Errors.removeAll();
        };
        // A function to determine if the given value is considered to be null for the given format.
        // Default implementation returns true if the value is null, is an empty list,
        // is a string with blank spaces only or is equal to the NullString for any format.
        // Subclasses can override this function to differentiate by the value format
        // or to provide different or additional rules.
        DataProperty.prototype.isValueNull = function (value, format) {
            if (value === null || typeof value === 'undefined')
                return true;
            if ($.isArray(value)) {
                return value.length === 0;
            }
            var str = value.toString().trim();
            return str == "" || str === this.NullString;
        };
        // Checks if the current property value is null.
        DataProperty.prototype.isNull = function () { return this.isValueNull(this.value(), xomega.ValueFormat.Internal); };
        // Sets new value for the property.
        DataProperty.prototype.setValue = function (value, format) {
            var newVal = this.resolveValue(value, xomega.ValueFormat.Internal, format);
            this.value(newVal);
        };
        // Resolves the given value or a list of values to the specified format based on the current property configuration.
        // If the property is restricted or the value is null and the format is string based,
        // the <c>RestrictedString</c> or <c>NullString</c> are returned respectively.
        // If the property is multivalued it will try to convert the value to a list or parse it into a list if it's a string
        // or just add it to a new list as is and then convert each value in the list into the given format.
        // Otherwise it will try to convert the single value to the given format.
        // If a custom value converter is set on the property, it will be used first before the default property conversion rules are applied.
        DataProperty.prototype.resolveValue = function (value, outFormat, inFormat) {
            var _this = this;
            if (this.AccessLevel() === xomega.AccessLevel.None)
                return outFormat == xomega.ValueFormat.DisplayString ? this.RestrictedString : value;
            if (this.isValueNull(value, outFormat))
                return outFormat == xomega.ValueFormat.DisplayString ? this.NullString : null;
            if (this.IsMultiValued) {
                var lst;
                if ($.isArray(value))
                    lst = value;
                else if (typeof value === "string") {
                    lst = value.split(this.ParseListSeparators);
                    lst = lst.map(function (str) { return str.trim(); });
                    lst = lst.filter(function (str) { return str !== ""; });
                }
                else
                    lst = [value];
                lst = lst.map(function (val) { return _this.convertValue(val, outFormat); }, this);
                return this.convertList(lst, outFormat);
            }
            else {
                return this.convertValue(value, outFormat, inFormat);
            }
        };
        // Converts a single value to a given format. The default implementation does nothing to the value,
        // but subclasses can implement the property specific rules for each format.
        DataProperty.prototype.convertValue = function (value, outFormat, inFormat) {
            return value;
        };
        // Converts a list of values to the given format.
        // Default implementation returns the list as is for the typed formats and 
        // uses the DisplayListSeparator to concatenate the values for any string format.
        // Subclasses can override this behavior to differentiate between the <c>DisplayString</c> format
        // and the <c>EditString</c> format and can also provide custom delimiting, e.g. comma-separated
        // and a new line between every five values to get five comma-separated values per line.
        DataProperty.prototype.convertList = function (list, format) {
            if (DataProperty.isTypedFormat(format))
                return list;
            return list.join(this.DisplayListSeparator);
        };
        // Returns if the current property value has been validated and is valid, i.e. has no validation errors.
        DataProperty.prototype.isValid = function (validate) {
            if (validate === void 0) { validate = true; }
            if (validate)
                this.validate();
            return this.ValidationErrors && !this.ValidationErrors.hasErrors();
        };
        // Validate the property. If force flag is true, then always validate,
        // otherwise (by default) validate only if needed.
        DataProperty.prototype.validate = function (force) {
            var _this = this;
            if (force === void 0) { force = false; }
            if (force)
                this.Validated = false;
            if (this.Validated)
                return;
            this.ValidationErrors.Errors().length = 0; // clear w/o notification to avoid recursion
            var value = this.InternalValue();
            if ($.isArray(value)) {
                var lst = value;
                lst.forEach(function (val) {
                    _this.Validators.forEach(function (validator) { validator(_this, val); }, _this);
                }, this);
            }
            else
                this.Validators.forEach(function (validator) { validator(_this, value); }, this);
            this.Validated = true;
            this.ValidationErrors.Errors.valueHasMutated(); // notify now
        };
        // A standard validation function that checks for null if the value is required.
        DataProperty.validateRequired = function (dp, value) {
            if (dp != null && dp.Required() && dp.isValueNull(value, xomega.ValueFormat.Internal))
                dp.ValidationErrors.addError("{0} is required.", dp);
        };
        // A function to get a list of possible values for the property
        DataProperty.prototype.getPossibleValues = function () {
            return null;
        };
        // update the list of possible values
        DataProperty.prototype.updateValueList = function () {
            this.PossibleValues(this.getPossibleValues());
        };
        // returns if the property is ready
        DataProperty.prototype.isReady = function () {
            return this.waitingFor.length == 0;
        };
        // registers an item to wait for before the property is ready
        DataProperty.prototype.addWaitItem = function (item) {
            if (this.waitingFor.indexOf(item) < 0)
                this.waitingFor.push(item);
        };
        // remove the item the property waits for and notify the parent object if the property is ready
        DataProperty.prototype.removeWaitItem = function (item) {
            var idx = this.waitingFor.indexOf(item);
            if (idx >= 0)
                this.waitingFor.splice(idx, 1);
            if (this.isReady() && this.Parent())
                this.Parent().checkIfReady();
        };
        return DataProperty;
    }(xomega.BaseProperty));
    xomega.DataProperty = DataProperty;
})(xomega || (xomega = {}));
// Copyright (c) 2017 Xomega.Net. All rights reserved.
var xomega;
(function (xomega) {
    // The base class for all data objects, which contain a list of data properties
    // and possibly a number of child objects or object lists.
    var DataObject = (function () {
        function DataObject() {
            var _this = this;
            // Gets or sets the parent object for the current data object.
            this.Parent = ko.observable();
            // Returns the current access level for the data object.
            // Allows setting a new access level for editability and visibility of all properties,
            // since both of it depend on the security access level.
            this.AccessLevel = ko.observable(xomega.AccessLevel.Full);
            this.editable = ko.observable(true);
            this.modified = ko.observable();
            /** A flag indicating if the object is tracking modifications */
            this.TrackModifications = true;
            // The list of validation errors for the data object.
            this.ValidationErrors = new xomega.ErrorList();
            // Validation status of the data object
            this.Validated = false;
            // An indicator if the object is new and not yet saved
            this.IsNew = ko.observable(true);
            // a list of callbacks to invoke when the object is ready
            this.readyCallbacks = [];
            // initialize Editable before properties, since it is used in turn by the properties
            this.Editable = ko.computed({
                read: function () {
                    var al = _this.AccessLevel();
                    return _this.editable()
                        && (_this.Parent() == null || _this.Parent().Editable())
                        && (al > xomega.AccessLevel.ReadOnly);
                },
                write: function (value) { _this.editable(value); },
                owner: this
            });
            // initialize the properties
            this.init();
            // set up name/parent and any additional initialization on all properties and child objects
            this.onInitialized();
            // initialize Modified after the properties and child objects, since it uses them
            this.Modified = ko.computed({
                read: function () {
                    if (!_this.TrackModifications)
                        return false;
                    var res = _this.modified();
                    for (var prop in _this) {
                        var p = _this[prop];
                        if (_this.hasOwnProperty(prop) && p && p.Modified) {
                            if (p.Modified() != null)
                                res = res || p.Modified();
                        }
                    }
                    return res;
                },
                write: function (value) {
                    _this.modified(value);
                    if (value === false || value == null) {
                        for (var prop in _this) {
                            var p = _this[prop];
                            if (_this.hasOwnProperty(prop) && p && p.Modified)
                                p.Modified(value);
                        }
                    }
                },
                owner: this
            });
            // reset the Modified flag in case it changed during initialization
            this.Modified(null);
        }
        // The abstract method to be implemented by the subclasses
        // to add and initialize data object properties and child objects.
        DataObject.prototype.init = function () { };
        // Additional initialization that happens after all the properties
        // and child objects have been added and are therefore accessible.
        DataObject.prototype.onInitialized = function () {
            for (var prop in this) {
                var p = this[prop];
                if (this.hasOwnProperty(prop) && p && p.onInitialized) {
                    var init = p;
                    init.setName(prop);
                    init.Parent(this);
                    init.onInitialized();
                }
            }
        };
        // implemntation of the IInitializable
        DataObject.prototype.setName = function (name) {
            this.NameInParent = name;
        };
        // resets the data object to default values
        DataObject.prototype.reset = function () {
            for (var prop in this) {
                var dp = this[prop];
                if (this.hasOwnProperty(prop) && dp && dp.reset)
                    dp.reset();
            }
            this.ValidationErrors.Errors.removeAll();
        };
        // gets current object's data property by name
        DataObject.prototype.getDataProperty = function (name) {
            var names = [name + 'Property', name];
            for (var _i = 0, names_1 = names; _i < names_1.length; _i++) {
                var nm = names_1[_i];
                var dp = this[nm];
                if (dp instanceof xomega.DataProperty)
                    return dp;
            }
            return null;
        };
        // gets current object's child object by name
        DataObject.prototype.getChildObject = function (name) {
            var names = [name + 'Object', name + 'List', name];
            for (var _i = 0, names_2 = names; _i < names_2.length; _i++) {
                var nm = names_2[_i];
                var dobj = this[nm];
                if (dobj instanceof DataObject)
                    return dobj;
            }
            return null;
        };
        // initializes data object's data from the specified JSON object
        DataObject.prototype.fromJSON = function (obj, options) {
            for (var prop in obj) {
                if (!obj.hasOwnProperty(prop))
                    continue;
                var dp = this.getDataProperty(prop);
                if (dp) {
                    dp.setValue(obj[prop], xomega.ValueFormat.Transport);
                    dp.Modified(false);
                }
                else {
                    var dobj = this.getChildObject(prop);
                    if (dobj)
                        dobj.fromJSON(obj[prop]);
                }
            }
        };
        // typed function to convert data object values to the specified structure
        DataObject.prototype.toStruct = function (c, options) {
            var struct = new c();
            return this.toJSON(struct, options);
        };
        // convert data object's data to a JSON object using the provided contract if any
        DataObject.prototype.toJSON = function (contract, options) {
            var res = {};
            var ignoreEmpty = !options || options.ignoreEmpty; // true by default
            for (var prop in this) {
                if (!this.hasOwnProperty(prop))
                    continue;
                var p = this[prop];
                if (p instanceof xomega.DataProperty && (!contract || contract.hasOwnProperty(prop))) {
                    res[p.Name] = p.TransportValue();
                    // ignore empty values to minimize JSON or the URL string when using $.param() on it
                    if (ignoreEmpty && !res[p.Name])
                        delete res[p.Name];
                }
                else if (p instanceof DataObject) {
                    var child = prop.replace(/(Object|List)$/, '');
                    if (!contract || contract.hasOwnProperty(child))
                        res[child] = p.toJSON();
                }
            }
            return res;
        };
        Object.defineProperty(DataObject.prototype, "Properties", {
            get: function () {
                var res = new Array();
                for (var prop in this) {
                    if (this.hasOwnProperty(prop) && this[prop] instanceof xomega.BaseProperty) {
                        var bp = this[prop];
                        res.push(bp);
                    }
                }
                return res;
            },
            enumerable: true,
            configurable: true
        });
        DataObject.prototype.isPropertyEditable = function (prop) {
            return this.Editable() && (this.Parent() == null || this.Parent().isPropertyEditable(prop));
        };
        DataObject.prototype.isPropertyVisible = function (prop) {
            return this.Parent() == null || this.Parent().isPropertyVisible(prop);
        };
        DataObject.prototype.isPropertyRequired = function (prop) {
            return this.Parent() == null || this.Parent().isPropertyRequired(prop);
        };
        // Validate the data object. If force flag is true, then always validate,
        // otherwise (by default) validate only if needed.
        DataObject.prototype.validate = function (force) {
            if (force === void 0) { force = false; }
            if (force)
                this.Validated = false;
            if (this.Validated)
                return;
            this.ValidationErrors.Errors.removeAll();
            for (var prop in this) {
                var p = this[prop];
                if (this.hasOwnProperty(prop) && p && p.validate) {
                    var pv = p;
                    pv.validate(force);
                    this.ValidationErrors.mergeWith(pv.ValidationErrors);
                }
            }
            this.validateSelf();
            this.Validated = true;
        };
        // Validate the object itself, e.g. cross-field validations. etc.
        DataObject.prototype.validateSelf = function () {
            // to be overridden in subclasses
        };
        // Reads object data asynchronously
        DataObject.prototype.readAsync = function (options) {
            var obj = this;
            return this.doReadAsync(options).then(function () {
                obj.IsNew(false);
                return true;
            });
        };
        // Actual implementation of reading object data provided by subclasses
        DataObject.prototype.doReadAsync = function (options) { return $.when(); };
        // Saves object data asynchronously
        DataObject.prototype.saveAsync = function (options) {
            var obj = this;
            obj.validate(true);
            if (obj.ValidationErrors.hasErrors())
                return $.Deferred().reject(obj.ValidationErrors);
            return this.doSaveAsync(options).then(function () {
                obj.IsNew(false);
                obj.Modified(false);
            });
        };
        // Actual implementation of saving object data provided by subclasses
        DataObject.prototype.doSaveAsync = function (options) { return $.when(); };
        // Deletes object asynchronously
        DataObject.prototype.deleteAsync = function (options) { return this.doDeleteAsync(options); };
        // Actual implementation of deleting the object provided by subclasses
        DataObject.prototype.doDeleteAsync = function (options) { return $.when(); };
        // register an callback to be invoked when the object is ready
        DataObject.prototype.onReady = function (callback) {
            if (this.isReady())
                callback();
            else if (this.readyCallbacks.indexOf(callback) < 0)
                this.readyCallbacks.push(callback);
        };
        // returns if the object, including all properties and child objects, is ready
        DataObject.prototype.isReady = function () {
            for (var prop in this) {
                var p = this[prop];
                if (this.hasOwnProperty(prop) && p && p.isReady) {
                    if (!p.isReady())
                        return false;
                }
            }
            return true;
        };
        // checks if the object is ready and, if so, invokes the regiestred onReady callbacks
        DataObject.prototype.checkIfReady = function () {
            if (this.Parent() != null)
                this.Parent().checkIfReady();
            else if (this.isReady()) {
                this.readyCallbacks.forEach(function (cb) { return cb(); });
                this.readyCallbacks.length = 0;
            }
        };
        return DataObject;
    }());
    xomega.DataObject = DataObject;
})(xomega || (xomega = {}));
// Copyright (c) 2017 Xomega.Net. All rights reserved.
var xomega;
(function (xomega) {
    var ListSortDirection;
    (function (ListSortDirection) {
        ListSortDirection[ListSortDirection["Ascending"] = 0] = "Ascending";
        ListSortDirection[ListSortDirection["Descending"] = 1] = "Descending";
    })(ListSortDirection = xomega.ListSortDirection || (xomega.ListSortDirection = {}));
    // A class that represents an individual sort field with a property name and a sort direction.
    var ListSortField = (function () {
        // constructs a new list sort field for the given property
        function ListSortField(property) {
            // The sort direction: ascending or descending.
            this.SortDirection = ListSortDirection.Ascending;
            // Whether nulls are placed first
            this.NullsFirst = false;
            this.PropertyName = property;
        }
        // Toggles sort direction for the current sort field
        ListSortField.prototype.toggleDirection = function () {
            this.SortDirection = (this.SortDirection == ListSortDirection.Ascending) ?
                ListSortDirection.Descending : ListSortDirection.Ascending;
        };
        return ListSortField;
    }());
    xomega.ListSortField = ListSortField;
})(xomega || (xomega = {}));
// Copyright (c) 2017 Xomega.Net. All rights reserved.
var xomega;
(function (xomega) {
    // A class that holds data for each row of the data list object
    var DataRow = (function () {
        // constructs a new data row object
        function DataRow(list) {
            this._selected = ko.observable(false);
            this.List = list;
        }
        // Handle user click to toggle selection of the current row if the list supports selection
        DataRow.prototype.toggleSelection = function () {
            if (this.List.RowSelectionMode())
                this.List.toggleSelection(this);
        };
        // initializes data row's data from the specified JSON object
        DataRow.prototype.fromJSON = function (obj) {
            for (var prop in obj) {
                var dp = this.List[prop];
                if (obj.hasOwnProperty(prop) && dp instanceof xomega.DataProperty) {
                    this[prop] = dp.resolveValue(obj[prop], xomega.ValueFormat.Internal, xomega.ValueFormat.Transport);
                }
            }
        };
        // convert data row to a JSON object
        DataRow.prototype.toJSON = function (contract) {
            var res = {};
            for (var prop in this.List) {
                var dp = this.List[prop];
                if (this.hasOwnProperty(prop) && dp instanceof xomega.DataProperty && (!contract || contract.hasOwnProperty(prop))) {
                    res[prop] = dp.resolveValue(this[prop], xomega.ValueFormat.Transport);
                }
            }
            return res;
        };
        // Compares this row with the other row provided using specified sort criteria.
        DataRow.prototype.compareTo = function (other, criteria) {
            if (criteria === void 0) { criteria = this.List ? this.List.SortCriteria() : null; }
            if (!criteria || this.List !== other.List)
                return 0;
            else if (!other)
                return 1;
            var res = 0;
            for (var i = 0; i < criteria.length; i++) {
                var p = this.List[criteria[i].PropertyName];
                var nulls = criteria[i].NullsFirst ? -1 : 1;
                if (p != null) {
                    var val1 = this[criteria[i].PropertyName];
                    var val2 = other[criteria[i].PropertyName];
                    if (val1 == val2)
                        res = 0;
                    else if (val1 == null && val2 != null)
                        res = -1 * nulls;
                    else if (val1 != null && val2 == null)
                        res = 1 * nulls;
                    else if (typeof val1 == 'number' && typeof val2 == 'number')
                        res = val1 - val2;
                    else if (val1 instanceof Date && val2 instanceof Date)
                        res = val1.getTime() - val2.getTime();
                    else if (val1.localeCompare)
                        res = val1.localeCompare(val2); // string
                    else if (val2.localeCompare)
                        res = -val2.localeCompare(val1); // string
                    else {
                        var str1 = p.resolveValue(val1, xomega.ValueFormat.DisplayString);
                        var str2 = p.resolveValue(val2, xomega.ValueFormat.DisplayString);
                        res = str1.localeCompare(str2);
                    }
                    if (criteria[i].SortDirection == xomega.ListSortDirection.Descending)
                        res *= -1;
                }
                if (res != 0)
                    return res;
            }
            return res;
        };
        return DataRow;
    }());
    xomega.DataRow = DataRow;
})(xomega || (xomega = {}));
// Copyright (c) 2017 Xomega.Net. All rights reserved.
var xomega;
(function (xomega) {
    // A dynamic data object that has a list of rows as its data instead of specific values.
    var DataListObject = (function (_super) {
        __extends(DataListObject, _super);
        // constructs a new data list object
        function DataListObject() {
            var _this = _super.call(this) || this;
            // the list of data objects for the current data object list
            _this.List = ko.observableArray();
            _this.SortCriteria = ko.observableArray();
            // criteria object
            _this.CriteriaObject = null;
            // applied criteria
            _this.AppliedCriteria = ko.observableArray();
            _this.AppliedCriteriaText = ko.pureComputed(function () {
                var text = '';
                var crit = this.AppliedCriteria();
                if (!crit)
                    return text;
                for (var i = 0; i < crit.length; i++) {
                    var fc = crit[i];
                    if (text)
                        text += '; ';
                    text += fc.Label + ':' + (fc.Operator ? ' ' + fc.Operator : '') + (fc.Data.length > 0 ? ' ' + fc.Data.join(' and ') : '');
                }
                return text;
            }, _this);
            /** Current selection mode for data list rows. Null means user selection is not supported */
            _this.RowSelectionMode = ko.observable();
            _this.reset();
            _this.SortCriteria.subscribe(function (newVal) { return _this.sort(); }, _this);
            return _this;
        }
        // resets the list and criteria as needed
        DataListObject.prototype.reset = function (full) {
            if (full === void 0) { full = true; }
            this.List.removeAll();
            this.AppliedCriteria(null);
            this.Modified(null);
            if (this.CriteriaObject && full)
                this.CriteriaObject.reset();
        };
        // override validate to not call it on properties
        DataListObject.prototype.validate = function (force) {
            if (force)
                this.Validated = false;
            if (this.Validated)
                return;
            this.ValidationErrors.Errors.removeAll();
            if (this.CriteriaObject) {
                this.CriteriaObject.validate(force);
                this.ValidationErrors.mergeWith(this.CriteriaObject.ValidationErrors);
            }
            this.validateSelf();
            this.Validated = true;
        };
        // initializes data object list's data from the specified JSON object
        DataListObject.prototype.fromJSON = function (obj, options) {
            var _this = this;
            if (!$.isArray(obj))
                return;
            var preserveSelection = options && options.preserveSelection; // false by default
            var sel = preserveSelection ? this.getSelectedRows() : [];
            var keys = this.Properties.filter(function (p) { return p.IsKey; }).map(function (p) { return new xomega.ListSortField(p.Name); });
            var objects = new Array();
            for (var i = 0; i < obj.length; i++) {
                var dr = new xomega.DataRow(this);
                dr.fromJSON(obj[i]);
                objects.push(dr);
                if (preserveSelection)
                    dr._selected(sel.some(function (r) { return _this.sameEntity(r, dr, keys); }, this));
            }
            this.List(objects);
            this.sort();
            this.AppliedCriteria(this.CriteriaObject ? this.CriteriaObject.getFieldsCriteria() : []);
            this.Modified(false);
        };
        // convert data object's data to a JSON object
        DataListObject.prototype.toJSON = function (contract) {
            var res = [];
            var data = this.List();
            var itemContract;
            if (contract instanceof Array && contract.length > 0)
                itemContract = contract[0];
            for (var i = 0; i < data.length; i++) {
                res.push(data[i].toJSON(itemContract));
            }
            return res;
        };
        /// Checks if two data rows represent the same entity. Can be overridden in subclasses.
        DataListObject.prototype.sameEntity = function (r1, r2, keys) {
            return !r1 || !keys || keys.length == 0 ? false : r1.compareTo(r2, keys) == 0;
        };
        DataListObject.prototype.sort = function () {
            if (this.SortCriteria && this.SortCriteria().length > 0)
                this.List.sort(function (left, right) { return left ? left.compareTo(right) : -1; });
        };
        /** Toggles selection of the given row according to the current row selection mode */
        DataListObject.prototype.toggleSelection = function (row) {
            var select = !row._selected();
            // deselect other rows if not multiple selection
            if (select && this.RowSelectionMode() !== DataListObject.SelectionModeMultiple)
                this.List().filter(function (dr) { return dr._selected() && dr !== row; }).forEach(function (dr) { return dr._selected(false); });
            row._selected(select);
        };
        DataListObject.prototype.getSelectedRows = function () {
            return this.List().filter(function (r) { return r._selected(); });
        };
        DataListObject.prototype.clearSelectedRows = function () {
            this.getSelectedRows().forEach(function (r) { return r._selected(false); });
        };
        return DataListObject;
    }(xomega.DataObject));
    // Data list supports single selection
    DataListObject.SelectionModeSingle = 'single';
    // Data list supports multiple selection
    DataListObject.SelectionModeMultiple = 'multiple';
    xomega.DataListObject = DataListObject;
})(xomega || (xomega = {}));
// Copyright (c) 2017 Xomega.Net. All rights reserved.
var xomega;
(function (xomega) {
    // The base class for all data objects, which contain a list of data properties
    // and possibly a number of child objects or object lists.
    var DataObjectList = (function (_super) {
        __extends(DataObjectList, _super);
        // constructs a new data object list with a function for creating individual data objects
        function DataObjectList(objectCreator) {
            var _this = _super.call(this) || this;
            // the list of data objects for the current data object list
            _this.List = ko.observableArray();
            _this.listModified = ko.observable();
            _this.objectCreator = objectCreator;
            _this.Template = objectCreator();
            // delegate parent management to the template
            _this.Parent = ko.computed({
                read: function () { return _this.Template.Parent(); },
                write: function (value) { _this.Template.Parent(value); },
                owner: _this
            });
            // delegate editable management to the template
            _this.Editable = ko.computed({
                read: function () { return _this.Template.Editable(); },
                write: function (value) { _this.Template.Editable(value); },
                owner: _this
            });
            // override the Modified to check the list of objects rather than properties
            _this.Modified = ko.computed({
                read: function () {
                    var obj, res = _this.listModified();
                    for (var i = 0; i < _this.List().length; i++) {
                        obj = _this.List()[i];
                        if (obj.Modified() != null)
                            res = res || obj.Modified();
                    }
                    return res;
                },
                write: function (value) { _this.listModified(value); },
                owner: _this
            });
            _this.List.subscribe(function (changes) {
                var modified = false;
                var obj;
                for (var i = 0; i < changes.length; i++) {
                    var change = changes[i];
                    obj = change.value;
                    if (change.status == "added") {
                        obj.Parent(_this);
                        // if new items are added that have not been read, mark the list as modified
                        if (obj.Modified() == null)
                            modified = true;
                    }
                    if (change.status == "deleted") {
                        obj.Parent(null);
                        // if items were removed, mark the list as modified
                        modified = true;
                    }
                }
                if (modified)
                    _this.Modified(true); // update observable once here
            }, _this, "arrayChange");
            return _this;
        }
        // Override onInitialized to prevent updating the template
        DataObjectList.prototype.onInitialized = function () {
            // do nothing, as base function would set this as a parent for the template
        };
        // resets the list
        DataObjectList.prototype.reset = function () {
            this.List.removeAll();
        };
        // initializes data object list's data from the specified JSON object
        DataObjectList.prototype.fromJSON = function (obj) {
            if (!$.isArray(obj))
                return;
            this.List.removeAll();
            var objects = new Array();
            for (var i = 0; i < obj.length; i++) {
                var dobj = this.objectCreator();
                dobj.fromJSON(obj[i]);
                objects.push(dobj);
            }
            this.List(objects);
        };
        // Delegates determining property editability to the template object.
        DataObjectList.prototype.isPropertyEditable = function (prop) {
            var p = this.Template[prop.Name];
            return p == null || p.Editable();
        };
        // Delegates determining property visibility to the template object.
        DataObjectList.prototype.isPropertyVisible = function (prop) {
            var p = this.Template[prop.Name];
            return p == null || p.Visible();
        };
        // Delegates determining if property is required to the template object.
        DataObjectList.prototype.isPropertyRequired = function (prop) {
            var p = this.Template[prop.Name];
            return p == null || p.Required();
        };
        // Validates the data object list and all its contained objects recursively.
        DataObjectList.prototype.validate = function (force) {
            if (force === void 0) { force = false; }
            if (force)
                this.Validated = false;
            if (this.Validated)
                return;
            this.ValidationErrors.Errors.removeAll();
            var obj;
            for (var i = 0; i < this.List().length; i++) {
                obj = this.List()[i];
                obj.validate(force);
                this.ValidationErrors.mergeWith(obj.ValidationErrors);
            }
            this.validateSelf();
            this.Validated = true;
        };
        return DataObjectList;
    }(xomega.DataObject));
    xomega.DataObjectList = DataObjectList;
})(xomega || (xomega = {}));
// Copyright (c) 2017 Xomega.Net. All rights reserved.
/// <reference path="DataObject.ts"/>
var xomega;
(function (xomega) {
    var CriteriaObject = (function (_super) {
        __extends(CriteriaObject, _super);
        function CriteriaObject() {
            return _super !== null && _super.apply(this, arguments) || this;
        }
        /// Determines if any criteria are populated
        CriteriaObject.prototype.hasCriteria = function () {
            return this.Properties.filter(function (p) { return p instanceof xomega.DataProperty && !(p instanceof xomega.OperatorProperty); })
                .some(function (p) { return !p.isNull(); });
        };
        // Sets values from the given object and adjusts values for operators
        CriteriaObject.prototype.fromJSON = function (obj, options) {
            _super.prototype.fromJSON.call(this, obj, options);
            // clear operators, for which associated properties are blank
            for (var prop in this) {
                var p = this[prop];
                if (!(p instanceof xomega.OperatorProperty))
                    continue;
                var op = p;
                var isBlank = true;
                for (var _i = 0, _a = [op.AdditionalPropertyName, op.AdditionalPropertyName2]; _i < _a.length; _i++) {
                    var nm = _a[_i];
                    var dp = this.getDataProperty(nm);
                    if (dp && !dp.isNull())
                        isBlank = false;
                }
                if (isBlank)
                    op.InternalValue(null);
            }
        };
        // gets an array of field criteria
        CriteriaObject.prototype.getFieldsCriteria = function () {
            // make a map of object's properties
            var map = {};
            for (var prop in this) {
                var p = this[prop];
                if (this.hasOwnProperty(prop) && p instanceof xomega.BaseProperty)
                    map[p.Name] = p;
            }
            // process operators if any
            for (var prop in map) {
                var p = map[prop];
                if (!(p instanceof xomega.OperatorProperty))
                    continue;
                var op = p;
                // clear mapping for bound properties
                if (op.AdditionalPropertyName)
                    map[op.AdditionalPropertyName] = null;
                if (op.AdditionalPropertyName2)
                    map[op.AdditionalPropertyName2] = null;
            }
            // make array of settings
            var res = new Array();
            for (var prop in map) {
                var p = map[prop];
                if (p instanceof xomega.OperatorProperty) {
                    var op = p;
                    if (op.isNull())
                        continue;
                    var data = new Array();
                    var dp1 = op.AdditionalPropertyName ? this[op.AdditionalPropertyName] : null;
                    if (dp1 && !dp1.isNull() && dp1.Visible())
                        data.push(dp1.DisplayStringValue());
                    var dp2 = op.AdditionalPropertyName2 ? this[op.AdditionalPropertyName2] : null;
                    if (dp2 && !dp2.isNull() && dp2.Visible())
                        data.push(dp2.DisplayStringValue());
                    res.push(new FieldCriteria(op.toString(), op.DisplayStringValue(), data));
                }
                else if (p instanceof xomega.DataProperty) {
                    var dp = p;
                    if (dp.isNull())
                        continue;
                    res.push(new FieldCriteria(dp.toString(), null, [dp.DisplayStringValue()]));
                }
            }
            return res;
        };
        return CriteriaObject;
    }(xomega.DataObject));
    xomega.CriteriaObject = CriteriaObject;
    // Field criteria structure
    var FieldCriteria = (function () {
        function FieldCriteria(label, op, data) {
            this.Label = label;
            this.Operator = op;
            this.Data = data;
        }
        return FieldCriteria;
    }());
    xomega.FieldCriteria = FieldCriteria;
})(xomega || (xomega = {}));
// Copyright (c) 2017 Xomega.Net. All rights reserved.
var xomega;
(function (xomega) {
    // property bindigs registry
    var Bindings = (function () {
        function Bindings() {
        }
        // initialize bindings by setting up knockout property binding handler
        Bindings.init = function () {
            // set up knockout binding that looks up property binding and delegates the work to it
            // only do the init function, which will listen to property changes if/when needed
            // to improve performance, since update is ALWAYS run in a dependentObservable that has its overhead
            ko.bindingHandlers['property'] = {
                init: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
                    var binding = Bindings.findBinding(element, valueAccessor());
                    if (binding != null)
                        binding.init(element, valueAccessor, allBindingsAccessor, viewModel, bindingContext);
                },
            };
            ko.bindingHandlers['id'] = {
                init: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
                    element.id = valueAccessor();
                }
            };
            // set up binding for sorting grid columns
            ko.bindingHandlers['sortby'] = {
                init: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
                    var settings = valueAccessor();
                    var list = (settings.list ? settings.list : bindingContext.$data);
                    var field = new xomega.ListSortField(null);
                    if (settings instanceof xomega.BaseProperty)
                        field.PropertyName = settings.Name;
                    else if (typeof settings.property === 'string')
                        field.PropertyName = settings.property;
                    else if (settings.property) {
                        field.PropertyName = settings.property.name;
                        if (settings.property.direction)
                            field.SortDirection = settings.property.direction;
                        if (settings.property.nullsFirst)
                            field.NullsFirst = settings.property.nullsFirst;
                    }
                    ;
                    if (!field.PropertyName)
                        console.warn('Invalid sortby binding: property name must be specified');
                    else if (!(list instanceof xomega.DataListObject))
                        console.warn("Invalid sortby binding for property " + settings.property + ": list property or current context should be a DataListObject");
                    else {
                        // add click event
                        var onClick_1 = function (ctx, event) {
                            var sc = ko.utils.arrayFirst(list.SortCriteria(), function (c) { return c.PropertyName == field.PropertyName; });
                            if (sc && event.ctrlKey)
                                list.SortCriteria.remove(sc);
                            else if (event.ctrlKey)
                                list.SortCriteria.push(field);
                            else if (sc) {
                                sc.toggleDirection();
                                list.SortCriteria.notifySubscribers();
                            }
                            else
                                list.SortCriteria([field]);
                        };
                        ko.bindingHandlers.click.init(element, function () { return onClick_1; }, allBindingsAccessor, viewModel, bindingContext);
                        // make clickable and add a sort glyph
                        var el_1 = $(element);
                        el_1.addClass('sortable');
                        el_1.append("<i style='display: none' class='sort-glyph fa' aria-hidden='true'/>");
                        // add glyph renderer
                        ko.computed(function () {
                            var glyph = el_1.children('i');
                            var crit = list.SortCriteria();
                            var sc = ko.utils.arrayFirst(crit, function (c) { return c.PropertyName == field.PropertyName; });
                            var idx = sc ? crit.indexOf(sc) : -1;
                            for (var i = 0; i < 3; i++) {
                                glyph.toggleClass('sort-' + (i + 1), i == idx);
                            }
                            glyph.attr('style', 'display:' + (sc ? 'inline' : 'none'));
                            if (sc) {
                                glyph.toggleClass('fa-long-arrow-up', sc.SortDirection == xomega.ListSortDirection.Ascending);
                                glyph.toggleClass('fa-long-arrow-down', sc.SortDirection == xomega.ListSortDirection.Descending);
                            }
                        });
                    }
                }
            };
        };
        // register a property binding
        Bindings.register = function (binding) {
            Bindings.registered.push(binding);
        };
        // find a property bidning that applies to the given element
        Bindings.findBinding = function (element, property) {
            for (var i = Bindings.registered.length - 1; i >= 0; i--)
                if (Bindings.registered[i].appliesTo(element, property))
                    return Bindings.registered[i];
            return null;
        };
        return Bindings;
    }());
    // a list of registered property bindings to look through
    Bindings.registered = new Array();
    xomega.Bindings = Bindings;
})(xomega || (xomega = {}));
// Copyright (c) 2017 Xomega.Net. All rights reserved.
var xomega;
(function (xomega) {
    // base property binding that all bindings inherit from
    var PropertyBinding = (function () {
        function PropertyBinding() {
        }
        // this is an abstract base class and should not be used directly on elements
        PropertyBinding.prototype.appliesTo = function (element, property) {
            return false;
        };
        // instead of using the standard KO approach of handling all property updates in the update method
        // we explicitly try to subscribe to individual events and handle each one with a separate function,
        // so that property updates wouldn't trigger unrelevant, but expensive operations (e.g. rebuilding selection lists).
        PropertyBinding.prototype.init = function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
            if (viewModel instanceof xomega.DataRow) {
                // to improve performance we don't subscribe to property changes and don't handle required and validation errors 
                // since the grid should not be directly editable except for selection checkboxes
                this.handleEditable(element, valueAccessor, allBindingsAccessor, viewModel, bindingContext);
                this.handleVisible(element, valueAccessor, allBindingsAccessor, viewModel, bindingContext);
                this.handleValue(element, valueAccessor, allBindingsAccessor, viewModel, bindingContext);
            }
            else {
                ko.computed(function () {
                    this.handleEditable(element, valueAccessor, allBindingsAccessor, viewModel, bindingContext);
                }, this, { disposeWhenNodeIsRemoved: element });
                ko.computed(function () {
                    this.handleVisible(element, valueAccessor, allBindingsAccessor, viewModel, bindingContext);
                }, this, { disposeWhenNodeIsRemoved: element });
                ko.computed(function () {
                    this.handleValidationErrors(element, valueAccessor, allBindingsAccessor, viewModel, bindingContext);
                }, this, { disposeWhenNodeIsRemoved: element });
                ko.computed(function () {
                    this.handleRequired(element, valueAccessor, allBindingsAccessor, viewModel, bindingContext);
                }, this, { disposeWhenNodeIsRemoved: element });
                ko.computed(function () {
                    this.handleValue(element, valueAccessor, allBindingsAccessor, viewModel, bindingContext);
                }, this, { disposeWhenNodeIsRemoved: element });
                this.setLabel(element, valueAccessor);
            }
        };
        // we don't have to do anything here if we subscribe to all updates in the init instead,
        // so this method is here just as a hook to allow subclasses to implement it if needed.
        PropertyBinding.prototype.update = function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
        };
        // handle changes in validation errors and udpate the error text and style accordingly
        PropertyBinding.prototype.handleValidationErrors = function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
            this.setErrorText(element, valueAccessor().ValidationErrors.ErrorsText());
            ko.bindingHandlers.css.update(element, function () {
                return {
                    invalid: !valueAccessor().isValid(false)
                };
            }, allBindingsAccessor, viewModel, bindingContext);
        };
        // function to set error text for the element that could be overridden in subclasses
        PropertyBinding.prototype.setErrorText = function (element, errorText) {
            element.title = errorText;
        };
        // handle changes in Editable and udpate the control's state accordingly
        PropertyBinding.prototype.handleEditable = function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
            ko.bindingHandlers.enable.update(element, function () { return valueAccessor().Editable; }, allBindingsAccessor, viewModel, bindingContext);
        };
        // handle changes in Visible and udpate the control's state accordingly
        PropertyBinding.prototype.handleVisible = function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
            ko.bindingHandlers.visible.update(element, function () { return valueAccessor().Visible; }, allBindingsAccessor, viewModel, bindingContext);
            var label = this.getLabel(element);
            if (label)
                ko.bindingHandlers.visible.update(label, function () { return valueAccessor().Visible; }, allBindingsAccessor, viewModel, bindingContext);
        };
        // handle changes in Required and udpate the label accordingly
        PropertyBinding.prototype.handleRequired = function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
            var label = this.getLabel(element);
            if (label)
                ko.bindingHandlers.css.update(label, function () {
                    return {
                        required: valueAccessor().Required()
                    };
                }, allBindingsAccessor, viewModel, bindingContext);
        };
        // handle changes in Value in subclasses
        PropertyBinding.prototype.handleValue = function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
        };
        // utility function to get the label for the element
        PropertyBinding.prototype.getLabel = function (element) {
            var qry = 'label[for="' + element.id + '"]';
            var label = $(qry);
            if (label.length <= 0)
                label = $(element).closest("label");
            else if (label.length > 1)
                label = $(element).closest(':has(> ' + qry + ')').find(qry);
            return label.length > 0 ? label.get(0) : null;
        };
        PropertyBinding.prototype.setLabel = function (element, valueAccessor) {
            var label = this.getLabel(element);
            if (label && label.innerText && valueAccessor().Label == null) {
                var text = label.innerText.replace('_', '').trim();
                if (text[text.length - 1] === ':')
                    text = text.substring(0, text.length - 1);
                valueAccessor().Label = text;
            }
        };
        return PropertyBinding;
    }());
    xomega.PropertyBinding = PropertyBinding;
})(xomega || (xomega = {}));
// Copyright (c) 2017 Xomega.Net. All rights reserved.
var xomega;
(function (xomega) {
    // property binding for readonly output text only
    var OutputTextBinding = (function (_super) {
        __extends(OutputTextBinding, _super);
        function OutputTextBinding() {
            return _super !== null && _super.apply(this, arguments) || this;
        }
        OutputTextBinding.prototype.appliesTo = function (element, property) {
            return true;
        };
        OutputTextBinding.prototype.handleValue = function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
            // if the viewModel is a DataRow, i.e. we are binding a list, then bind directly to the value
            // instead of the observable object to improve performance.
            // As a result, data rows should be replaced in the list instead of updating their values in code
            ko.bindingHandlers.text.update(element, function () { return viewModel instanceof xomega.DataRow ?
                valueAccessor().resolveValue(viewModel[valueAccessor().Name], xomega.ValueFormat.DisplayString) : valueAccessor().DisplayStringValue; }, allBindingsAccessor, viewModel, bindingContext);
        };
        return OutputTextBinding;
    }(xomega.PropertyBinding));
    xomega.OutputTextBinding = OutputTextBinding;
    xomega.Bindings.register(new OutputTextBinding());
})(xomega || (xomega = {}));
// Copyright (c) 2017 Xomega.Net. All rights reserved.
/// <reference path="PropertyBinding.ts"/>
var xomega;
(function (xomega) {
    // property binding for input text controls
    var InputTextBinding = (function (_super) {
        __extends(InputTextBinding, _super);
        function InputTextBinding() {
            return _super !== null && _super.apply(this, arguments) || this;
        }
        InputTextBinding.prototype.appliesTo = function (element, property) {
            return element.tagName.toLowerCase() == "textarea" ||
                element.tagName.toLowerCase() == "input" && InputTextBinding.inputTypes.indexOf(element.type) >= 0;
        };
        InputTextBinding.prototype.init = function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
            _super.prototype.init.call(this, element, valueAccessor, allBindingsAccessor, viewModel, bindingContext);
            ko.bindingHandlers.value.init(element, function () {
                return valueAccessor().Editable() ?
                    valueAccessor().EditStringValue : valueAccessor().DisplayStringValue;
            }, allBindingsAccessor, viewModel, bindingContext);
            if (controls) {
                var el = $(element);
                if (typeof (controls.datePicker) === 'function' && valueAccessor() instanceof xomega.DateTimeProperty
                    && (el.hasClass('date') || el.hasClass('datetime'))) {
                    var dtp = valueAccessor();
                    controls.datePicker(el, dtp.EditFormat);
                }
                if (typeof (controls.autoComplete) === 'function' && valueAccessor() instanceof xomega.EnumProperty) {
                    controls.autoComplete(el, valueAccessor());
                }
            }
        };
        InputTextBinding.prototype.handleValue = function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
            ko.bindingHandlers.value.update(element, function () {
                return valueAccessor().Editable() ?
                    valueAccessor().EditStringValue : valueAccessor().DisplayStringValue;
            }, allBindingsAccessor, viewModel, bindingContext);
        };
        return InputTextBinding;
    }(xomega.PropertyBinding));
    InputTextBinding.inputTypes = ['text', 'password', 'email', 'tel', 'url'];
    xomega.InputTextBinding = InputTextBinding;
    xomega.Bindings.register(new InputTextBinding());
})(xomega || (xomega = {}));
// Copyright (c) 2017 Xomega.Net. All rights reserved.
/// <reference path="PropertyBinding.ts"/>
var xomega;
(function (xomega) {
    // property binding for a single checkbox
    var CheckboxBinding = (function (_super) {
        __extends(CheckboxBinding, _super);
        function CheckboxBinding() {
            return _super !== null && _super.apply(this, arguments) || this;
        }
        CheckboxBinding.prototype.appliesTo = function (element, property) {
            return element.tagName.toLowerCase() == "input" && element.type == "checkbox"
                && !property.IsMultiValued;
        };
        CheckboxBinding.prototype.init = function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
            _super.prototype.init.call(this, element, valueAccessor, allBindingsAccessor, viewModel, bindingContext);
            $(element).click(function () {
                if (valueAccessor().isNull())
                    valueAccessor().InternalValue(false);
                else if (!valueAccessor().InternalValue())
                    valueAccessor().InternalValue(true);
                else
                    valueAccessor().InternalValue(valueAccessor().Required() ? false : null);
            });
        };
        CheckboxBinding.prototype.handleValue = function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
            $(element).prop("indeterminate", valueAccessor().isNull());
            element.checked = valueAccessor().InternalValue() ? true : false;
        };
        return CheckboxBinding;
    }(xomega.PropertyBinding));
    xomega.CheckboxBinding = CheckboxBinding;
    xomega.Bindings.register(new CheckboxBinding());
})(xomega || (xomega = {}));
// Copyright (c) 2017 Xomega.Net. All rights reserved.
var xomega;
(function (xomega) {
    // property binding for a single or multiple select control
    var SelectBinding = (function (_super) {
        __extends(SelectBinding, _super);
        function SelectBinding() {
            return _super !== null && _super.apply(this, arguments) || this;
        }
        SelectBinding.prototype.appliesTo = function (element, property) {
            return element.tagName.toLowerCase() == "select";
        };
        SelectBinding.prototype.init = function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
            _super.prototype.init.call(this, element, valueAccessor, allBindingsAccessor, viewModel, bindingContext);
            if (valueAccessor().IsMultiValued)
                element.multiple = true;
            $(element).change(function () {
                valueAccessor().InternalValue($(this).val());
            });
        };
        SelectBinding.prototype.handleValue = function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
            var tmpl = '<option value="{0}">{1}</option>';
            // use transport value for equality comparison, since internal value can be an object (e.g. Header)
            var value = valueAccessor().TransportValue();
            $(element).empty();
            if (!valueAccessor().IsMultiValued && (valueAccessor().isNull() || !valueAccessor().Required())) {
                var opt = xomega.format(tmpl, "", valueAccessor().Required() ? SelectBinding.DefaultSelectOption :
                    valueAccessor().NullString);
                $(opt).appendTo(element).prop("selected", valueAccessor().isNull());
            }
            var vals = valueAccessor().PossibleValues();
            var tvals = [];
            if (vals != null) {
                ko.utils.arrayForEach(vals, function (item) {
                    var val = valueAccessor().convertValue(item, xomega.ValueFormat.Transport);
                    tvals.push(val);
                    var opt = xomega.format(tmpl, val, valueAccessor().convertValue(item, xomega.ValueFormat.DisplayString));
                    var selected = valueAccessor().IsMultiValued ? value && ko.utils.arrayIndexOf(value, val) >= 0 : val == value;
                    $(opt).appendTo(element).prop("selected", selected);
                });
            }
            if (!valueAccessor().IsMultiValued && !valueAccessor().isNull() && ko.utils.arrayIndexOf(tvals, value) < 0) {
                var opt = xomega.format(tmpl, value, valueAccessor().convertValue(valueAccessor().InternalValue(), xomega.ValueFormat.DisplayString));
                $(opt).appendTo(element).prop("selected", true).attr('disabled', 'disabled');
            }
        };
        return SelectBinding;
    }(xomega.PropertyBinding));
    SelectBinding.DefaultSelectOption = "Select...";
    xomega.SelectBinding = SelectBinding;
    xomega.Bindings.register(new SelectBinding());
})(xomega || (xomega = {}));
// Copyright (c) 2017 Xomega.Net. All rights reserved.
var xomega;
(function (xomega) {
    // property binding for a list of checkboxes or radio buttons
    var OptionsBinding = (function (_super) {
        __extends(OptionsBinding, _super);
        function OptionsBinding() {
            return _super !== null && _super.apply(this, arguments) || this;
        }
        OptionsBinding.prototype.appliesTo = function (element, property) {
            return element.dataset.control == "options";
        };
        OptionsBinding.prototype.handleValue = function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
            var tmpl = '<div><label><input type="{0}" name="{1}" value="{2}"/>{3}</label></div>';
            var type = valueAccessor().IsMultiValued ? "checkbox" : "radio";
            var name = element.dataset.name;
            // use transport value for equality comparison, since internal value can be an object (e.g. Header)
            var value = valueAccessor().TransportValue();
            $(element).empty();
            if (!valueAccessor().IsMultiValued && !valueAccessor().Required()) {
                var opt = xomega.format(tmpl, type, name, "", valueAccessor().NullString);
                $(opt).appendTo(element).find("input").click(updateModel).prop("checked", valueAccessor().isNull());
            }
            var vals = valueAccessor().PossibleValues();
            if (vals != null) {
                ko.utils.arrayForEach(valueAccessor().PossibleValues(), function (item) {
                    var val = valueAccessor().convertValue(item, xomega.ValueFormat.Transport);
                    var opt = xomega.format(tmpl, type, name, val, valueAccessor().convertValue(item, xomega.ValueFormat.DisplayString));
                    var checked = valueAccessor().IsMultiValued ? value && ko.utils.arrayIndexOf(value, val) >= 0 : val == value;
                    $(opt).appendTo(element).find("input").click(updateModel).prop("checked", checked);
                });
            }
            function updateModel(evt) {
                if (valueAccessor().IsMultiValued) {
                    var arr = valueAccessor().TransportValue() || [];
                    var koUtils = ko.utils; // since ko.utils.addOrRemoveItem is not 'definitely typed' yet
                    koUtils.addOrRemoveItem(arr, evt.target.value, evt.target.checked);
                    valueAccessor().InternalValue(arr);
                }
                else
                    valueAccessor().InternalValue(evt.target.value);
            }
        };
        return OptionsBinding;
    }(xomega.PropertyBinding));
    xomega.OptionsBinding = OptionsBinding;
    xomega.Bindings.register(new OptionsBinding());
})(xomega || (xomega = {}));
// Copyright (c) 2017 Xomega.Net. All rights reserved.
var xomega;
(function (xomega) {
    // A data property that has enumerated set of possible values
    // that come from a lookup table of the specified type.
    // Internally the values are stored as objects of type Header,
    // which can store ID, text and a number of additional attributes for the value.
    // When a value is being set to the property it tries to resolve it to a Header
    // by looking it up in the lookup table for the property, which is obtained
    // from a lookup cache of a given type.
    var EnumProperty = (function (_super) {
        __extends(EnumProperty, _super);
        // Constructs a new EnumProperty.
        function EnumProperty() {
            var _this = _super.call(this) || this;
            // The string format that is used to obtain the key field from the Header.
            // The default value points to the header ID (see Header.fieldId),
            // but it can be customized to point to another unique field or a combination of fields
            // in the header, e.g. a custom attribute that stores a unique abbreviation.
            _this.KeyFormat = xomega.Header.fieldId;
            // The string format for a header field or combination of fields that is used
            // to display the header as a string. The default value is to display the header text
            // (see Header.fieldText).
            _this.DisplayFormat = xomega.Header.fieldText;
            // A dictionary that maps additional attributes that each possible value of this property may have
            // to other properties that could be used to implement cascading restrictions of the possible values
            // based on the current values of other properties.
            _this.cascadingProperties = new Object();
            _this.compare = function (h1, h2) {
                var s1 = _this.convertValue(h1, xomega.ValueFormat.DisplayString);
                var s2 = _this.convertValue(h2, xomega.ValueFormat.DisplayString);
                if (s1 != null && s1.localeCompare)
                    return s1.localeCompare(s2);
                else
                    return s1 < s2 ? -1 : s1 > s2 ? 1 : 0;
            };
            _this.updateValue = function (type) {
                // prevent from changing the modification state
                var mod = _this.Modified();
                _this.Modified(null); // prevent forced validation
                var h = _this.InternalValue();
                if (h instanceof xomega.Header && !h.isValid)
                    _this.InternalValue(h.id);
                _this.Modified(mod);
            };
            _this.updateList = function (type) {
                _this.updateValueList();
                _this.removeWaitItem(_this.updateList);
            };
            return _this;
        }
        // A function to filter allowed items. By default only active items are allowed.
        EnumProperty.prototype.filter = function (h) {
            return h != null && h.isActive && this.matchesCascadingProperties(h);
        };
        // Gets the lookup table for the property. The default implementation uses the <see cref="EnumType"/>
        // to find the lookup table in the lookup cache specified by the <see cref="CacheType"/>.
        // <returns>The lookup table to be used for the property.</returns>
        EnumProperty.prototype.getLookupTable = function (onReadyCallback) {
            if (this.localLookupTable != null)
                return this.localLookupTable;
            return xomega.LookupCache.current.getLookupTable(this.EnumType, onReadyCallback);
        };
        // Sets local lookup table for the property, blanks out the current value if it's not in the table
        // and notifies listeners about updated value list
        EnumProperty.prototype.setLookupTable = function (table) {
            this.localLookupTable = table;
            if (table && !table.lookupById(this.TransportValue()))
                this.InternalValue(null);
            else if (this.updateValue)
                this.updateValue(null);
            if (this.updateValueList)
                this.updateValueList();
        };
        // Converts a single value to a given format. For internal format
        // this method tries to convert the value to a header by looking it up
        // in the lookup table. For the transport format it uses the header ID.
        // For DisplayString and EditString formats it displays the header formatted according
        // to the specified DisplayFormat or KeyFormat respectively.
        EnumProperty.prototype.convertValue = function (value, format) {
            var h = value;
            if (format == xomega.ValueFormat.Internal) {
                if (value instanceof xomega.Header && h.type == this.EnumType)
                    return value;
                var str = ("" + value).trim();
                var tbl = this.getLookupTable(this.updateValue);
                if (tbl != null) {
                    h = null;
                    if (this.KeyFormat != xomega.Header.fieldId)
                        h = tbl.lookupByFormat(this.KeyFormat, str);
                    if (h == null)
                        h = tbl.lookupById(str);
                    if (h != null) {
                        h.defaultFormat = this.KeyFormat;
                        return h;
                    }
                }
                h = new xomega.Header(this.EnumType, str, null);
                h.isValid = false;
                return h;
            }
            else if (value instanceof xomega.Header) {
                if (format == xomega.ValueFormat.Transport)
                    return h.id;
                if (format == xomega.ValueFormat.EditString)
                    return h.toString(this.KeyFormat);
                if (format == xomega.ValueFormat.DisplayString)
                    return h.toString(this.DisplayFormat);
            }
            return _super.prototype.convertValue.call(this, value, format);
        };
        // A function that is used by default as the possible items provider
        // for the property by getting all possible values from the lookup table
        // filtered by the specified filter function and ordered by
        // the specified compare function.
        EnumProperty.prototype.getPossibleValues = function () {
            var res;
            var tbl = this.getLookupTable(this.updateList);
            if (tbl != null) {
                res = tbl.getValues(this.filter, this);
                if (this.compare)
                    res = res.sort(this.compare);
            }
            else {
                this.addWaitItem(this.updateList);
            }
            return res;
        };
        // Makes the list of possible values dependent on the current value(s) of another property,
        // which would be used to filter the list of possible values by the specified attribute.
        EnumProperty.prototype.setCascadingProperty = function (attribute, prop) {
            var _this = this;
            var oldProp = this.cascadingProperties[attribute];
            if (oldProp) {
                oldProp.subscription.dispose();
                delete this.cascadingProperties[attribute];
            }
            if (prop != null) {
                this.cascadingProperties[attribute] = new CascadingProperty(prop, prop.InternalValue.subscribe(function (newVal) {
                    if (!_this.isNull() && _this.filter) {
                        if (_this.IsMultiValued) {
                            var lst = _this.InternalValue();
                            _this.InternalValue(lst.filter(_this.filter, _this));
                        }
                        else if (!_this.filter(_this.InternalValue()))
                            _this.InternalValue(null);
                    }
                    _this.updateValueList();
                }, this));
            }
        };
        // The method that determines if a given possible value matches the current values
        // of all cascading properties using the attribute specified for each property.
        // Cascading properties with blank values are ignored, i.e. a blank value
        // is considered to match any value.
        // This method is used as part of the default filter function <see cref="IsAllowed"/>,
        // but can also be used separately as part of a custom filter function.
        // Parameter h: The possible value to match against cascading properties.
        // It should have the same attributes as specified for each cascading property.</param>
        // Returns: True, if the specified value matches the current value(s) of all cascading properties,
        // false otherwise.
        EnumProperty.prototype.matchesCascadingProperties = function (h) {
            for (var attr in this.cascadingProperties) {
                if (!this.cascadingProperties.hasOwnProperty(attr) ||
                    !(this.cascadingProperties[attr] instanceof CascadingProperty))
                    continue;
                var p = this.cascadingProperties[attr].property;
                var pv = p.TransportValue(); // use transport values (IDs) for correct comparison
                // resolve attribute to transport though internal first
                // to handle possible string/number differences
                var hv = p.resolveValue(h.attr[attr], xomega.ValueFormat.Internal);
                hv = p.resolveValue(hv, xomega.ValueFormat.Transport);
                if (p.isNull() || p.isValueNull(hv, xomega.ValueFormat.Transport))
                    continue;
                var match;
                if ($.isArray(hv)) {
                    if ($.isArray(pv)) {
                        match = $.grep(pv, function (v) {
                            return $.inArray(v, hv) > -1;
                        }).length > 0;
                    }
                    else
                        match = $.inArray(pv, hv) > -1;
                }
                else
                    match = $.isArray(pv) ? $.inArray(hv, pv) > -1 : (hv == pv);
                if (!match)
                    return false;
            }
            return true;
        };
        return EnumProperty;
    }(xomega.DataProperty));
    xomega.EnumProperty = EnumProperty;
    // Internal data structure to hold a property and a subscription for cascading selection
    var CascadingProperty = (function () {
        function CascadingProperty(prop, subscr) {
            this.property = prop;
            this.subscription = subscr;
        }
        return CascadingProperty;
    }());
})(xomega || (xomega = {}));
// Copyright (c) 2017 Xomega.Net. All rights reserved.
var xomega;
(function (xomega) {
    // A data property that has enumerated set of possible values
    // that come from a lookup table of the specified type.
    // Internally the values are stored as objects of type Header,
    // which can store ID, text and a number of additional attributes for the value.
    // When a value is being set to the property it tries to resolve it to a Header
    // by looking it up in the lookup table for the property, which is obtained
    // from a lookup cache of a given type.
    var OperatorProperty = (function (_super) {
        __extends(OperatorProperty, _super);
        // Constructs a new OperatorProperty.
        function OperatorProperty() {
            var _this = _super.call(this) || this;
            // The string format that is used to obtain the key field from the Header.
            // The name of the operator attribute that stores the number of additional
            // properties of the operator requires: 0, 1 or 2.
            _this.AttributeAddlProps = "addl props";
            // The name of the operator attribute that stores 1 or 0 to indicate
            // if the additional property can be multivalued.
            _this.AttributeMultival = "multival";
            // The name of the operator attribute that stores a fully qualified type
            // of the additional property, which this operator applies to.
            // It will also apply to all subclasses of this type. Multiple types can be specified.
            _this.AttributeType = "type";
            // The name of the operator attribute that stores a fully qualified type
            // of the additional property, which this operator does not apply to.
            // It won't also apply to all subclasses of this type. Multiple exclude types can be specified.
            // Exclude types should be generally more concrete than include types.
            _this.AttributeExcludeType = "exclude type";
            // The name of the operator attribute that stores the sort order
            // of the operators with respect to other operators.
            _this.AttributeSortOrder = "sort order";
            // The name of the operator attribute that stores 1 for null check operators
            // (Is Null or Is Not Null) to enable easily hiding or showing them.
            _this.AttributeNullCheck = "null check";
            // Gets or sets a Boolean to enable or disable display of the null check operators.
            _this.HasNullCheck = false;
            _this.filter = _this.isApplicable;
            _this.compare = function (h1, h2) {
                var s1 = h1.attr[_this.AttributeSortOrder];
                var s2 = h2.attr[_this.AttributeSortOrder];
                return s1 < s2 ? -1 : s1 > s2 ? 1 : 0;
            };
            _this.InternalValue.subscribe(_this.onValueChanged, _this);
            return _this;
        }
        // Default additional property names based on the name of this property
        OperatorProperty.prototype.onInitialized = function () {
            if (this.AdditionalPropertyName == null && this.Name.match(/Operator$/))
                this.AdditionalPropertyName = this.Name.substring(0, this.Name.length - 8);
            if (this.AdditionalPropertyName2 == null && this.AdditionalPropertyName != null)
                this.AdditionalPropertyName2 = this.AdditionalPropertyName + "2";
            _super.prototype.onInitialized.call(this);
        };
        // Determines if the given operator is applicable for the current additional properties
        // by checking the first additional property type and whether or not it's multivalued
        // and comparing it to the corresponding attributes of the given operator.
        // This method is used as a filter function for the list of operators to display.
        OperatorProperty.prototype.isApplicable = function (oper) {
            var addlProp = this.Parent ? this.Parent()[this.AdditionalPropertyName] : null;
            var multiVal = oper.attr[this.AttributeMultival];
            if (addlProp == null && multiVal != null || addlProp != null && (multiVal == "0" && addlProp.IsMultiValued ||
                multiVal == "1" && !addlProp.IsMultiValued))
                return false;
            var nullCheck = oper.attr[this.AttributeNullCheck];
            if (nullCheck == "1" && !this.HasNullCheck)
                return false;
            var type = oper.attr[this.AttributeType];
            var exclType = oper.attr[this.AttributeExcludeType];
            if (type == null && exclType == null)
                return true;
            if (addlProp == null)
                return false;
            // probe exclude types first
            if ($.isArray(exclType)) {
                for (var i = 0; i < exclType.length; i++)
                    if (this.typeMatches(addlProp, exclType[i]))
                        return false;
            }
            else if (this.typeMatches(addlProp, exclType))
                return false;
            // probe include types next
            if ($.isArray(type)) {
                for (i = 0; i < type.length; i++)
                    if (this.typeMatches(addlProp, type[i]))
                        return true;
                return false;
            }
            return this.typeMatches(addlProp, type);
        };
        // Determines if the specified type or any of its base types match the provided name.
        OperatorProperty.prototype.typeMatches = function (prop, name) {
            var propClass = /(\w+)\(/.exec(prop.constructor.toString())[1];
            return (propClass == name) || prop.__proto__ != null && this.typeMatches(prop.__proto__, name);
        };
        // Updates the visibility and the Required flag of the additional properties
        // based on the currently selected operator. This method is triggered
        // whenever the current operator changes.
        OperatorProperty.prototype.onValueChanged = function (newVal) {
            var addlProp = this.Parent()[this.AdditionalPropertyName];
            var addlProp2 = this.Parent()[this.AdditionalPropertyName2];
            if (addlProp == null)
                return;
            var depCnt = 0;
            if (!this.isNull())
                depCnt = newVal.attr[this.AttributeAddlProps];
            addlProp.Visible(this.Visible() && depCnt > 0);
            addlProp.Required(addlProp.Visible());
            if (addlProp2 != null) {
                addlProp2.Visible(this.Visible() && depCnt > 1);
                addlProp2.Required(addlProp2.Visible());
            }
        };
        return OperatorProperty;
    }(xomega.EnumProperty));
    xomega.OperatorProperty = OperatorProperty;
})(xomega || (xomega = {}));
// Copyright (c) 2017 Xomega.Net. All rights reserved.
var xomega;
(function (xomega) {
    // A data property that has a string value. The maximum length of the string
    // can be specified by setting the DataProperty.Size on the data property.
    var TextProperty = (function (_super) {
        __extends(TextProperty, _super);
        //  Constructs a new TextProperty.
        function TextProperty() {
            var _this = _super.call(this) || this;
            _this.Validators.push(TextProperty.validateSize);
            return _this;
        }
        // Converts a single value to a given format, which is always a string.
        TextProperty.prototype.convertValue = function (value, format) {
            if (xomega.DataProperty.isTypedFormat(format))
                return '' + value;
            return _super.prototype.convertValue.call(this, value, format);
        };
        // A validation function that checks if the value length is not greater
        // than the property size and reports a validation error if it is.
        TextProperty.validateSize = function (dp, value) {
            if (dp != null && !dp.isValueNull(value, xomega.ValueFormat.Internal)
                && dp.Size > 0 && ('' + value).length > dp.Size)
                dp.ValidationErrors.addError("{0} cannot be longer than {1} characters. Invalid value: {2}.", dp, dp.Size, value);
        };
        return TextProperty;
    }(xomega.DataProperty));
    xomega.TextProperty = TextProperty;
    // A data property that holds GUID values.
    var GuidProperty = (function (_super) {
        __extends(GuidProperty, _super);
        //  Constructs a new GuidProperty.
        function GuidProperty() {
            return _super.call(this) || this;
        }
        return GuidProperty;
    }(TextProperty));
    xomega.GuidProperty = GuidProperty;
})(xomega || (xomega = {}));
// Copyright (c) 2017 Xomega.Net. All rights reserved.
var xomega;
(function (xomega) {
    // A data property that holds Boolean values.
    var BooleanProperty = (function (_super) {
        __extends(BooleanProperty, _super);
        //  Constructs a new BooleanProperty.
        function BooleanProperty() {
            return _super.call(this) || this;
        }
        // Converts a single value to a given format. For typed formats
        // this method tries to convert various types of values to a nullable Boolean
        // and may utilize lists of strings that represent true or false values
        // (see rueStrings and FalseStrings).
        BooleanProperty.prototype.convertValue = function (value, format) {
            if (xomega.DataProperty.isTypedFormat(format)) {
                if (typeof value === 'boolean')
                    return value;
                if (1 == value)
                    return true;
                if (0 == value)
                    return false;
                if (this.isValueNull(value, format))
                    return null;
                var str = ('' + value).trim().toLowerCase();
                if (BooleanProperty.TrueStrings.indexOf(str) > -1)
                    return true;
                if (BooleanProperty.FalseStrings.indexOf(str) > -1)
                    return false;
            }
            return _super.prototype.convertValue.call(this, value, format);
        };
        return BooleanProperty;
    }(xomega.DataProperty));
    // An array of strings that should be parsed as a true Boolean value.
    // To default values are: "true", "1", "yes".
    // It can also be set externally for a more precise control over this behavior.
    BooleanProperty.TrueStrings = ["true", "1", "yes", "y"];
    // An array of strings that should be parsed as a false Boolean value.
    // To default values are: "false", "0", "no".
    // It can also be set externally for a more precise control over this behavior.
    BooleanProperty.FalseStrings = ["false", "0", "no", "n"];
    xomega.BooleanProperty = BooleanProperty;
})(xomega || (xomega = {}));
// Copyright (c) 2017 Xomega.Net. All rights reserved.
var xomega;
(function (xomega) {
    // A data property that holds numeric values.
    var DecimalProperty = (function (_super) {
        __extends(DecimalProperty, _super);
        //  Constructs a new DecimalProperty.
        function DecimalProperty() {
            var _this = _super.call(this) || this;
            _this.Validators.push(DecimalProperty.validateNumber, DecimalProperty.validateMinimum, DecimalProperty.validateMaximum);
            return _this;
        }
        // Converts a single value to a given format. For typed formats
        // this method tries to convert various types of values to a decimal.
        // For string formats it displays the internal decimal formatted according
        // to the specified DisplayFormat if set.
        DecimalProperty.prototype.convertValue = function (value, fmt) {
            if (xomega.DataProperty.isTypedFormat(fmt)) {
                if (typeof value === 'number')
                    return value;
                if (this.isValueNull(value, fmt))
                    return null;
                return isNaN(value) ? value : parseFloat('' + value);
            }
            if (fmt == xomega.ValueFormat.DisplayString && typeof value === 'number' && !this.isValueNull(value, fmt)) {
                var s = this.FractionDigits ? value.toFixed(this.FractionDigits) : value;
                if (this.DisplayFormat)
                    s = xomega.format(this.DisplayFormat, s);
                return s;
            }
            return _super.prototype.convertValue.call(this, value, fmt);
        };
        // A validation function that checks if the value is a number and reports a validation error if not.
        DecimalProperty.validateNumber = function (dp, value) {
            if (dp != null && !dp.isValueNull(value, xomega.ValueFormat.Internal) && typeof value !== 'number')
                dp.ValidationErrors.addError("{0} must be a number.", dp);
        };
        // A validation function that checks if the value is a decimal that is not less
        // than the property minimum and reports a validation error if it is.
        DecimalProperty.validateMinimum = function (dp, value) {
            if (dp.MinimumValue && (typeof value === 'number') && value < dp.MinimumValue)
                dp.ValidationErrors.addError("{0} cannot be less than {1}.", dp, dp.MinimumValue);
        };
        // A validation function that checks if the value is a decimal that is not greater
        // than the property maximum and reports a validation error if it is.
        DecimalProperty.validateMaximum = function (dp, value) {
            if (dp.MaximumValue && (typeof value === 'number') && value > dp.MaximumValue)
                dp.ValidationErrors.addError("{0} cannot be greater than {1}.", dp, dp.MaximumValue);
        };
        return DecimalProperty;
    }(xomega.DataProperty));
    xomega.DecimalProperty = DecimalProperty;
    // A data property that holds non-negative numeric values.
    var PositiveDecimalProperty = (function (_super) {
        __extends(PositiveDecimalProperty, _super);
        //  Constructs a new PositiveDecimalProperty.
        function PositiveDecimalProperty() {
            var _this = _super.call(this) || this;
            _this.MinimumValue = 0;
            return _this;
        }
        return PositiveDecimalProperty;
    }(DecimalProperty));
    xomega.PositiveDecimalProperty = PositiveDecimalProperty;
})(xomega || (xomega = {}));
// Copyright (c) 2017 Xomega.Net. All rights reserved.
var xomega;
(function (xomega) {
    // A data property that holds numeric currency values.
    var MoneyProperty = (function (_super) {
        __extends(MoneyProperty, _super);
        //  Constructs a new MoneyProperty.
        function MoneyProperty() {
            var _this = _super.call(this) || this;
            _this.FractionDigits = 2;
            _this.DisplayFormat = MoneyProperty.DefaultMoneyFormat;
            return _this;
        }
        return MoneyProperty;
    }(xomega.DecimalProperty));
    // Default format for displaying currency.
    MoneyProperty.DefaultMoneyFormat = "${0}";
    xomega.MoneyProperty = MoneyProperty;
    // A data property that holds non-negative numeric currency values.
    var PositiveMoneyProperty = (function (_super) {
        __extends(PositiveMoneyProperty, _super);
        //  Constructs a new PositiveMoneyProperty.
        function PositiveMoneyProperty() {
            var _this = _super.call(this) || this;
            _this.MinimumValue = 0;
            return _this;
        }
        return PositiveMoneyProperty;
    }(MoneyProperty));
    xomega.PositiveMoneyProperty = PositiveMoneyProperty;
})(xomega || (xomega = {}));
// Copyright (c) 2017 Xomega.Net. All rights reserved.
var xomega;
(function (xomega) {
    // A data property that holds integer values.
    var IntegerProperty = (function (_super) {
        __extends(IntegerProperty, _super);
        //  Constructs a new IntegerProperty.
        function IntegerProperty() {
            return _super.call(this) || this;
        }
        // Converts a single value to a given format. For typed formats
        // this method tries to convert various types of values to an integer.
        IntegerProperty.prototype.convertValue = function (value, fmt) {
            if (xomega.DataProperty.isTypedFormat(fmt)) {
                if (typeof value === 'number')
                    return value;
                if (this.isValueNull(value, fmt))
                    return null;
                return isNaN(value) ? value : parseInt('' + value);
            }
            return _super.prototype.convertValue.call(this, value, fmt);
        };
        return IntegerProperty;
    }(xomega.DecimalProperty));
    xomega.IntegerProperty = IntegerProperty;
    // A data property that holds non-negative integer values.
    var PositiveIntegerProperty = (function (_super) {
        __extends(PositiveIntegerProperty, _super);
        //  Constructs a new PositiveIntegerProperty.
        function PositiveIntegerProperty() {
            var _this = _super.call(this) || this;
            _this.MinimumValue = 0;
            return _this;
        }
        return PositiveIntegerProperty;
    }(IntegerProperty));
    xomega.PositiveIntegerProperty = PositiveIntegerProperty;
})(xomega || (xomega = {}));
// Copyright (c) 2017 Xomega.Net. All rights reserved.
var xomega;
(function (xomega) {
    // A data property that holds date/time values.
    var DateTimeProperty = (function (_super) {
        __extends(DateTimeProperty, _super);
        //  Constructs a new DecimalProperty.
        function DateTimeProperty() {
            var _this = _super.call(this) || this;
            // A string used to indicate the value type in the validation error
            _this.valueType = "date/time";
            _this.Validators.push(DateTimeProperty.validateDateTime);
            _this.FormatOptions = $.extend(DateTimeProperty.DefaultFormatOptions);
            _this.EditFormat = DateTimeProperty.DefaultEditFormat;
            return _this;
        }
        // Converts a single value to a given format. For typed formats
        // this method tries to convert various types of values to a Date.
        // For string formats it displays the internal Date formatted as date and time
        // according to the current locale and FormatOptions or the Format if set.
        DateTimeProperty.prototype.convertValue = function (value, outFormat, inFormat) {
            if (outFormat === xomega.ValueFormat.Internal) {
                if (value instanceof Date)
                    return value;
                if (this.isValueNull(value, outFormat))
                    return null;
                if (inFormat === xomega.ValueFormat.Transport) {
                    // for optimal performance, transport format should be ISO-compliant to work well with native Date code
                    var v = new Date(value);
                    if (!isNaN(v.valueOf()))
                        return v;
                    // otherwise, try parsing with moment using configured JSON formats (can result in slow performance)
                    var m = moment(value, DateTimeProperty.JsonFormat, true);
                    if (!m.isValid())
                        return value;
                }
                // if not from transport layer, the value is expected to be in edit format
                if (moment.isMoment(value))
                    return value.toDate();
                m = moment(value, this.EditFormat);
                if (!m.isValid())
                    return value;
                // adjust short year inputs, treating '< 50' as current century and '>= 50' and '< 100' as the previous one
                if (m.year() < 100) {
                    var baseYear = ((moment().year() / 100) | 0) * 100;
                    if (m.year() >= 50)
                        baseYear = baseYear - 100;
                    m.year(m.year() + baseYear);
                }
                return m.toDate();
            }
            if (outFormat === xomega.ValueFormat.Transport) {
                var str = JSON.stringify(value).replace(/^"/, "").replace(/"$/, "");
                return str;
            }
            if (xomega.DataProperty.isStringFormat(outFormat) && value instanceof Date && !this.isValueNull(value, outFormat)) {
                if (outFormat === xomega.ValueFormat.EditString)
                    return moment(value).format(this.EditFormat);
                // instantiate format once (if not set) as this is an expensive operation
                if (!this.Format) {
                    // the following construct returns a Collator for whaterver reason (MS bug?),
                    // so we assign through an untyped local variable to avoid compilation issue
                    var dtFmt = new Intl.DateTimeFormat([], this.FormatOptions);
                    this.Format = dtFmt;
                }
                return this.Format.format(value);
            }
            return _super.prototype.convertValue.call(this, value, outFormat, inFormat);
        };
        // A validation function that checks if the value is a number and reports a validation error if not.
        DateTimeProperty.validateDateTime = function (dp, value) {
            if (dp != null && !dp.isValueNull(value, xomega.ValueFormat.Internal) && !(value instanceof Date))
                dp.ValidationErrors.addError("{0} has an invalid " + dp.valueType +
                    ": {1}. Please use the correct format, e.g. {2}.", dp, value, dp.convertValue(new Date(), xomega.ValueFormat.EditString));
        };
        return DateTimeProperty;
    }(xomega.DataProperty));
    // Default date time edit format
    DateTimeProperty.DefaultEditFormat = 'YYYY-MM-DD HH:mm';
    // Default date time display format options that can be reset globally
    DateTimeProperty.DefaultFormatOptions = {
        year: 'numeric',
        month: 'numeric',
        day: 'numeric',
        hour12: false,
        hour: '2-digit',
        minute: 'numeric'
    };
    // JSON transport format (for use with moment.js parser when not ISO compliant and can't be parsed by Date)
    DateTimeProperty.JsonFormat = ['YYYY-MM-DDTHH:mm:ss.SSSSZ', 'YYYY-MM-DDTHH:mm:ss.SSS', 'YYYY-MM-DDTHH:mm:ss.SS', 'YYYY-MM-DDTHH:mm:ss.S', 'YYYY-MM-DDTHH:mm:ss'];
    xomega.DateTimeProperty = DateTimeProperty;
})(xomega || (xomega = {}));
// Copyright (c) 2017 Xomega.Net. All rights reserved.
/// <reference path="DateTimeProperty.ts"/>
var xomega;
(function (xomega) {
    // A DateTimeProperty for the date part only.
    var DateProperty = (function (_super) {
        __extends(DateProperty, _super);
        //  Constructs a new DateProperty.
        function DateProperty() {
            var _this = _super.call(this) || this;
            _this.valueType = "date";
            _this.FormatOptions = {
                year: 'numeric',
                month: 'numeric',
                day: 'numeric',
                hour12: false,
            };
            _this.EditFormat = DateProperty.DefaultEditFormat;
            return _this;
        }
        // Converts a single value to a given format. For typed formats
        // this method tries to convert various types of values to a Date.
        // For string formats it displays the internal Date formatted as date
        // according to the current locale and FormatOptions or the Format if set.
        DateProperty.prototype.convertValue = function (value, outFormat, inFormat) {
            if (xomega.DataProperty.isTypedFormat(outFormat)) {
                var dt = _super.prototype.convertValue.call(this, value, outFormat, inFormat);
                if (dt instanceof Date) {
                    dt.setHours(0);
                    dt.setMinutes(0);
                    dt.setSeconds(0);
                    dt.setMilliseconds(0);
                }
                return dt;
            }
            return _super.prototype.convertValue.call(this, value, outFormat, inFormat);
        };
        return DateProperty;
    }(xomega.DateTimeProperty));
    // Default date edit format
    DateProperty.DefaultEditFormat = 'YYYY-MM-DD';
    xomega.DateProperty = DateProperty;
})(xomega || (xomega = {}));
// Copyright (c) 2017 Xomega.Net. All rights reserved.
var xomega;
(function (xomega) {
    // A DateTimeProperty for the time part only.
    var TimeProperty = (function (_super) {
        __extends(TimeProperty, _super);
        //  Constructs a new DateProperty.
        function TimeProperty() {
            var _this = _super.call(this) || this;
            // A Boolean flag to control whether to treat a single integer under 24
            // as minutes or hours. The default is to treat it as hours.
            _this.MinutesCentric = false;
            _this.valueType = "time";
            _this.FormatOptions = {
                hour12: false,
                hour: '2-digit',
                minute: 'numeric'
            };
            _this.EditFormat = TimeProperty.DefaultEditFormat;
            return _this;
        }
        // Converts a single value to a given format. For typed formats
        // this method tries to convert various types of values to a DateTime.
        // It also handles parsing strings that are input without a colon for speed entry (e.g. 1500).
        TimeProperty.prototype.convertValue = function (value, fmt) {
            if (fmt === xomega.ValueFormat.Internal) {
                var dt = new Date(0, 0, 0, 0, 0, 0, 0);
                var s = ('' + value).trim();
                var i = parseInt(s);
                var valid = true;
                if (/^\d+$/.test(s) && i >= 0) {
                    if (s.length == 4) {
                        i = parseInt(s.substr(0, 2));
                        if (i < 24)
                            dt.setHours(i);
                        else
                            valid = false;
                        i = parseInt(s.substr(2));
                        if (i < 59)
                            dt.setMinutes(i);
                        else
                            valid = false;
                    }
                    else if (i > 23 && i < 60 || i < 24 && this.MinutesCentric)
                        dt.setMinutes(i);
                    else if (i < 24)
                        dt.setHours(i);
                    else
                        valid = false;
                }
                else {
                    // try JSON format, ignoring fraction part
                    var m = moment(s, TimeProperty.JsonFormat, true);
                    if (m.isValid())
                        dt = m.toDate();
                    else
                        dt = _super.prototype.convertValue.call(this, value, fmt);
                    valid = dt instanceof Date;
                }
                return valid ? dt : value;
            }
            if (fmt === xomega.ValueFormat.Transport) {
                var str = value instanceof Date ? moment(value).format(TimeProperty.JsonFormat[0]) : value;
                return str;
            }
            return _super.prototype.convertValue.call(this, value, fmt);
        };
        return TimeProperty;
    }(xomega.DateTimeProperty));
    // Default time edit format
    TimeProperty.DefaultEditFormat = 'HH:mm';
    // JSON transport format
    TimeProperty.JsonFormat = ['HH:mm:ss.SSS', 'HH:mm:ss'];
    xomega.TimeProperty = TimeProperty;
})(xomega || (xomega = {}));
// Copyright (c) 2017 Xomega.Net. All rights reserved.
var xomega;
(function (xomega) {
    var ViewModel = (function () {
        function ViewModel() {
            this.errorList = new xomega.ErrorList();
            // id prefix for the current view to append to all IDs
            this.idPfx = '';
            // observable current active child view
            this.ActiveChildView = ko.observable();
            // callbacks for view events
            this.ViewEvents = $.Callbacks();
        }
        Object.defineProperty(ViewModel.prototype, "Params", {
            // Retirns parameters the view model was last activated with
            get: function () { return this._params; },
            enumerable: true,
            configurable: true
        });
        // Activates the view model
        ViewModel.prototype.activateAsync = function (params) {
            this._params = params || {};
            this.ActiveChildView(null); // reset active child
            return $.when(true);
        };
        // implements Durandal lifecycle function to activate the view model if needed
        ViewModel.prototype.canActivate = function () {
            // activation parameters will be from the query, which is the last argument
            return this.activateAsync(arguments.length > 0 ? arguments[arguments.length - 1] : null);
        };
        // implements Durandal lifecycle function to activate the view model if needed
        ViewModel.prototype.canDeactivate = function () {
            return $.when(this.canClose());
        };
        ViewModel.prototype.getErrorList = function () { return this.errorList; };
        // Returns global id for the given local view id
        ViewModel.prototype.id = function (localId) {
            return this.idPfx + localId;
        };
        // navigate to a child view asynchrounously
        ViewModel.prototype.navigateTo = function (childViewName, activationParams, idPfx) {
            if (idPfx === void 0) { idPfx = '_'; }
            var vm = this;
            return this.acquireView(childViewName).then(function (view) {
                var res;
                // check if active view exists and can be reused
                if (vm.ActiveChildView() && vm.ActiveChildView().canReuseView(view)) {
                    if (vm.ActiveChildView().sameParams(activationParams))
                        return $.when(false); // same params, keep the current view
                    else
                        view = vm.ActiveChildView(); // reuse the current view
                }
                else
                    vm.subscribeToChildEvents(view);
                var canDeactivate = vm.ActiveChildView() ? vm.ActiveChildView().canDeactivate() : $.when(true);
                return canDeactivate.then(function (success) {
                    if (!success)
                        return false;
                    return view.activateAsync(activationParams).then(function (success) {
                        if (!success)
                            return false;
                        if (view !== vm.ActiveChildView()) {
                            view.idPfx = vm.idPfx + idPfx;
                            vm.ActiveChildView(view);
                        }
                        return true;
                    });
                });
            }).fail(function (err, msg) {
                if (vm.getErrorList())
                    vm.getErrorList().mergeWith(xomega.ErrorList.fromErrorResponse(err, msg));
            });
        };
        ViewModel.prototype.acquireView = function (viewName) {
            return $.Deferred(function (dfd) {
                require([viewName], function (mod) {
                    var view = null;
                    if ($.isFunction(mod)) {
                        view = new mod();
                        if (view instanceof ViewModel)
                            dfd.resolve(view);
                        else
                            dfd.reject("View '" + viewName + "' should be an instance of ViewModel");
                    }
                    else
                        dfd.reject("View '" + viewName + "' should be a constructor function");
                }, function (err) { dfd.reject(err); });
            }).promise();
        };
        // Checks if we can reuse the current view if they are of the same type. Can be overridden in subclasses
        ViewModel.prototype.canReuseView = function (view) {
            return view && this.constructor === view.constructor;
        };
        // Checks if activation parameters are the same as in this view. Can be overridden in subclasses
        ViewModel.prototype.sameParams = function (activationParams) {
            return $.param(this.Params) === $.param(activationParams);
        };
        // Checks if the view can be closed.
        ViewModel.prototype.canClose = function () {
            return this.ActiveChildView() ? this.ActiveChildView().canClose() : true;
        };
        // Checks if the view has the Close button visible
        ViewModel.prototype.hasClose = function () {
            // display Close button only if the view is activated as a child (popup or inline)
            return this.Params && this.Params[xomega.ViewParams.Mode];
        };
        // Close the view by firing a closed event
        ViewModel.prototype.close = function () {
            if (this.canClose())
                this.fireViewEvent(xomega.ViewEvent.Closed);
        };
        // Adds a listener to the view events
        ViewModel.prototype.onViewEvent = function (callback) {
            this.ViewEvents.add(callback);
        };
        // Fires the specified view event
        ViewModel.prototype.fireViewEvent = function (event, source) {
            if (source === void 0) { source = this; }
            this.ViewEvents.fire(source, event);
        };
        // Subscribes to child view's events
        ViewModel.prototype.subscribeToChildEvents = function (child) {
            if (child) {
                var vm_1 = this;
                child.onViewEvent(function (view, event) { return vm_1.onChildEvent(view, event); });
            }
        };
        // Default handler for child events, which just re-publishes them.
        ViewModel.prototype.onChildEvent = function (childViewModel, e) {
            this.fireViewEvent(e.with(xomega.ViewEvent.Child), childViewModel);
            if (e.isClosed() && childViewModel === this.ActiveChildView())
                this.ActiveChildView(null);
        };
        return ViewModel;
    }());
    xomega.ViewModel = ViewModel;
})(xomega || (xomega = {}));
// Copyright (c) 2017 Xomega.Net. All rights reserved.
/// <reference path="ViewModel.ts"/>
var xomega;
(function (xomega) {
    var DetailsViewModel = (function (_super) {
        __extends(DetailsViewModel, _super);
        function DetailsViewModel() {
            return _super !== null && _super.apply(this, arguments) || this;
        }
        /** Activates the details view asynchronously */
        DetailsViewModel.prototype.activateAsync = function (params) {
            var vm = this;
            return _super.prototype.activateAsync.call(this, params).then(function (success) {
                if (!success || !vm.DetailsObject)
                    return false;
                if (vm.Params[xomega.ViewParams.Action] === xomega.ViewParams.ActionCreate) {
                    vm.DetailsObject.reset();
                    vm.DetailsObject.fromJSON(vm.Params);
                    vm.DetailsObject.Modified(false);
                    return true;
                }
                else {
                    vm.DetailsObject.fromJSON(vm.Params);
                    vm.DetailsObject.Modified(null);
                    return vm.loadDataAsync();
                }
            });
        };
        DetailsViewModel.prototype.getErrorList = function () { return this.DetailsObject ? this.DetailsObject.ValidationErrors : _super.prototype.getErrorList.call(this); };
        // Pops up a confirmation dialog for modified object before closing
        DetailsViewModel.prototype.canClose = function () {
            if (!_super.prototype.canClose.call(this))
                return false;
            if (this.DetailsObject) {
                if (this.DetailsObject.Modified() && !confirm('Do you want to discard unsaved changes?'))
                    return false;
                this.DetailsObject.Modified(false);
            }
            return true;
        };
        // Pops up a confirmation dialog before deleting
        DetailsViewModel.prototype.canDelete = function () {
            if (!this.DetailsObject || this.DetailsObject.IsNew() || !confirm("Are you sure you want to delete this object?\nThis operation cannot be undone."))
                return false;
            return true;
        };
        // Handles the save action
        DetailsViewModel.prototype.onSave = function () {
            if (this.DetailsObject) {
                var vm_2 = this;
                this.DetailsObject.saveAsync().then(function () { return vm_2.fireViewEvent(xomega.ViewEvent.Saved); });
            }
        };
        // Handles the delete action, and closes the view on successful delete
        DetailsViewModel.prototype.onDelete = function () {
            if (this.canDelete() && this.DetailsObject) {
                var vm_3 = this;
                this.DetailsObject.deleteAsync().then(function () {
                    vm_3.fireViewEvent(xomega.ViewEvent.Deleted);
                    vm_3.close();
                });
            }
        };
        /**
         * Loads the details view data asynchronously.
         */
        DetailsViewModel.prototype.loadDataAsync = function () {
            return this.DetailsObject ? this.DetailsObject.readAsync() : $.when(true);
        };
        // Default handler for saving or deleting of a child details view.
        DetailsViewModel.prototype.onChildEvent = function (childViewModel, e) {
            if (e.isSaved() || e.isDeleted())
                this.loadDataAsync(); // reload child lists
            _super.prototype.onChildEvent.call(this, childViewModel, e);
        };
        return DetailsViewModel;
    }(xomega.ViewModel));
    xomega.DetailsViewModel = DetailsViewModel;
})(xomega || (xomega = {}));
// Copyright (c) 2017 Xomega.Net. All rights reserved.
/// <reference path="ViewModel.ts"/>
var xomega;
(function (xomega) {
    var SearchViewModel = (function (_super) {
        __extends(SearchViewModel, _super);
        function SearchViewModel() {
            var _this = _super !== null && _super.apply(this, arguments) || this;
            // An indicator if the search criteria panel is collapsed
            _this.CriteriaCollapsed = ko.observable();
            return _this;
        }
        Object.defineProperty(SearchViewModel.prototype, "AutoCollapseCriteria", {
            // Controls if criteria panel will automatically collapse/expand on Search/Reset.
            get: function () { return true; },
            enumerable: true,
            configurable: true
        });
        /** Activates the search view asynchronously */
        SearchViewModel.prototype.activateAsync = function (params) {
            var vm = this;
            return _super.prototype.activateAsync.call(this, params).then(function (success) {
                if (!success)
                    return false;
                if (vm.ListObject.CriteriaObject)
                    vm.ListObject.CriteriaObject.fromJSON(vm.Params);
                vm.ListObject.RowSelectionMode(vm.Params[xomega.ViewParams.SelectionMode]);
                if (vm.Params[xomega.ViewParams.Action] === xomega.ViewParams.ActionSearch)
                    return vm.search();
                if (vm.Params[xomega.ViewParams.Action] === xomega.ViewParams.ActionSelect)
                    return vm.searchAsync().then(function (success) {
                        if (!success)
                            return false;
                        return vm.autoSelectAsync();
                    });
                return true;
            });
        };
        SearchViewModel.prototype.getErrorList = function () { return this.ListObject ? this.ListObject.ValidationErrors : _super.prototype.getErrorList.call(this); };
        SearchViewModel.prototype.getPermalink = function () {
            if (!this.ListObject.CriteriaObject)
                return null;
            var qry = $.param(this.ListObject.CriteriaObject.toJSON());
            if (this.ListObject.AppliedCriteria()) {
                if (qry)
                    qry += '&';
                qry += xomega.ViewParams.Action + "=" + xomega.ViewParams.ActionSearch;
            }
            if (qry)
                qry = '?' + qry;
            return qry;
        };
        // Search function exposed as an event handler for the Search button
        SearchViewModel.prototype.search = function () {
            var vm = this;
            return this.searchAsync(true).then(function (success) {
                if (success && vm.AutoCollapseCriteria)
                    vm.CriteriaCollapsed(true);
                return success;
            });
        };
        // Resets current view to initial state
        SearchViewModel.prototype.reset = function (full) {
            if (full === void 0) { full = true; }
            if (this.ListObject)
                this.ListObject.reset();
            if (this.AutoCollapseCriteria)
                this.CriteriaCollapsed(false);
        };
        /** Runs the search asynchronously */
        SearchViewModel.prototype.searchAsync = function (preserveSelection) {
            if (preserveSelection === void 0) { preserveSelection = true; }
            var vm = this;
            if (vm.ListObject)
                vm.ListObject.validate(true);
            if (vm.ListObject.ValidationErrors.hasErrors())
                return $.Deferred().reject(vm.ListObject.ValidationErrors);
            return vm.ListObject.readAsync({ preserveSelection: preserveSelection });
        };
        /** Runs the search asynchronously if criteria are provided, and auto-selects the result */
        SearchViewModel.prototype.autoSelectAsync = function (preserveSelection) {
            if (preserveSelection === void 0) { preserveSelection = true; }
            var vm = this;
            if (!vm.ListObject || vm.ListObject.CriteriaObject && !vm.ListObject.CriteriaObject.hasCriteria())
                return $.when(true);
            return vm.searchAsync(false).then(function (success) {
                if (!success)
                    return false;
                var rowCount = vm.ListObject.List().length;
                if (rowCount > 1 && vm.AutoCollapseCriteria)
                    vm.CriteriaCollapsed(true);
                else if (rowCount == 0)
                    vm.CriteriaCollapsed(false);
                else if (rowCount == 1) {
                    vm.ListObject.List()[0]._selected(true);
                    vm.fireViewEvent(new xomega.ViewSelectionEvent(vm.ListObject.getSelectedRows()));
                    return false; // single row returned, no need to activate the view
                }
                return true;
            });
        };
        // Select function exposed as an event handler for the Select button
        SearchViewModel.prototype.select = function () {
            if (this.ListObject) {
                this.fireViewEvent(new xomega.ViewSelectionEvent(this.ListObject.getSelectedRows()));
                this.close();
            }
        };
        // Checks if the view has the Select button visible
        SearchViewModel.prototype.hasSelect = function () {
            return this.Params && this.Params[xomega.ViewParams.SelectionMode];
        };
        // Handles child closing or change to refresh the list.
        SearchViewModel.prototype.onChildEvent = function (childViewModel, e) {
            if (e.isClosed() && this.ListObject) {
                this.ListObject.clearSelectedRows();
            }
            if (e.isSaved() || e.isDeleted()) {
                this.searchAsync(true);
            }
            _super.prototype.onChildEvent.call(this, childViewModel, e);
        };
        return SearchViewModel;
    }(xomega.ViewModel));
    xomega.SearchViewModel = SearchViewModel;
})(xomega || (xomega = {}));
// Copyright (c) 2017 Xomega.Net. All rights reserved.
var xomega;
(function (xomega) {
    var ViewEvent = (function () {
        // Constructs a view event from the provided flags
        function ViewEvent(events) {
            this.events = events;
        }
        // Returns an event that is this event with the provided event added
        ViewEvent.prototype.with = function (viewEvent) { return new ViewEvent(this.events | viewEvent.events); };
        // Returns an event that is this event with the provided event removed
        ViewEvent.prototype.without = function (viewEvent) { return new ViewEvent(this.events & ~viewEvent.events); };
        // Returns if the view was closed.
        ViewEvent.prototype.isClosed = function (self) {
            if (self === void 0) { self = true; }
            return (self && !this.isChild() || !self) && (this.events & ViewEvent.Closed.events) > 0;
        };
        // Returns if the view was saved.
        ViewEvent.prototype.isSaved = function (self) {
            if (self === void 0) { self = true; }
            return (self && !this.isChild() || !self) && (this.events & ViewEvent.Saved.events) > 0;
        };
        // Returns if the view was closed.
        ViewEvent.prototype.isDeleted = function (self) {
            if (self === void 0) { self = true; }
            return (self && !this.isChild() || !self) && (this.events & ViewEvent.Deleted.events) > 0;
        };
        // Returns if a child view event occured.
        ViewEvent.prototype.isChild = function () { return (this.events & ViewEvent.Child.events) > 0; };
        return ViewEvent;
    }());
    // A static constant representing a combination of all events.
    ViewEvent.All = new ViewEvent(0xFFFF);
    // A static constant representing a Closed event
    ViewEvent.Closed = new ViewEvent(1 << 0);
    // A static constant representing a Saved event
    ViewEvent.Saved = new ViewEvent(1 << 1);
    // A static constant representing a Deleted event
    ViewEvent.Deleted = new ViewEvent(1 << 2);
    // A static constant representing a Child view event
    ViewEvent.Child = new ViewEvent(1 << 3);
    xomega.ViewEvent = ViewEvent;
})(xomega || (xomega = {}));
// Copyright (c) 2017 Xomega.Net. All rights reserved.
var xomega;
(function (xomega) {
    var ViewParams = (function () {
        function ViewParams() {
        }
        return ViewParams;
    }());
    // Parameter indicating action to perform
    ViewParams.Action = '_action';
    // Action to create a new object
    ViewParams.ActionCreate = 'create';
    // Action to initiate search on activation
    ViewParams.ActionSearch = 'search';
    // Action to activate for selection
    ViewParams.ActionSelect = 'select';
    // Query parameter indicating specific source link on the parent that invoked this view
    ViewParams.Source = '_source';
    // Parameter indicating selection mode to set, if any
    ViewParams.SelectionMode = '_selection';
    // Single selection mode
    ViewParams.SelectionModeSingle = xomega.DataListObject.SelectionModeSingle;
    // Multiple selection mode
    ViewParams.SelectionModeMultiple = xomega.DataListObject.SelectionModeMultiple;
    // Parameter for view display modes
    ViewParams.Mode = '_mode';
    // Mode to open views in a popup dialog.
    ViewParams.ModePopup = 'popup';
    // Mode to open views inline as master-details.
    ViewParams.ModeInline = 'inline';
    xomega.ViewParams = ViewParams;
})(xomega || (xomega = {}));
// Copyright (c) 2017 Xomega.Net. All rights reserved.
var xomega;
(function (xomega) {
    var ViewSelectionEvent = (function (_super) {
        __extends(ViewSelectionEvent, _super);
        // Constructs a view selection event with the provided selected rows
        function ViewSelectionEvent(selectedRows) {
            var _this = _super.call(this, 1 << 4) || this;
            _this.selection = selectedRows;
            return _this;
        }
        Object.defineProperty(ViewSelectionEvent.prototype, "SelectedRows", {
            // Selected data rows.
            get: function () { return this.selection; },
            enumerable: true,
            configurable: true
        });
        return ViewSelectionEvent;
    }(xomega.ViewEvent));
    xomega.ViewSelectionEvent = ViewSelectionEvent;
})(xomega || (xomega = {}));
// Copyright (c) 2017 Xomega.Net. All rights reserved.
var xomega;
(function (xomega) {
    // initialize xomega based on the 3rd party libs
    function init(knockout, jQuery, momentjs, xomegaControls) {
        ko = knockout;
        $ = jQuery;
        moment = momentjs;
        controls = xomegaControls;
        xomega.Bindings.init();
    }
    xomega.init = init;
})(xomega || (xomega = {}));
if (typeof module === 'object' && module.exports) {
    // CommonJS (Node)
    module.exports = xomega;
}
else if (typeof define === 'function' && define['amd']) {
    // AMD
    define(['knockout', 'jquery', 'moment', 'xomega-controls'], function (knockout, jquery, momentjs, xomegaControls) {
        xomega.init(knockout, jquery, momentjs, xomegaControls);
        return xomega;
    });
}
//# sourceMappingURL=xomega.js.map