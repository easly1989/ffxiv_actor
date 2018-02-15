using System.Windows;

namespace ActorGui
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            Height = MinHeight = 450;
            Width = MinWidth = 600;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            Title = "ActorGui"; // todo: add a localization service
        }
    }
}
