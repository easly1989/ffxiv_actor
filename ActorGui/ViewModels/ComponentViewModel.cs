using System;
using System.IO;
using System.Threading.Tasks;
using Actor.Core;

namespace ActorGui.ViewModels
{
    public class ComponentViewModel : DisposableViewModelBase
    {
        private readonly Component _component;
        private readonly SystemInteractions _systemInteractions;

        private bool _versionCheck;
        private string _installPath;
        private string _componentPath;
        private string _installedVersion;

        public bool VersionCheck => _versionCheck;
        public string Name => _component.Name;
        public string LatestVersion => _component.Version;
        public string InstalledVersion => _installedVersion;

        public ComponentViewModel(Component component, 
                                  SystemInteractions systemInteractions,
                                  string installPath = "")
        {
            _component = component;
            _systemInteractions = systemInteractions;
            _installPath = installPath;

            _componentPath = _component.IsPrerequisite ? _component.VersionCheck : Path.Combine(installPath, _component.VersionCheck);

            CheckVersion(new Progress<Tuple<bool, string>>(VersionUpdate));
        }

        private void VersionUpdate(Tuple<bool, string> tuple)
        {
            _installedVersion = tuple.Item2;
            _versionCheck = !tuple.Item1;

            RaisePropertyChanged(() => VersionCheck);
            RaisePropertyChanged(() => InstalledVersion);
        }

        private async Task CheckVersion(IProgress<Tuple<bool, string>> progress)
        {
            await Task.Run(() =>
            {
                var result = _systemInteractions.CheckVersion(_componentPath, LatestVersion);
                progress.Report(result);
            });
        }

        public void UpdateInstallPath(string installPath)
        {
            _installPath = installPath;
            _componentPath = _component.IsPrerequisite ? _component.VersionCheck : Path.Combine(installPath, _component.VersionCheck);

            RaisePropertyChanged(() => VersionCheck);
        }
    }
}