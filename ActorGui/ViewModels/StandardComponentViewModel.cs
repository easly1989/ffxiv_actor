using System;
using System.IO;
using Actor.Core;

namespace ActorGui.ViewModels
{
    public class StandardComponentViewModel : ComponentViewModelBase
    {
        public StandardComponentViewModel(
            Component component, 
            SystemInteractions systemInteractions, 
            string installPath) 
            : base(component, systemInteractions, Path.Combine(installPath, component.VersionCheck))
        {
        }

        public void UpdateInstallPath(string installPath)
        {
            ComponentPath = Path.Combine(installPath, Component.VersionCheck);
#pragma warning disable 4014
            CheckVersion(new Progress<Tuple<bool, string>>(VersionUpdate));
#pragma warning restore 4014
        }
    }
}