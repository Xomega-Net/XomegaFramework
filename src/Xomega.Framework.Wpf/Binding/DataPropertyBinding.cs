// Copyright (c) 2021 Xomega.Net. All rights reserved.

using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using Xomega.Framework.Binding;

namespace Xomega.Framework
{
    /// <summary>
    /// A base class for providing bindings between data properties and various WPF elements.
    /// A data property binding is responsible for making sure that the state of the WPF element
    /// is in sync with the state of the underlying data property. Data property bindings
    /// are attached to the framework elements via a WPF dependency property
    /// (see <see cref="Property.BindingProperty"/>). They are constructed with just a property name
    /// based on the value of another dependency property (see <see cref="Property.NameProperty"/>)
    /// that can be statically specified in the XAML. Once a data object is set as a data context
    /// for the framework element, a data property with the given name will be retrieved from the 
    /// data object and the element will be bound to that property.
    /// Data property bindings are created via a factory design pattern. A <c>PropertyBindingCreator</c>
    /// callback can be registered for any particular type of framework elements. If no binding
    /// is registered for a given type, the one for the  base type will be used.
    /// </summary>
    public class DataPropertyBinding : BasePropertyBinding
    {
        #region Static registration

        /// <summary>
        /// A static constructor that registers Xomega framework data property bindings.
        /// </summary>
        static DataPropertyBinding()
        {
            Register();
            TextBlockPropertyBinding.Register();
            TextBoxPropertyBinding.Register();
            ToggleButtonPropertyBinding.Register();
            SelectorPropertyBinding.Register();
            LinkPropertyBinding.Register();
            DatePickerPropertyBinding.Register();
            PasswordBoxPropertyBinding.Register();
        }

        /// <summary>
        ///  A static catch-all method to register DataPropertyBinding for all bind-able WPF dependency objects.
        /// </summary>
        private static void Register()
        {
            Register(typeof(TextBlock), delegate(object obj)
            {
                DependencyObject dobj = obj as DependencyObject;
                return IsBindable(dobj) ? new DataPropertyBinding(dobj) : null;
            });
        }

        /// <summary>
        /// Checks if a dependency object is property bind-able by checking its 
        /// dependency property for the property name (see <see cref="Property.NameProperty"/>).
        /// </summary>
        /// <param name="obj">Dependency object to check.</param>
        /// <returns>Whether or not the dependency object is property bind-able.</returns>
        public static bool IsBindable(DependencyObject obj)
        {
            return obj != null && Property.GetName(obj) != null;
        }

        /// <summary>
        /// Recursively removes and disposes property bindings from the specified element and its children
        /// </summary>
        /// <param name="el">Element to remove bindings from</param>
        public static void RemoveBindings(DependencyObject el)
        {
            if (el == null) return;
            if (el.GetValue(Property.BindingProperty) is IDisposable binding)
            {
                binding.Dispose();
                el.SetValue(Property.BindingProperty, null);
            }
            if (el is Visual || el is Visual3D)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(el); i++)
                    RemoveBindings(VisualTreeHelper.GetChild(el, i));
            }
            InlineCollection inlines = null;
            if (el is TextBlock) inlines = ((TextBlock)el).Inlines;
            else if (el is Span) inlines = ((Span)el).Inlines;
            if (inlines != null)
                foreach (Inline il in inlines) RemoveBindings(il);
        }
        #endregion

        /// <summary>
        /// The framework element that is bound to the data property.
        /// </summary>
        protected DependencyObject element;

        /// <summary>
        /// Constructs a base data property binding for the given framework element and the property name.
        /// </summary>
        /// <param name="fwkElement">The framework element to be bound to a property with the specified name.</param>
        protected DataPropertyBinding(DependencyObject fwkElement)
        {
            element = fwkElement;

            OnDataContextChanged(element, new DependencyPropertyChangedEventArgs());
            if (fwkElement is FrameworkElement el)
            {
                el.DataContextChanged += OnDataContextChanged;
                el.LostFocus += OnLostFocus;

                // Ideally we want to dispose the binding when the control is disposed,
                // but we cannot rely on the Unloaded event, as it breaks for tab control, for example.
                // Assuming lifecycle of the data objects is the same as that of the controls, it's not a big deal
                //el.Unloaded += delegate { Dispose(); }
            }
        }

        /// <inheritdoc/>
        public override void BindTo(DataProperty property, DataRow row)
        {
            base.BindTo(property, row);
            if (property == null)
            {
                element.SetValue(Property.ValidationProperty, null);
                BindingOperations.ClearBinding(element, ValidationExpressionProperty);
            }
        }

        /// <inheritdoc/>
        /// Overridden to update elements in a thread-safe manner.
        protected override void OnPropertyChange(object sender, PropertyChangeEventArgs e)
        {
            if (property == sender && !PreventElementUpdate)
            {
                element.Dispatcher.InvokeAsync(async () =>
                {
                    bool b = PreventModelUpdate;
                    PreventModelUpdate = true;
                    await UpdateElementAsync(e.Change);
                    PreventModelUpdate = b;
                });
            }
        }

        /// <summary>
        /// Remove any listeners when disposing
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
            if (element is FrameworkElement el)
            {
                el.DataContextChanged -= OnDataContextChanged;
                el.LostFocus -= OnLostFocus;
            }
        }

        /// <summary>
        /// A handler of the data context change that finds the data property in the context data object
        /// and binds the framework element to it.
        /// </summary>
        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!(sender is DependencyObject element)) return;
            var ctx = element.GetValue(FrameworkElement.DataContextProperty);
            DataRow dr = ctx as DataRow;
            DataObject obj = (dr != null) ? dr.List : 
                FindChildObject(ctx as DataObject, Property.GetChildObject(element));
            string propertyName = Property.GetName(element);
            DataProperty dp = null;
            if (obj != null && propertyName != null)
                dp = obj[propertyName];
            element.Dispatcher.Invoke(() => BindTo(dp, dr));
        }

        private void OnLostFocus(object sender, RoutedEventArgs e)
        {
            if (property != null) property.Editing = false;
        }

        /// <summary>
        /// Asynchronously updates the framework element based on the given property change.
        /// Invokes the synchronous version by default, but subclasses can override this method.
        /// </summary>
        /// <param name="change">The property change.</param>
        protected virtual async Task UpdateElementAsync(PropertyChange change)
        {
            UpdateElement(change);
            await Task.CompletedTask;
        }

        /// <summary>
        /// Updates editability of the element based on editability of the property.
        /// Default behavior just disables the control, but subclasses can make it read-only instead
        /// or handle it in a different way.
        /// </summary>
        protected override void UpdateEditability()
        {
            element.SetValue(UIElement.IsEnabledProperty, property.Editable);
        }

        /// <summary>
        /// Updates visibility of the element based on the visibility of the property.
        /// Default behavior sets the element visibility to Collapsed if the property is not visible
        /// and updates the label visibility, but subclasses can handle it in a different way.
        /// </summary>
        protected override void UpdateVisibility()
        {
            Visibility vis = property.Visible ? Visibility.Visible : Visibility.Collapsed;
            element.SetValue(UIElement.VisibilityProperty, vis);
            if (element.GetValue(Property.LabelProperty) is UIElement lbl && 
                BindingOperations.GetBinding(lbl, UIElement.VisibilityProperty) == null)
                lbl.Visibility = vis;
        }

        /// <summary>
        /// Updates a Required dependency property of the element and the label (if any)
        /// based on the Required flag of the data property. Subclasses can handle it in a different way.
        /// </summary>
        /// <seealso cref="Property.RequiredProperty"/>
        protected override void UpdateRequired()
        {
            element.SetValue(Property.RequiredProperty, property.Required);
            if (element.GetValue(Property.LabelProperty) is DependencyObject lbl)
                lbl.SetValue(Property.RequiredProperty, property.Required);
        }

        /// <summary>
        /// Updates WPF validation status of the element based on the validation status
        /// of the data property. The default implementation adds a single validation error
        /// to the element with a combined error text from all property validation errors.
        /// </summary>
        protected override void UpdateValidation()
        {
            ErrorList errors = property?.GetValidationErrors(null);
            BindingExpression exp = GetValidationExpression();
            element.SetValue(Property.ValidationProperty, new Property.ValidationResults() { Errors = errors });
            Validation.ClearInvalid(exp);
            if (errors != null && errors.Errors.Count > 0 && property.Visible && property.Editable)
                Validation.MarkInvalid(exp, new ValidationError(new DataErrorValidationRule(), exp.ParentBindingBase, errors.ErrorsText, null)); 
        }

        /// <summary>
        /// A dummy attached property for the <see cref="GetValidationExpression"/> method.
        /// </summary>
        protected static readonly DependencyProperty ValidationExpressionProperty = DependencyProperty.RegisterAttached(
            "ValidationExpression", typeof(string), typeof(DataPropertyBinding), new PropertyMetadata(null));

        /// <summary>
        /// Creates a dummy binding expression that can be used to add or clear WPF validation errors.
        /// </summary>
        /// <returns>A dummy binding expression to support WPF validation.</returns>
        protected BindingExpression GetValidationExpression()
        {
            // get a dummy binding expression so that we could use it for WPF validation framework
            BindingExpression be = !(element is FrameworkElement fwkEl) ? null : fwkEl.GetBindingExpression(ValidationExpressionProperty);
            if (be != null) return be;
            System.Windows.Data.Binding bnd = new System.Windows.Data.Binding("Errors.ErrorsText")
            {
                Mode = BindingMode.TwoWay,
                NotifyOnValidationError = true,
                ValidatesOnExceptions = true,
                Source = new Property.ValidationResults()
            };
            return (BindingExpression)BindingOperations.SetBinding(element, ValidationExpressionProperty, bnd);
        }

        /// <summary>
        /// Associates the current framework element with the label that is stored in the element's
        /// dependency property <see cref="Property.LabelProperty"/>, which can be statically set in XAML.
        /// Default implementation sets the current element as the target for the label and also sets
        /// the label text on the data property from the corresponding label control if not already set.
        /// </summary>
        protected override void SetLabel()
        {
            object lbl = element.GetValue(Property.LabelProperty);
            // if it is a label, set its target to this element unless it's already set
            if (lbl is Label elLbl && elLbl.Target == null && 
                BindingOperations.GetBinding(elLbl, Label.TargetProperty) == null)
                elLbl.Target = element as UIElement;

            // set property label if it is not already set and if associated label is present
            string lblTxt = null;
            if (lbl is ContentControl) lblTxt = "" + ((ContentControl)lbl).Content;
            else if (lbl is TextBlock) lblTxt = ((TextBlock)lbl).Text;
            SetPropertyLabel(lblTxt);
        }
    }
}
