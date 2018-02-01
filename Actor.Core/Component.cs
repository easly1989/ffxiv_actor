namespace Actor.Core
{
    /// <summary>
    /// This class represents any of the component we need to download and install.
    /// </summary>
    public class Component
    {
        public int InstallationOrder { get; }
        public string Url { get; }
        public string FileName { get; }
        public string Name { get; }
        public string Version { get; }
        public ComponentType ComponentType { get; }
        public bool IsPlugin { get; }
        public bool IsFromGitHub { get; }
        public bool IsPrerequisite { get; }
        public string InstallArguments { get; }

        public Component(int installationOrder, string url, string fileName, string name, string version, ComponentType componentType, bool isPlugin, bool isFromGitHub, bool isPrerequisite, string installArguments)
        {
            InstallationOrder = installationOrder;
            Url = url;
            FileName = fileName;
            Name = name;
            Version = version;
            ComponentType = componentType;
            IsPlugin = isPlugin;
            IsFromGitHub = isFromGitHub;
            IsPrerequisite = isPrerequisite;
            InstallArguments = installArguments;
        }
    }
}
