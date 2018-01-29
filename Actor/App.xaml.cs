using System.Windows;
using Actor.ViewModels;

namespace Actor
{
    public partial class App
    {
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            MainWindow = new MainWindow();
            var dataContext = new MainViewModel();
            MainWindow.DataContext = dataContext;
            MainWindow.Show();

            dataContext.Load();
        }
    }
}
