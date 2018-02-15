using System;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using ActorGui.ViewModels;

namespace ActorGui
{
    public partial class App
    {
        private IDisposable _disposable;
        private MainViewModel _mainViewModel;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            _mainViewModel = new MainViewModel();

            MainWindow = new MainWindow(_mainViewModel);

            _disposable = Observable.FromEventPattern<UnhandledExceptionEventHandler, UnhandledExceptionEventArgs>(
                    handler => AppDomain.CurrentDomain.UnhandledException += handler,
                    handler => AppDomain.CurrentDomain.UnhandledException -= handler)
                .Select(result => result.EventArgs)
                .Subscribe(OnUnhandledException);

            MainWindow?.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            _mainViewModel.Dispose();
            _disposable.Dispose();
        }
        
        private void OnUnhandledException(UnhandledExceptionEventArgs args)
        {
            // todo: Handle exceptions
        }
    }
}
