// Copyright (c) 2021 Xomega.Net. All rights reserved.

using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Resources;
using System.Runtime.Serialization;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Xomega.Framework.Services;

namespace Xomega.Framework
{
    /// <summary>
    /// The base class for all data objects, which contain a list of data properties
    /// and possibly a number of child objects or object lists.
    /// </summary>
    [DataContract]
    public abstract class DataObject : INotifyPropertyChanged
    {
        #region Construction

        /// <summary>
        /// Service provider for the data object
        /// </summary>
        public IServiceProvider ServiceProvider { get; set; }

        /// <summary>
        /// Protected default constructor that delegates construction
        /// to the <c>Init</c> method.
        /// </summary>
        protected DataObject()
        {
            Init();
        }

        /// <summary>
        /// Protected default constructor that delegates construction
        /// to the <c>Init</c> method.
        /// </summary>
        protected DataObject(IServiceProvider serviceProvider)
        {
            this.ServiceProvider = serviceProvider;
            Init();
        }

        /// <summary>
        /// OnDeserializing callback to support deserialization, which
        /// delegates construction to the <c>Init</c> method.
        /// </summary>
        /// <param name="context"></param>
        [OnDeserializing]
        private void OnDeserializing(StreamingContext context)
        {
            Init();
        }

        /// <summary>
        /// Initializes the dictionaries for properties and child objects
        /// and calls the main <c>Initialize</c> method to add the actual
        /// properties and child objects.
        /// </summary>
        private void Init()
        {
            properties = new Dictionary<string, DataProperty>();
            childObjects = new Dictionary<string, DataObject>();
            actions = new Dictionary<string, ActionProperty>();

            // add save action, and make it enabled only when the object is modified or doesn't track modifications
            SaveAction = new ActionProperty(this, Messages.Action_Save);
            Expression<Func<DataObject, bool>> saveEnabled = (obj) => obj != null && (obj.Modified || !obj.TrackModifications);
            SaveAction.SetComputedEnabled(saveEnabled, this);

            // add delete action, and make it enabled only when the object is not new
            DeleteAction = new ActionProperty(this, Messages.Action_Delete);
            Expression<Func<DataObject, bool>> deleteEnabled = (obj) => obj != null && !obj.IsNew;
            DeleteAction.SetComputedEnabled(deleteEnabled, this);

            ResetAction = new ActionProperty(this, Messages.Action_Reset);

            // call subclass initialization in a separate method
            // to make sure base initialization is always called
            Initialize();
            OnInitialized();
        }

        /// <summary>
        /// The abstract method to be implemented by the subclasses
        /// to add and initialize data object properties and child objects.
        /// </summary>
        protected abstract void Initialize();

        /// <summary>
        /// Additional initialization that happens after all the properties
        /// and child objects have been added and are therefore accessible.
        /// The base class implementation just calls the <c>Initialize</c> method
        /// on all the data properties to initialize themselves.
        /// If the main <c>Initialize</c> method is generated as part of the 
        /// generated object and therefore cannot be changed, this method
        /// can also be used by the non-generated part of the class
        /// to perform additional post-initialization.
        /// </summary>
        protected virtual void OnInitialized()
        {
            foreach (DataProperty p in properties.Values) p.Initialize();
        }

        /// <summary>
        /// Perform a deep copy of the state from another data object (presumably of the same type).
        /// </summary>
        /// <param name="obj">The object to copy the state from.</param>
        public virtual void CopyFrom(DataObject obj)
        {
            if (!(obj is DataObject dObj)) return;
            foreach (DataProperty p in properties.Values)
                p.CopyFrom(dObj[p.Name]);
            foreach (string chName in childObjects.Keys)
                childObjects[chName].CopyFrom(dObj.GetChildObject(chName));
        }

        /// <summary>
        /// The criteria/list reset action associated with this list object,
        /// which allows controlling the state of the button bound to it.
        /// </summary>
        public ActionProperty ResetAction { get; private set; }

        /// <summary>
        /// Resets data object to initial values
        /// </summary>
        public virtual void ResetData()
        {
            foreach (DataProperty p in properties.Values)
                p.ResetValue();
            foreach (string chName in childObjects.Keys)
                childObjects[chName].ResetData();
            modified = null;
        }

        /// <summary>
        /// Recursively updates computed values for object's actions and editable flag.
        /// </summary>
        public void UpdateComputed()
        {
            foreach (var a in actions.Values)
            {
                a.UpdateComputedEnabed();
                a.UpdateComputedVisible();
            }
            
            foreach (var obj in childObjects.Values)
                obj.UpdateComputed();

            if (editableBinding != null)
                editableBinding.Update(null);
        }

        /// <summary>
        /// Gets a key for the current object that is used to look up localized resources
        /// (e.g. labels) for object's properties.
        /// </summary>
        /// <returns>The resource key for the object.</returns>
        public virtual string GetResourceKey()
        {
            var t = GetType();
            if (t.Name.StartsWith(t.BaseType.Name))
                t = t.BaseType; // use base class for customized objects
            return t.Name;
        }

        /// <summary>
        /// Utility function to convert Pascal-case string to words
        /// </summary>
        /// <param name="str">The string to convert.</param>
        /// <returns>String converted from Pascal-case to words.</returns>
        public static string StringToWords(string str) => Regex.Replace(Regex.Replace(str,
            "([a-z])([A-Z])", "$1 $2"), "([A-Z][A-Z])([A-Z])([a-z])", "$1 $2$3");

        #endregion

        #region Properties

        /// <summary>
        /// The dictionary of the data object properties by their names.
        /// </summary>
        private Dictionary<string, DataProperty> properties;

        /// <summary>
        /// A string-based indexer that returns a data object property by its name
        /// or null if no property with this name is found on the object.
        /// </summary>
        /// <param name="name">The property of the name to return.</param>
        /// <returns>The data property of the data object with the given name or null if not found.</returns>
        public virtual DataProperty this[string name] => HasProperty(name) ? properties[name] : null;

        /// <summary>
        /// Checks of the data object has a property with the given name.
        /// </summary>
        /// <param name="name">The property name to check for existence.</param>
        /// <returns>True if the data object contains a property with the given name, false otherwise.</returns>
        public bool HasProperty(string name) => name != null && properties.ContainsKey(name);

        /// <summary>
        /// Returns an enumeration of the data object properties.
        /// </summary>
        public IEnumerable<DataProperty> Properties => properties.Values;

        /// <summary>
        /// Adds the specified property to the data object.
        /// </summary>
        /// <param name="property">The property to add to the data object.</param>
        internal void AddProperty(DataProperty property)
        {
            properties[property.Name] = property;
            property.Change += OnDataPropertyChange;
        }

        private Dictionary<string, ActionProperty> actions;

        /// <summary>
        /// Adds the specified action to the data object.
        /// </summary>
        /// <param name="action">The action to add to the data object.</param>
        internal void AddAction(ActionProperty action)
        {
            actions[action.Name] = action;
        }

        #endregion

        #region Object hierarchy

        /// <summary>
        /// The parent data object for the current object if any.
        /// </summary>
        private DataObject parent;

        /// <summary>
        /// Gets or sets the parent data object for the current object if any.
        /// </summary>
        public virtual DataObject Parent
        {
            get { return parent; }
            set
            {
                parent = value;
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(Parent)));
            }
        }

        /// <summary>
        /// A dictionary of the current data object's child objects or object lists by their name.
        /// </summary>
        private Dictionary<string, DataObject> childObjects;

        /// <summary>
        /// Adds the specified child object or object list to the current data object
        /// under the given name.
        /// </summary>
        /// <param name="name">The name, under which the child object will be added to the current data object.</param>
        /// <param name="obj">The child object to add to the current object.</param>
        protected void AddChildObject(string name, DataObject obj)
        {
            childObjects[name] = obj;
            obj.Parent = this;
            obj.PropertyChanged += (s, e) => {
                if (e.PropertyName == nameof(Modified))
                  OnPropertyChanged(new PropertyChangedEventArgs(nameof(Modified)));
            };
        }

        /// <summary>
        /// Returns an enumeration of the data object's child objects.
        /// </summary>
        public IEnumerable<DataObject> Children => childObjects.Values;

        /// <summary>
        /// Gets the child object or object list for the given name or null
        /// if no child is found under this name.
        /// </summary>
        /// <param name="name">The name of the child object to return.</param>
        /// <returns>The child object or object list for the given name or null
        /// if no child is found under this name.</returns>
        public DataObject GetChildObject(string name)
        {
            return childObjects.ContainsKey(name) ? childObjects[name] : null;
        }

        /// <summary>
        /// Gets a localized title for the current object.
        /// </summary>
        /// <returns>Localized title, or null if no resource is found.</returns>
        public virtual string GetTitle()
        {
            var resMgr = ServiceProvider?.GetService<ResourceManager>();
            return resMgr.GetString("_Title", GetResourceKey());
        }

        /// <summary>
        /// Gets a localized title for the specified child object. If no title text is available
        /// uses the child name to generate a title.
        /// </summary>
        /// <param name="name">The name of the child.</param>
        /// <returns>A localized title for the given child.</returns>
        public virtual string GetChildTitle(string name)
        {
            var resMgr = ServiceProvider?.GetService<ResourceManager>();
            string title;
            title = resMgr?.GetString($"{GetResourceKey()}-{name}_Title");
            if (title == null) title = StringToWords(name);
            return title;
        }

        /// <summary>
        /// Gets a localized title for the specified link. If no title text is available
        /// uses the link name to generate a title.
        /// </summary>
        /// <param name="name">The name of the link.</param>
        /// <returns>A localized title for the given link.</returns>
        public virtual string GetLinkTitle(string name)
        {
            var resMgr = ServiceProvider?.GetService<ResourceManager>();
            string title;
            title = resMgr?.GetString($"{GetResourceKey()}_Link-{name}_Title");
            if (title == null) title = StringToWords(name);
            return title;
        }

        #endregion

        #region Property change events

        /// <summary>
        /// Fires a property change event recursively through all properties and child objects.
        /// </summary>
        /// <param name="args">Property change event arguments.</param>
        protected void FireDataPropertyChange(PropertyChangeEventArgs args)
        {
            foreach (DataProperty p in properties.Values) p.FirePropertyChange(args);
            foreach (ActionProperty a in actions.Values) a.FirePropertyChange(args);
            foreach (DataObject obj in childObjects.Values) obj.FireDataPropertyChange(args);
            if (args.Change.IncludesEditable())
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(Editable)));
        }

        /// <summary>
        /// Handles change events from own data properties.
        /// Raises modification state change if a property value has changed.
        /// </summary>
        /// <param name="sender">Property that send the event.</param>
        /// <param name="e">The property change event.</param>
        protected virtual void OnDataPropertyChange(object sender, PropertyChangeEventArgs e)
        {
            if (e.Change.IncludesValue())
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(Modified)));
        }

        /// <summary>
        /// Implements INotifyPropertyChanged to notify listeners
        /// about changes in data object's plain properties (not data properties)
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises property changed event
        /// </summary>
        /// <param name="e">Event arguments with property name</param>
        protected void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }

        #endregion

        #region Editability support

        /// <summary>
        /// An internal flag to allow manually making the data object uneditable.
        /// The default value is true.
        /// </summary>
        private bool editable = true;

        /// <summary>
        /// Returns a value indicating whether or not the data object is editable.
        /// This value is calculated based on the internal value of the editable field,
        /// the parent object's editability and the value of the security access level.
        /// Setting this value updates the internal editable flag and fires
        /// a property change event for all properties if necessary.
        /// </summary>
        public bool Editable
        {
            get
            {
                bool b = editable;
                if (parent != null) b &= parent.Editable;
                return b && AccessLevel > AccessLevel.ReadOnly;
            }
            set
            {
                bool oldValue = Editable;
                this.editable = value;
                if (Editable != oldValue) FireDataPropertyChange(
                    new PropertyChangeEventArgs(PropertyChange.Editable, oldValue, Editable, null));
            }
        }

        /// <summary>
        /// Allows controlling property editability on the data object level.
        /// Subclasses can override this method to define custom rules
        /// for property editability.
        /// </summary>
        /// <param name="p">The property to check the editability of.</param>
        /// <returns>True if the property should be editable, false otherwise.</returns>
        public virtual bool IsPropertyEditable(BaseProperty p)
        {
            return Editable && (parent == null || parent.IsPropertyEditable(p));
        }

        private ComputedBinding editableBinding;

        /// <summary>
        /// Sets the expression to use for computing whether the data object is editable.
        /// </summary>
        /// <param name="expression">Lambda expression used to compute the editability,
        /// or null to make the editable flag non-computed.</param>
        /// <param name="args">Arguments for the specified expression to use for evaluation.</param>
        public void SetComputedEditable(LambdaExpression expression, params object[] args)
        {
            if (editableBinding != null) editableBinding.Dispose();
            editableBinding = expression == null ? null : new ComputedEditableObjectBinding(this, expression, args);
        }

        #endregion

        #region Security support

        /// <summary>
        /// Internal field that stores the security access level for the data object.
        /// The default value is full access.
        /// </summary>
        [DataMember]
        private AccessLevel accessLevel = AccessLevel.Full;

        /// <summary>
        /// Returns the current access level for the data object.
        /// Allows setting a new access level and fires a property change event
        /// for editability and visibility of all properties,
        /// since they both depend on the security access level.
        /// </summary>
        public AccessLevel AccessLevel
        {
            get { return accessLevel; }
            set
            {
                AccessLevel oldValue = accessLevel;
                accessLevel = value;
                FireDataPropertyChange(new PropertyChangeEventArgs(
                    PropertyChange.Editable + PropertyChange.Visible, oldValue, accessLevel, null));
            }
        }

        /// <summary>
        /// The principal for the current operation
        /// </summary>
        public IPrincipal CurrentPrincipal => ServiceProvider.GetCurrentPrincipal();

        #endregion

        #region Data Contract support

        /// <summary>
        /// Sets the data object values from the given data contract object
        /// by copying the values of the data contract object fields to the
        /// data object properties or child objects with the same names.
        /// If there is no exact match between some data contract field names
        /// and the data object property names, this method can be overridden
        /// in the subclass to address each such case.
        /// </summary>
        /// <param name="dataContract">The data contract object to copy the values from.</param>
        /// <param name="options">Additional options for the operation.</param>
        public virtual void FromDataContract(object dataContract, object options)
            => FromDataContract(dataContract, options, null);

        /// <summary>
        /// Sets the values of the given data row or, if the row is null,
        /// the data object from the given data contract object
        /// by copying the values of the data contract object fields to the
        /// data object properties or child objects with the same names.
        /// If there is no exact match between some data contract field names
        /// and the data object property names, this method can be overridden
        /// in the subclass to address each such case.
        /// </summary>
        /// <param name="dataContract">The data contract object to copy the values from.</param>
        /// <param name="options">Additional options for the operation.</param>
        /// <param name="row">The row to set the values for.
        /// Null to set values of the current data object.</param>
        protected void FromDataContract(object dataContract, object options, DataRow row)
        {
            if (dataContract == null) return;
            SetModified(false, false);
            foreach (PropertyInfo pi in dataContract.GetType().GetProperties())
            {
                object val = pi.GetValue(dataContract, null);
                DataProperty dp = this[pi.Name];
                DataObject child;
                if (dp != null)
                {
                    dp.Modified = null;
                    dp.SetValue(val, row);
                }
                else if ((child = GetChildObject(pi.Name)) != null)
                {
                    child.FromDataContract(val, options);
                }
                else if (val != null)
                {
                    foreach (PropertyInfo cpi in pi.PropertyType.GetProperties())
                    {
                        DataProperty cdp = this[pi.Name + "_" + cpi.Name];
                        if (cdp != null)
                        {
                            cdp.Modified = null;
                            cdp.SetValue(cpi.GetValue(val, null), row);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Sets the data object values from the given data contract object
        /// by copying the values of the data contract object fields to the
        /// data object properties or child objects with the same names.
        /// If there is no exact match between some data contract field names
        /// and the data object property names, this method can be overridden
        /// in the subclass to address each such case.
        /// </summary>
        /// <param name="dataContract">The data contract object to copy the values from.</param>
        /// <param name="options">Additional options for the operation.</param>
        /// <param name="token">Cancellation token.</param>
        public virtual async Task FromDataContractAsync(object dataContract, object options,
                                                        CancellationToken token = default)
            => await FromDataContractAsync(dataContract, options, null, token);

        /// <summary>
        /// Sets the values of the given data row or, if the row is null,
        /// the data object from the given data contract object
        /// by copying the values of the data contract object fields to the
        /// data object properties or child objects with the same names.
        /// If there is no exact match between some data contract field names
        /// and the data object property names, this method can be overridden
        /// in the subclass to address each such case.
        /// </summary>
        /// <param name="dataContract">The data contract object to copy the values from.</param>
        /// <param name="options">Additional options for the operation.</param>
        /// <param name="row">The row to set the values for.
        /// <param name="token">Cancellation token.</param>
        /// Null to set values of the current data object.</param>
        protected async Task FromDataContractAsync(object dataContract, object options, DataRow row,
                                                   CancellationToken token = default)
        {
            if (dataContract == null) return;
            SetModified(false, false);
            foreach (PropertyInfo pi in dataContract.GetType().GetProperties())
            {
                object val = pi.GetValue(dataContract, null);
                DataProperty dp = this[pi.Name];
                DataObject child;
                if (dp != null)
                {
                    dp.Modified = null;
                    await dp.SetValueAsync(val, row, token);
                }
                else if ((child = GetChildObject(pi.Name)) != null)
                {
                    await child.FromDataContractAsync(val, options, token);
                }
                else if (val != null)
                {
                    foreach (PropertyInfo cpi in pi.PropertyType.GetProperties())
                    {
                        DataProperty cdp = this[pi.Name + "_" + cpi.Name];
                        if (cdp != null)
                        {
                            cdp.Modified = null;
                            await cdp.SetValueAsync(cpi.GetValue(val, null), row, token);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Exports the data object property values and child object values
        /// to the given data contract object by setting all its properties
        /// to the values of the corresponding properties or child objects
        /// with the same names.
        /// If there is no exact match between some data contract property names
        /// and the data object property names, this method can be overridden
        /// in the subclass to address each such case.
        /// </summary>
        /// <param name="dataContract">The data contract object to export
        /// the current data object values to.</param>
        /// <param name="options">Additional options for the operation.</param>
        public virtual void ToDataContract(object dataContract, object options)
        {
            if (dataContract == null) return;
            ToDataContractProperties(dataContract, dataContract.GetType().GetProperties(), options, null);
        }

        /// <summary>
        /// Exports the data object property values and child object values
        /// to the given data contract type by setting all its properties
        /// to the values of the corresponding properties or child objects
        /// with the same names.
        /// If there is no exact match between some data contract property names
        /// and the data object property names, this method can be overridden
        /// in the subclass to address each such case.
        /// </summary>
        /// <param name="options">Additional options for the operation.</param>
        /// <returns>The data contract object populated with the current data object values.</returns>
        public virtual T ToDataContract<T>(object options)
        {
            T dataContract = (T)CreateInstance(typeof(T));
            ToDataContract(dataContract, options);
            return dataContract;
        }

        /// <summary>
        /// Exports the data object property values and child object values
        /// to the given data contract object by setting the specified properties
        /// of the data contract to the values of the corresponding properties
        /// or child objects with the same names.
        /// This method can be used to partially export data object values
        /// to a data contract object.
        /// </summary>
        /// <param name="dataContract">The data contract object to export
        /// the current data object values to.</param>
        /// <param name="props">The data contract object fields to set.</param>
        /// <param name="options">Additional options for the operation.</param>
        /// <param name="row">The data row context, if any.</param>
        protected void ToDataContractProperties(object dataContract, PropertyInfo[] props, object options, DataRow row = null)
        {
            if (dataContract == null) return;
            foreach (PropertyInfo pi in props)
            {
                DataProperty dp = this[pi.Name];
                if (dp != null)
                {
                    if (dp.IsValid(true, row))
                    {
                        if (dp.IsMultiValued)
                        {
                            IList lst = null;
                            if (dp.GetValue(ValueFormat.Transport, row) is IEnumerable valLst)
                            {
                                // create the right type of list and copy the values rather than directly assign
                                lst = CreateInstance(pi.PropertyType) as IList;
                                if (lst != null) foreach (object o in valLst) lst.Add(o);
                            }
                            pi.SetValue(dataContract, lst, null);
                        }
                        else pi.SetValue(dataContract, dp.GetValue(ValueFormat.Transport, row), null);
                    }
                    continue;
                }
                object obj;
                try { obj = CreateInstance(pi.PropertyType); }
                catch { continue; }

                DataObject child = GetChildObject(pi.Name);
                if (child != null) child.ToDataContract(obj, options);
                else
                {
                    foreach (PropertyInfo cpi in pi.PropertyType.GetProperties())
                    {
                        DataProperty cdp = this[pi.Name + "_" + cpi.Name];
                        if (cdp != null && cdp.IsValid(true, row)) cpi.SetValue(obj, cdp.GetValue(ValueFormat.Transport, row), null);
                    }
                }
                pi.SetValue(dataContract, obj, null);
            }
        }

        /// <summary>
        /// Create an instance of a certain type. If the type is IEnumerable
        /// then creates a corresponding generic list or dictionary as appropriate.
        /// </summary>
        /// <param name="type">The type to create an instance of.</param>
        /// <returns>An instance of the corresponding type.</returns>
        protected virtual object CreateInstance(Type type)
        {
            Type t = type;
            if (t.IsInterface && typeof(IEnumerable).IsAssignableFrom(t))
            {
                Type[] args = t.GetGenericArguments();
                if (args.Length > 1 && typeof(IDictionary).IsAssignableFrom(t))
                    t = typeof(Dictionary<,>).MakeGenericType(args[0], args[1]);
                else if (args.Length > 0) t = typeof(List<>).MakeGenericType(args[0]);
                else t = typeof(List<object>);
            }
            return Activator.CreateInstance(t);
        }

        #endregion

        #region NameValueCollection support

        /// <summary>
        /// Sets property values from NameValueCollection object.
        /// </summary>
        public virtual void SetValues(NameValueCollection nvc)
        {
            foreach (string p in nvc.Keys)
                if (HasProperty(p))
                    this[p].SetValue(nvc[p]);
        }

        /// <summary>
        /// Sets property values from NameValueCollection object asynchronously.
        /// </summary>
        public virtual async Task SetValuesAsync(NameValueCollection nvc, CancellationToken token)
        {
            foreach (string p in nvc.Keys)
                if (HasProperty(p))
                    await this[p].SetValueAsync(nvc[p], null, token);
        }

        /// <summary>
        /// Returns NameValueCollection object with property values.
        /// </summary>
        public virtual NameValueCollection ToNameValueCollection(bool includeNullValues = false)
        {
            NameValueCollection nvc = new NameValueCollection();
            foreach (DataProperty p in Properties)
            {
                if (p.IsNull() && !includeNullValues)
                    continue;

                nvc[p.Name] = p.EditStringValue;
            }
            return nvc;
        }
        #endregion

        #region Validation

        /// <summary>
        /// Constructs a new error list with injected resources from the current service provider.
        /// </summary>
        /// <returns>A new error list.</returns>
        public ErrorList NewErrorList() => new ErrorList(ServiceProvider?.GetService<ResourceManager>());

        /// <summary>
        /// A list of validation errors that are not tied to any particular
        /// data property but rather to the data object as a whole.
        /// Null value means that the object has not been validated yet.
        /// </summary>
        protected ErrorList validationErrorList;

        /// <summary>
        /// Gets all validation errors from the data object, all its properties and child objects recursively.
        /// </summary>
        /// <returns>Validation errors from the data object, all its properties and child objects.</returns>
        public virtual ErrorList GetValidationErrors()
        {
            ErrorList errLst = NewErrorList();
            if (validationErrorList != null) errLst.MergeWith(validationErrorList);
            foreach (DataProperty p in properties.Values) errLst.MergeWith(p.GetValidationErrors(null));
            foreach (DataObject obj in childObjects.Values) errLst.MergeWith(obj.GetValidationErrors());
            return errLst;
        }

        /// <summary>
        /// Resets validation status to not validated on the object
        /// by setting the validation error list to null.
        /// </summary>
        public void ResetValidation()
        {
            validationErrorList = null;
        }

        /// <summary>
        /// Resets validation status to not validated on the object,
        /// all its properties and child objects recursively.
        /// </summary>
        public virtual void ResetAllValidation()
        {
            ResetValidation();
            foreach (DataProperty p in properties.Values) p.ResetValidation(null);
            foreach (DataObject obj in childObjects.Values) obj.ResetAllValidation();
        }

        /// <summary>
        /// Validates the data object and all its properties and child objects recursively.
        /// </summary>
        /// <param name="force">True to validate regardless of whether or not it has been already validated.</param>
        public virtual void Validate(bool force)
        {
            foreach (DataProperty p in properties.Values) p.Validate(force, null);
            foreach (DataObject obj in childObjects.Values) obj.Validate(force);

            if (force) ResetValidation();
            if (validationErrorList != null) return;

            validationErrorList = NewErrorList();
        }

        #endregion

        #region Modification support

        /// <summary>
        /// A flag indicating if the object is tracking modifications
        /// </summary>
        public bool TrackModifications = true;

        /// <summary>
        /// Tracks the modification state of the data object.
        /// Null means the date object has never been initialized with data.
        /// False means the data object has been initialized, but has not been changed since then.
        /// True means that the data object has been modified since it was initialized.
        /// </summary>
        protected bool? modified;

        /// <summary>
        /// Gets or (non-recursively) sets modification state of the object.
        /// </summary>
        public bool Modified { get => IsModified() ?? false; set => SetModified(value, false); }

        /// <summary>
        /// Returns the modification state of the data object.
        /// </summary>
        /// <returns>The modification state of the data object.
        /// Null means the data object has never been initialized with data.
        /// False means the data object has been initialized, but has not been changed since then.
        /// True means that the data object has been modified since it was initialized.</returns>
        public virtual bool? IsModified()
        {
            bool? res = modified;
            foreach (DataProperty prop in properties.Values)
                if (prop.Modified.HasValue) res |= prop.Modified;
            foreach (DataObject child in childObjects.Values)
            {
                bool? childModified = child.IsModified();
                if (childModified.HasValue) res |= childModified;
            }
            return !TrackModifications && res != null ? false : res;
        }

        /// <summary>
        /// Sets the modification state for the data object to the specified value.
        /// </summary>
        /// <param name="modified">The modification state value.
        /// Null means the data object has never been initialized with data.
        /// False means the data object has been initialized, but has not been changed since then.
        /// True means that the data object has been modified since it was initialized.</param>
        /// <param name="recursive">True to propagate the modification state
        /// to all properties and child objects, false otherwise.</param>
        public virtual void SetModified(bool? modified, bool recursive)
        {
            this.modified = modified;
            if (recursive)
            {
                foreach (DataProperty prop in properties.Values) prop.Modified = modified;
                foreach (DataObject child in childObjects.Values) child.SetModified(modified, true);
            }
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(Modified)));
        }

        #endregion

        #region CRUD support

        /// <summary>
        /// Options for calling CRUD operations
        /// </summary>
        public class CrudOptions
        {
            /// <summary>
            /// Indicates if the operation should call child objects recursively.
            /// </summary>
            public bool Recursive = true;

            /// <summary>
            /// Indicates if the operation should stop on any errors.
            /// </summary>
            public bool AbortOnErrors = true;

            /// <summary>
            /// Indicates if the operation should call child objects in parallel.
            /// </summary>
            public bool Parallel = true;

            /// <summary>
            /// A flag indicating whether or not to preserve selection in data lists.
            /// </summary>
            public bool PreserveSelection = false;
        }

        private bool isNew = true;

        /// <summary>
        /// An indicator if the object is new and not yet saved.
        /// </summary>
        public bool IsNew
        {
            get { return isNew; }
            set
            {
                isNew = value;
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(IsNew)));
            }
        }

        /// <summary>
        /// Reads data for the data object.
        /// </summary>
        public virtual ErrorList Read(object options)
        {
            ErrorList msgList = DoRead(options) ?? NewErrorList();
            CrudOptions crudOpts = options as CrudOptions;
            if (crudOpts?.Recursive ?? true)
                foreach (DataObject child in childObjects.Values)
                {
                    if (crudOpts?.AbortOnErrors ?? true)
                        msgList.AbortIfHasErrors();
                    msgList.MergeWith(child.Read(options));
                }
            msgList.AbortIfHasErrors();
            IsNew = false;
            return msgList;
        }

        /// <summary>
        /// Actual implementation of reading object data provided by subclasses.
        /// </summary>
        protected virtual ErrorList DoRead(object options)
        {
            return null;
        }

        /// <summary>
        /// Reads data for the data object asynchronously.
        /// </summary>
        public virtual async Task<ErrorList> ReadAsync(object options, CancellationToken token = default)
        {
            List<Task<ErrorList>> readTasks = new List<Task<ErrorList>>
            {
                DoReadAsync(options, token)
            };
            CrudOptions crudOpts = options as CrudOptions;
            if (crudOpts?.Recursive ?? true)
                foreach (DataObject child in childObjects.Values)
                    readTasks.Add(child.ReadAsync(options, token));

            ErrorList msgList = NewErrorList();
            if (crudOpts?.Parallel ?? true)
            {
                await Task.WhenAll(readTasks.ToArray());
                foreach (var rt in readTasks)
                    msgList.MergeWith(rt.Result);
            }
            else
            {
                foreach (var rt in readTasks)
                {
                    if (crudOpts?.AbortOnErrors ?? true)
                        msgList.AbortIfHasErrors();
                    token.ThrowIfCancellationRequested();
                    await rt;
                    msgList.MergeWith(rt.Result);
                }
            }
            msgList.AbortIfHasErrors();
            IsNew = false;
            return msgList;
        }

        /// <summary>
        /// Actual implementation of reading object data asynchronously provided by subclasses.
        /// </summary>
        protected virtual async Task<ErrorList> DoReadAsync(object options, CancellationToken token = default)
        {
            return await Task.FromResult(DoRead(options));
        }

        /// <summary>
        /// Save action for the data object that can be bound to a Save button
        /// and control whether it is visible or enabled.
        /// </summary>
        public ActionProperty SaveAction { get; private set; }

        /// <summary>
        /// Saves the data object.
        /// </summary>
        public virtual ErrorList Save(object options)
        {
            Validate(true);
            ErrorList msgList = GetValidationErrors();
            msgList.AbortIfHasErrors();
            msgList.MergeWith(DoSave(options));
            if (!(options is CrudOptions crudOpts) || crudOpts.Recursive)
                foreach (DataObject child in childObjects.Values)
                {
                    msgList.AbortIfHasErrors();
                    msgList.MergeWith(child.Save(options));
                }
            msgList.AbortIfHasErrors();
            IsNew = false;
            SetModified(false, true);
            return msgList;
        }

        /// <summary>
        /// Actual implementation of saving the data object provided by subclasses.
        /// </summary>
        protected virtual ErrorList DoSave(object options)
        {
            return null;
        }

        /// <summary>
        /// Saves the data object asynchronously.
        /// </summary>
        public virtual async Task<ErrorList> SaveAsync(object options, CancellationToken token = default)
        {
            Validate(true);
            ErrorList msgList = GetValidationErrors();
            msgList.AbortIfHasErrors();

            List<Task<ErrorList>> saveTasks = new List<Task<ErrorList>>
            {
                DoSaveAsync(options, token)
            };
            CrudOptions crudOpts = options as CrudOptions;
            if (crudOpts?.Recursive ?? true)
                foreach (DataObject child in childObjects.Values)
                    saveTasks.Add(child.SaveAsync(options, token));

            if (crudOpts?.Parallel ?? false)
            {
                await Task.WhenAll(saveTasks.ToArray());
                foreach (var st in saveTasks)
                    msgList.MergeWith(st.Result);
            }
            else
            {
                foreach (var st in saveTasks)
                {
                    if (crudOpts?.AbortOnErrors ?? true)
                        msgList.AbortIfHasErrors();
                    token.ThrowIfCancellationRequested();
                    await st;
                    msgList.MergeWith(st.Result);
                }
            }
            msgList.AbortIfHasErrors();
            IsNew = false;
            SetModified(false, true);
            return msgList;
        }

        /// <summary>
        /// Asynchronous implementation of saving the data object provided by subclasses.
        /// </summary>
        protected virtual async Task<ErrorList> DoSaveAsync(object options, CancellationToken token = default)
        {
            return await Task.FromResult(DoSave(options));
        }

        /// <summary>
        /// Delete action for the data object that can be bound to a Delete button
        /// and control whether it is visible or enabled.
        /// </summary>
        public ActionProperty DeleteAction { get; private set; }

        /// <summary>
        /// Deletes the data object.
        /// </summary>
        public virtual ErrorList Delete(object options)
        {
            return DoDelete(options);
        }

        /// <summary>
        /// Actual implementation of deleting the data object provided by subclasses.
        /// </summary>
        protected virtual ErrorList DoDelete(object options)
        {
            return null;
        }

        /// <summary>
        /// Deletes the data object asynchronously.
        /// </summary>
        public virtual async Task<ErrorList> DeleteAsync(object options, CancellationToken token = default)
        {
            return await DoDeleteAsync(options, token);
        }

        /// <summary>
        /// Asynchronously implementation of deleting the data object provided by subclasses.
        /// </summary>
        protected virtual async Task<ErrorList> DoDeleteAsync(object options, CancellationToken token = default)
        {
            return await Task.FromResult(DoDelete(options));
        }

        #endregion
    }
}