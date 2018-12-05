using System.Windows;

namespace ActorGUI
{
    public partial class App
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            MainWindow = new MainWindow(e.Args);
            MainWindow.Show();
        }
    }
}
