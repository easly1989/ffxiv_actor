using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Reactive.Disposables;
using System.Reflection;
using System.Windows.Input;
using Actor.Core;
using ActorGui.ViewModels.Dialogs;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;

namespace ActorGui.ViewModels
{
    public class MainViewModel : DisposableViewModelBase
    {
        private readonly CommandLineResult _commandLineResult;

        private bool _isDialogOpen;
        private bool _dialogCanCancel;
        private string _installPath;
        private ViewModelBase _dialogContent;

        public string Title => $"ActorGui ~ v{Assembly.GetExecutingAssembly().GetName().Version}";
        public string ChangeInstallPathHint => "Change the install path for ACT";

        public bool IsDialogOpen
        {
            get => _isDialogOpen;
            set => Set(ref _isDialogOpen, value);
        }

        public bool DialogCanCancel
        {
            get => _dialogCanCancel;
            set => Set(ref _dialogCanCancel, value);
        }

        public ViewModelBase DialogContent
        {
            get => _dialogContent;
            set => Set(ref _dialogContent, value);
        }

        public ICommand ChangeInstallPathCommand { get; }
        public ObservableCollection<ComponentViewModel> Components { get; }

        public MainViewModel(CommandLineResult commandLineResult)
        {
            _commandLineResult = commandLineResult;

            ChangeInstallPathCommand = new RelayCommand(() => RequestChangeInstallPath("Insert a new install path for ACT", "Insert a valid path", true));
            Components = new ObservableCollection<ComponentViewModel>();

            _installPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ACT");
            if (SystemInteractions.IsValidPath(_commandLineResult.InstallPath))
            {
                _installPath = _commandLineResult.InstallPath;
            }
            else
            {
                if (!SystemInteractions.IsValidPath(_installPath))
                    RequestChangeInstallPath("The Current install path for ACT is not valid", "Insert a valid path");
            }

            ActConfigurationHelper.UpdateActInstallPath(_installPath);

            var downloadPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "download");
            if (!Directory.Exists(downloadPath))
                Directory.CreateDirectory(downloadPath);

            var systemInteractions = new SystemInteractions();
            var webInteractions = new WebInteractions();

            var components = webInteractions.LoadConfiguration();
            foreach (var component in components)
            {
                Components.Add(new ComponentViewModel(component, systemInteractions, _installPath));
            }
        }

        private void RequestChangeInstallPath(string message, string hint, bool canCancel = false)
        {
            var contentDisposable = new CompositeDisposable();
            var content = new RequestChangeInstallPathDialogViewModel(message, hint, _installPath, canCancel);

            contentDisposable.Add(content.WhenSaveRequested.Subscribe(installPath =>
            {
                _installPath = installPath;
                ActConfigurationHelper.UpdateActInstallPath(installPath);
                IsDialogOpen = false;
                contentDisposable.Dispose();
            }));

            contentDisposable.Add(content.WhenCancelRequested.Subscribe(_ =>
            {
                IsDialogOpen = false;
                contentDisposable.Dispose();
            }));

            DialogContent = content;
            DialogCanCancel = canCancel;
            IsDialogOpen = true;
        }
    }
}
