using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Threading;

namespace WPF_CV_RRHH
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Configurar manejadores de excepciones
            DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show($"Error grave: {e.Exception.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true; // Evita que la aplicación se cierre
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = (Exception)e.ExceptionObject;
            MessageBox.Show($"Error no manejado: {ex.Message}", "Error crítico", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

}
