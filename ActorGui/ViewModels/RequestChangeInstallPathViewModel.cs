using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows.Input;
using Actor.Core;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using MaterialDesignThemes.Wpf;

namespace ActorGui.ViewModels
{
    public class RequestChangeInstallPathViewModel : ViewModelBase
    {
        private readonly ISubject<string> _savedSubject;
        private readonly ISubject<Unit> _cancelSubject;

        private string _installPath;

        public IObservable<string> WhenInstallPathSaved => _savedSubject.AsObservable();
        public IObservable<Unit> WhenCancelRequested => _cancelSubject.AsObservable();

        public bool CanCancel { get; }
        public string Message { get; }
        public string InstallPathHint { get; }
        public ICommand SaveCommnad { get; }
        public ICommand CancelCommand { get; }

        public string InstallPath
        {
            get => _installPath;
            set => Set(ref _installPath, value);
        }

        public RequestChangeInstallPathViewModel(string message, string hint, string installPath, bool canCancel = false)
        {
            _savedSubject = new Subject<string>();
            _cancelSubject = new Subject<Unit>();
            _installPath = installPath;

            CanCancel = canCancel;
            Message = message;
            InstallPathHint = hint;

            SaveCommnad = new RelayCommand(
                () => _savedSubject.OnNext(_installPath),
                () => SystemInteractions.IsValidPath(_installPath));

            CancelCommand = new RelayCommand(
                () => _cancelSubject.OnNext(Unit.Default));
        }
    }
}