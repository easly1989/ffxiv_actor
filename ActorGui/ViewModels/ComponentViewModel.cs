namespace ActorGui.ViewModels
{
    public class ComponentViewModel : DisposableViewModelBase
    {
        private string _installedVersion;

        public string Name { get; }
        public string LatestVersion { get; }

        public string InstalledVersion => _installedVersion;

        public bool VersionCheck => true; //todo SystemInteractions.CheckVersion() needs the component
    }
}