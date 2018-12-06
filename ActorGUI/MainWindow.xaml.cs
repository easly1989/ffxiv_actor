using System.Reflection;
using ActorGUI.Localization;
using ActorGUI.ViewModels;

namespace ActorGUI
{
    public partial class MainWindow
    {
        public MainWindow(string[] args)
        {
            InitializeComponent();

            var version = Assembly.GetExecutingAssembly().GetName().Version;
            WindowTitle.Text = string.Format(Locals.MainWindow_Title, version);

            DataContext = new MainViewModel(args);
        }
    }
}
