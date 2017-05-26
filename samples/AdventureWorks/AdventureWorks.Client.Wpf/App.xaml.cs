using AdventureWorks.Client.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Threading;
using Xomega.Framework;
using Xomega.Framework.Views;

namespace AdventureWorks.Client.Wpf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            AppInitializer.Initalize(new WpfAppInit());

            LoginView vwLogin = DI.DefaultServiceProvider.GetService<LoginView>();
            NameValueCollection parameters = new NameValueCollection();
            parameters.Add(ViewParams.Mode.Param, ViewParams.Mode.Popup);
            ViewModel.NavigateTo(DI.DefaultServiceProvider.GetService<LoginViewModel>(), vwLogin, parameters, null, null);
        }

        private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            ErrorParser errorParser = DI.DefaultServiceProvider.GetService<ErrorParser>();
            if (errorParser == null) errorParser = new ErrorParser();

            IErrorPresenter errorPresenter = DI.DefaultServiceProvider.GetService<IErrorPresenter>();
            if (errorPresenter != null)
                errorPresenter.Show(errorParser.FromException(e.Exception));
        }
    }
}
