using Actor.Core;

namespace ActorGui.ViewModels.Dialogs
{
    public class RequestChangeInstallPathDialogViewModel : DialogViewModelBase<string>
    {
        private string _installPath;

        public string InstallPathHint { get; }

        public string InstallPath
        {
            get => _installPath;
            set => Set(ref _installPath, value);
        }

        public RequestChangeInstallPathDialogViewModel(string message, string hint, string installPath, bool canCancel = false)
            : base(message, showCancel: canCancel)
        {
            _installPath = installPath;
            InstallPathHint = hint;
        }

        protected override string OnSave()
        {
            // This is the only place where IsValidPath should also create the directory if needed!
            SystemInteractions.IsValidPath(_installPath);
            return _installPath;
        }

        protected override bool OnCanSave()
        {
            return SystemInteractions.IsValidPath(_installPath, false);
        }
    }
}