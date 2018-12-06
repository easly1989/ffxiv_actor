using System;
using System.Threading.Tasks;
using Actor.Core;

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

        protected ComponentViewModelBase(
            Component component, 
            SystemInteractions systemInteractions,
            string installPath = "")
        {
            Component = component;
            ComponentPath = installPath;
            SystemInteractions = systemInteractions;
            
#pragma warning disable 4014
            CheckVersion(new Progress<Tuple<bool, string>>(VersionUpdate));
#pragma warning restore 4014
        }

        protected void VersionUpdate(Tuple<bool, string> tuple)
        {
            InstalledVersion = tuple.Item2;
            VersionCheck = !tuple.Item1;

            RaisePropertyChanged(() => VersionCheck);
            RaisePropertyChanged(() => InstalledVersion);
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