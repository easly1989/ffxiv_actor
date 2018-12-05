using System.Reflection;
using ActorGUI.ViewModels;

namespace ActorGUI
{
    public partial class MainWindow
    {
        public MainWindow(string[] args)
        {
            InitializeComponent();

            var version = Assembly.GetExecutingAssembly().GetName().Version;
            WindowTitle.Text = $"~ ActorGUI v{version}";

            DataContext = new MainViewModel(args);
        }
    }
}
