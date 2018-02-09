using System.Collections.Generic;

namespace Actor.Core
{
    /// <summary>
    /// This class represents any of the component we need to download and install.
    /// </summary>
    public class Component
    {
        public int InstallOrder { get; }
        public string Url { get; }
        public string FileName { get; }
        public string Name { get; }
        public string Version { get; }
        public string VersionCheck { get; }
        public ComponentType ComponentType { get; }
        public bool IsPlugin { get; }
        public bool IsFromGitHub { get; }
        public bool IsPrerequisite { get; }
        public bool CanBeSkipped { get; }
        public string InstallArguments { get; }
        public string Win7InstallArguments { get; }
        public string[] Libraries { get; }
        public Dictionary<string, string> Configurations { get; }

        public Component(int installOrder, string url, string fileName, string name, string version, ComponentType componentType, bool isPlugin, bool isFromGitHub, bool isPrerequisite, string installArguments, bool canBeSkipped, string versionCheck, Dictionary<string, string> configurations, string[] libraries, string win7InstallArguments)
        {
            InstallOrder = installOrder;
            Url = url;
            FileName = fileName;
            Name = name;
            Version = version;
            ComponentType = componentType;
            IsPlugin = isPlugin;
            IsFromGitHub = isFromGitHub;
            IsPrerequisite = isPrerequisite;
            InstallArguments = installArguments;
            CanBeSkipped = canBeSkipped;
            VersionCheck = versionCheck;
            Configurations = configurations;
            Libraries = libraries;
            Win7InstallArguments = win7InstallArguments;
        }
    }
}
