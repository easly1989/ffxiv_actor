namespace Actor.Core
{
    public class CommandLineResult
    {
        public string InstallPath { get; }
        public CommandLineSwitches Switch { get; }

        public CommandLineResult(string installPath, CommandLineSwitches s)
        {
            InstallPath = installPath;
            Switch = s;
        }
    }
}