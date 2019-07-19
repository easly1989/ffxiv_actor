using System.Threading;
using System.Windows;
using Actor.UI.Common;
using ActorWizard.ViewModels;

namespace ActorWizard
{
    public partial class App
    {
        private MainViewModel _mainViewModel;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            ViewModelBase.UIContext = SynchronizationContext.Current;

            _mainViewModel = new MainViewModel();

            MainWindow = new MainWindow { DataContext = _mainViewModel };
            MainWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            _mainViewModel.Dispose();
        }
    }
}
