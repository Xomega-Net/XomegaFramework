// Copyright (c) 2020 Xomega.Net. All rights reserved.

using System.Windows;
using Xomega.Framework.Binding;

namespace Xomega.Framework
{
    /// <summary>
    /// A base class for providing bindings between data objects and various WPF elements.
    /// A data object binding is responsible for making sure that the state of the WPF element
    /// is in sync with the state of the underlying data object. Data object bindings
    /// are attached to the framework elements via a WPF dependency property
    /// (see <see cref="Data.BindingProperty"/>).
    /// </summary>
    public class DataObjectBinding : BaseBinding
    {
        #region Static registration

        /// <summary>
        /// A static constructor that registers Xomega framework data object bindings.
        /// </summary>
        static DataObjectBinding()
        {
            Register();
            ListViewBinding.Register();
        }

        /// <summary>
        ///  A static catch-all method to register DataPropertyBinding for all bindable WPF dependency objects.
        /// </summary>
        private static void Register()
        {
            Register(typeof(FrameworkElement), delegate (object obj)
            {
                DependencyObject dobj = obj as DependencyObject;
                return IsBindable(dobj) ? new DataObjectBinding(dobj) : null;
            });
        }

        /// <summary>
        /// Checks if a dependency object is data object bindable.
        /// Serves mainly as a trigger for static class registration.
        /// </summary>
        /// <param name="obj">Dependency object to check.</param>
        /// <returns>Whether or not the dependency object is data object bindable.</returns>
        public static bool IsBindable(DependencyObject obj)
        {
            return obj != null && Data.GetObjectBinding(obj) != null;
        }

        #endregion

        /// <summary>
        /// The data object that the framework element / panel is bound to.
        /// </summary>
        protected DataObject context;

        /// <summary>
        /// The framework element that is bound to the data property.
        /// </summary>
        protected DependencyObject element;

        /// <summary>
        /// Constructs a base data object binding for the given framework element.
        /// </summary>
        /// <param name="fwkElement">The framework element to be bound to a supplied data object.</param>
        protected DataObjectBinding(DependencyObject fwkElement)
        {
            element = fwkElement;

            OnDataContextChanged(element, new DependencyPropertyChangedEventArgs());
            FrameworkElement el = fwkElement as FrameworkElement;
            if (el != null)
            {
                el.DataContextChanged += OnDataContextChanged;
            }
        }

        /// <summary>
        /// A handler of the data context change that finds the data property in the context data object
        /// and binds the framework element to it.
        /// </summary>
        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            DependencyObject element = sender as DependencyObject;
            if (element == null) return;
            DataObject obj = element.GetValue(FrameworkElement.DataContextProperty) as DataObject;
            BindTo(obj);
        }

        /// <summary>
        /// Binds the framework element to the given data object.
        /// </summary>
        /// <param name="obj">The data object to bind the framework element to.</param>
        public virtual void BindTo(DataObject obj)
        {
            this.context = obj;
        }

        /// <summary>
        /// Reset data context by default
        /// </summary>
        public override void Dispose()
        {
            FrameworkElement el = element as FrameworkElement;
            if (el != null)
            {
                el.DataContextChanged -= OnDataContextChanged;
            }
            BindTo(null);
            element = null;
            base.Dispose();
        }
    }
}
