using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Actor.Core;
using GalaSoft.MvvmLight.CommandWpf;

namespace ActorGui.ViewModels
{
    public abstract class ComponentViewModelBase : DisposableViewModelBase
    {
        protected readonly Component Component;
        protected readonly SystemInteractions SystemInteractions;

        protected string ComponentPath;

        public bool VersionCheck { get; private set; }
        public string InstalledVersion { get; private set; }

        public string Name => Component.Name;
        public string LatestVersion => Component.Version;
        public string InstallOrUpdateText => string.IsNullOrWhiteSpace(InstalledVersion) || InstalledVersion.Equals("0.0.0.0") ? "Install" : "Update";

        public ICommand InstallOrUpdateCommand { get; }

        protected ComponentViewModelBase(
            Component component, 
            SystemInteractions systemInteractions,
            string installPath = "")
        {
            Component = component;
            ComponentPath = installPath;
            SystemInteractions = systemInteractions;

            InstallOrUpdateCommand = new RelayCommand(InstallOrUpdate, CanInstallOrUpdate);
            
#pragma warning disable 4014
            CheckVersion(new Progress<Tuple<bool, string>>(VersionUpdate));
#pragma warning restore 4014
        }

        private bool CanInstallOrUpdate()
        {
            return VersionCheck;
        }

        private void InstallOrUpdate()
        {
            // todo
        }

        protected void VersionUpdate(Tuple<bool, string> tuple)
        {
            InstalledVersion = tuple.Item2;
            VersionCheck = !tuple.Item1;

            RaisePropertyChanged(() => VersionCheck);
            RaisePropertyChanged(() => InstalledVersion);
            RaisePropertyChanged(() => InstallOrUpdateText);
        }

        protected async Task CheckVersion(IProgress<Tuple<bool, string>> progress)
        {
            await Task.Run(() =>
            {
                var result = SystemInteractions.CheckVersion(ComponentPath, LatestVersion);
                progress.Report(result);
            });
        }
    }
}