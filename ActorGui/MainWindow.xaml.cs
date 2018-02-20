using System.Windows;
using ActorGui.ViewModels;

namespace ActorGui
{
    public partial class MainWindow
    {
        public MainWindow(MainViewModel mainViewModel)
        {
            InitializeComponent();

            Height = MinHeight = 600;
            Width = MinWidth = 700;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            Title = mainViewModel.Title;

            DataContext = mainViewModel;
        }
    }
}
