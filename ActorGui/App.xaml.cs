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
        private MainViewModel _mainViewModel;
        private CompositeDisposable _disposables;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            _disposables = new CompositeDisposable();
            _mainViewModel = new MainViewModel();

            MainWindow = new MainWindow(_mainViewModel);

            _disposables.Add(Observable.FromEventPattern<CancelEventHandler, CancelEventArgs>(
                    handler => MainWindow.Closing += handler,
                    handler => MainWindow.Closing -= handler)
                .Select(result => result.EventArgs)
                .Subscribe(_ => OnMainWindowClosing()));

            _disposables.Add(Observable.FromEventPattern<UnhandledExceptionEventHandler, UnhandledExceptionEventArgs>(
                    handler => AppDomain.CurrentDomain.UnhandledException += handler,
                    handler => AppDomain.CurrentDomain.UnhandledException -= handler)
                .Select(result => result.EventArgs)
                .Subscribe(OnUnhandledException));

            MainWindow?.Show();
        }

        private void OnUnhandledException(UnhandledExceptionEventArgs args)
        {
            // todo: Handle exceptions
        }

        private void OnMainWindowClosing()
        {
            _mainViewModel.Dispose();
            _disposables.Dispose();
        }
    }
}
