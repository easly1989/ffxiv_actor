using System;

namespace Actor.Core
{
    /// <summary>
    /// This static class handles (in a vertical/hardcoded way) all the possible command
    /// line parameters any Actor application can receive.
    /// </summary>
    public static class CommandLineParametersHelper
    {
        /// <summary>
        /// Given the args, returns the install path and informations about the application
        /// automatic execution
        /// </summary>
        /// <param name="args">The arguments given through the command line</param>
        /// <returns>The CommandLineResult with informations about install path and autmatic execution</returns>
        public static CommandLineResult EvaluateArgs(string[] args)
        {
            var installPath = string.Empty;
            var cmd = CommandLineSwitches.UserInput;
            foreach (var arg in args)
            {
                if (arg.StartsWith("/path", StringComparison.InvariantCultureIgnoreCase))
                {
                    var split = arg.Split('=');
                    if (split.Length != 2)
                        return null;

                    installPath = split[1];
                    continue; 
                }

                if(arg.Equals("/y", StringComparison.InvariantCultureIgnoreCase))
                    cmd = CommandLineSwitches.SilentInstallAll;
                else if (arg.Equals("/n", StringComparison.InvariantCultureIgnoreCase))
                    cmd = CommandLineSwitches.SilentInstallNone;
            }

            return new CommandLineResult(installPath, cmd);
        }
    }
}
