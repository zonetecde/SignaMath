using System;
using System.Windows;

namespace SignaMath
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show("Désolé, il y a eu une erreur!" + Environment.NewLine + e.Exception.Message + "\n" + e.Exception.StackTrace, "Erreur");
            e.Handled = true;
        }
    }
}
