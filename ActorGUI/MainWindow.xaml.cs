using System.Reflection;
using ActorGUI.ViewModels;
using ReactiveUI;

namespace ActorGUI
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            var version = Assembly.GetExecutingAssembly().GetName().Version;
            ViewModel = new MainViewModel($"~ ActorGUI v{version}");

            // todo: handle binding with mainWindow
            this.WhenActivated(disposableRegistration =>
            {

            });
        }
    }
}
