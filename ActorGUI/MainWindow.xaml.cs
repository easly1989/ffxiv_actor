using ActorGUI.ViewModels;

namespace ActorGUI
{
    public partial class MainWindow
    {
        public MainWindow(string[] args)
        {
            InitializeComponent();
            DataContext = new MainViewModel(args);
        }
    }
}
