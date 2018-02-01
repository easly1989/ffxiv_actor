namespace Actor.Core
{
    /// <summary>
    /// This class represents any of the component we need to download and install.
    /// </summary>
    public class Component
    {
        public int InstallationOrder { get; set; }
        public string Url { get; set; }
        public string FileName { get; set; }
        public string Name { get; set; }
        public string Version { get; set; }
        public ComponentType ComponentType { get; set; }
        public bool IsPlugin { get; set; }
        public bool IsFromGitHub { get; set; }
        public bool IsPrerequisite { get; set; }
        public string InstallArguments { get; set; }
    }
}
