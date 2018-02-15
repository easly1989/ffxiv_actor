using System.Windows;
using ActorGui.ViewModels;

namespace ActorGui
{
    public partial class MainWindow
    {
        public MainWindow(MainViewModel mainViewModel)
        {
            InitializeComponent();

            Height = MinHeight = 450;
            Width = MinWidth = 600;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            Title = mainViewModel.Title;

            DataContext = mainViewModel;
        }
    }
}
