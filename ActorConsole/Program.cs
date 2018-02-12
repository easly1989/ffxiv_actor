using System;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reflection;
using Actor.Core;

namespace ActorConsole
{
    internal class Program
    {
        private const string DefaultIterationErrorMessage = "##### You should answer just with 'y' or 'n'...";

        private static void Main(string[] args)
        {
            var cmdResult = CommandLineParametersHelper.EvaluateArgs(args);
            var hasInstallPath = !string.IsNullOrWhiteSpace(cmdResult.InstallPath);
            var switches = cmdResult.Switch;

            var version = Assembly.GetExecutingAssembly().GetName().Version;
            var downloadPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "download");

            var installPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ACT");
            if (hasInstallPath && SystemInteractions.IsValidPath(cmdResult.InstallPath))
                installPath = cmdResult.InstallPath;

            Console.WriteLine($"##### ~ ActorConsole v{version}");
            Console.WriteLine($"##### Going to install ACT in '{installPath}'");

            if (switches == CommandLineSwitches.UserInput)
            {
                if (Iterate(_ => YesOrNoIteration("n"), CommandLineSwitches.UserInput, "##### Would you like to change it?' [y/N] ", DefaultIterationErrorMessage))
                {
                    Iterate(__ =>
                    {
                        installPath = Console.ReadLine();
                        return SystemInteractions.IsValidPath(installPath);
                    }, CommandLineSwitches.UserInput, "##### Write the path you prefer: ", "##### The path inserted is not valid...");
                }
            }

            ActConfigurationHelper.UpdateActInstallPath(installPath);

            Console.WriteLine("##### To ensure that ACT works correctly you should first install:");
            Console.WriteLine("#####   1. Microsoft Visual C++ Redistributable");
            Console.WriteLine("#####   2. Microsoft .NET Framework 4.7");
            Console.WriteLine("#####   3. Win10Pcap");
            Console.WriteLine("##### If you have already installed then you can skip this step.");

            if (!Directory.Exists(downloadPath))
                Directory.CreateDirectory(downloadPath);

            var systemInteractions = new SystemInteractions();
            var webInteractions = new WebInteractions();

            systemInteractions.KillProcess("Advanced Combat Tracker");

            var components = webInteractions.LoadConfiguration(() =>
            {
                Console.WriteLine("##### An error occurred when reading the configuration file.\nThe program will be terminated!\n");
                Console.ReadLine();
                Environment.Exit(1);
            });

            if (Iterate(_ => YesOrNoIteration(), switches, "##### Do you want to install the prerequisites? [Y/n] ", DefaultIterationErrorMessage))
            {
                foreach (var component in components.Where(x => x.IsPrerequisite).OrderBy(x => x.InstallOrder))
                {
                    Handle(webInteractions, systemInteractions, component, switches, downloadPath);
                }
            }

            Console.Clear();
            Console.WriteLine($"##### ~ Actor v{version}");

            foreach (var component in components.Where(x => !x.IsPrerequisite).OrderBy(x => x.InstallOrder))
            {
                Handle(webInteractions, systemInteractions, component, switches, downloadPath, installPath);
            }

            Console.WriteLine("##### Clearing Download folder...");
            Directory.Delete(downloadPath, true);

            var actComponent = components.First(x => x.InstallOrder == 3);
            var actConfiguration = actComponent.Configurations.First();
            ActConfigurationHelper.SaveConfiguration(actConfiguration.Key, actConfiguration.Value, true, _ =>
            {
                if (switches != CommandLineSwitches.UserInput)
                    return true;

                return Iterate(__ =>
                    {
                        var result = YesOrNoIteration();
                        return result.HasValue && result.Value;
                    }, CommandLineSwitches.UserInput,
                    $"##### Do you want to overwrite the existing configuration for {actComponent.Name}? [Y/n] ",
                    DefaultIterationErrorMessage);
            });

            var firewallHelper = FirewallHelper.Instance;
            var actExePath = Path.Combine(installPath, actComponent.Name + ".exe");
            if (firewallHelper.IsFirewallInstalled && firewallHelper.IsFirewallEnabled)
            {
                if (Iterate(_ => YesOrNoIteration(), switches, $"##### Would you like to add {actComponent.Name} to the Firewall Exceptions? [Y/n] ", DefaultIterationErrorMessage))
                {
                    var actPath = actExePath;
                    try
                    {
                        firewallHelper.GrantAuthorization(actPath, actComponent.Name);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
            }

            SystemInteractions.ApplyCompatibilityChanges(actExePath, CompatibilityMode.RUNASADMIN, CompatibilityMode.GDIDPISCALING, CompatibilityMode.DPIUNAWARE);

            if (Iterate(_ => YesOrNoIteration(), switches, $"##### Do you want to run {actComponent.Name}? [Y/n] ", DefaultIterationErrorMessage))
                systemInteractions.CreateProcess(actExePath).Start();

            Console.WriteLine("##### Finally we are done!");
            Console.WriteLine("##### Press any key to close this windows...");
            Console.ReadLine();
        }

        private static bool? YesOrNoIteration(string defaultValue = "y")
        {
            var result = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(result))
                return defaultValue.Equals("y", StringComparison.InvariantCultureIgnoreCase);

            if (result.Equals("y", StringComparison.InvariantCultureIgnoreCase))
                return true;

            if (result.Equals("n", StringComparison.InvariantCultureIgnoreCase))
                return false;

            return null;
        }

        private static bool Iterate(Func<Unit, bool?> action, CommandLineSwitches switches, string question = null, string errorMessage = null)
        {
            if (switches != CommandLineSwitches.UserInput)
                return switches == CommandLineSwitches.SilentInstallAll;

            bool? result = null;

            while (!result.HasValue)
            {
                if (!string.IsNullOrWhiteSpace(question))
                    Console.Write(question);

                result = action.Invoke(Unit.Default);

                if (!result.HasValue && !string.IsNullOrWhiteSpace(errorMessage))
                    Console.WriteLine(errorMessage);
            }

            return result.Value;
        }

        private static void Handle(WebInteractions webInteractions,
                                   SystemInteractions systemInteractions,
                                   Component component,
                                   CommandLineSwitches switches,
                                   string downloadTo,
                                   string installTo = "")
        {
            if (component.CanBeSkipped)
            {
                if (!Iterate(_ => YesOrNoIteration(), switches, $"##### Do you want to install {component.Name}? [Y/n] ", DefaultIterationErrorMessage))
                    return;
            }

            var componentVersionCheck = component.IsPrerequisite ? component.VersionCheck : Path.Combine(installTo, component.VersionCheck);
            if (systemInteractions.CheckVersion(componentVersionCheck, component.Version, () => Console.WriteLine($"##### Unable to check the version for {component.Name}...")))
            {
                Console.WriteLine($"##### The latest version of {component.Name} is already installed!");
                if (component.IsPlugin)
                    UpdatePluginConfiguration(component, UpdatePluginInstallPath(component, installTo), switches);
                return;
            }

            var downloadToFullPath = Path.Combine(downloadTo, component.FileName);
            var parsingText = $"##### Parsing latest github api for {component.Name}...";
            var downloadingText = $"##### Downloading {component.Name} -> ";
            const string installText = "##### {0} {1}...";

            var downloadFrom = component.Url;
            if (component.IsFromGitHub)
            {
                var isWin7 = !string.IsNullOrWhiteSpace(component.Win7InstallArguments) 
                             && Environment.OSVersion.Version.Major == 6
                             && Environment.OSVersion.Version.Minor == 1;

                var installArguments = isWin7 ? component.Win7InstallArguments : component.InstallArguments;
                downloadFrom = webInteractions.ParseAssetFromGitHub(downloadFrom, int.Parse(installArguments), () => Console.WriteLine(parsingText));
            }

            var bundle = webInteractions.Download(downloadFrom, downloadToFullPath, () => Console.Write(downloadingText), args => Console.Write($"\r{downloadingText} {args.ProgressPercentage}%"), () => Console.Write("\n"));
            if (bundle.Result == WebInteractionsResultType.Fail)
            {
                Console.WriteLine($"\nThere was an error while downloading {component.Name}.\nThe program will be terminated!\n");
                Console.ReadLine();
                Environment.Exit(1);
            }

            var file = bundle.DownloadedFile;
            if (component.ComponentType == ComponentType.Executable)
            {
                Console.WriteLine(installText, "Installing", component.Name);
                systemInteractions.Install(file.FullName, component.InstallArguments);
            }
            else
            {
                if (string.IsNullOrWhiteSpace(installTo))
                {
                    Console.WriteLine($"\nThere was an error while unzipping {component.Name}.\nThe install path was not valid, the program will be terminated!\n");
                    Console.ReadLine();
                    Environment.Exit(1);
                }

                if (component.IsPlugin)
                {
                    installTo = UpdatePluginInstallPath(component, installTo);
                    UpdatePluginConfiguration(component, installTo, switches);
                }

                try
                {
                    if (Directory.Exists(installTo))
                        Directory.Delete(installTo);
                }
                catch (Exception)
                {
                    // do nothing
                }

                Console.WriteLine(installText, "Unzipping", component.Name);
                systemInteractions.Unzip(file.FullName, installTo);
            }
        }

        private static string UpdatePluginInstallPath(Component component, string installTo)
        {
            installTo = Path.Combine(installTo, "plugin");
            if (!Directory.Exists(installTo))
                Directory.CreateDirectory(installTo);

            installTo = Path.Combine(installTo, component.Name);
            return installTo;
        }

        private static void UpdatePluginConfiguration(Component component, string destination, CommandLineSwitches switches)
        {
            foreach (var componentLibrary in component.Libraries)
            {
                ActConfigurationHelper.AddPlugin(Path.Combine(destination, componentLibrary));
            }

            if (component.Configurations == null) return;

            bool? result = null;
            foreach (var componentConfiguration in component.Configurations)
            {
                var url = componentConfiguration.Key;
                var confDestination = componentConfiguration.Value;

                ActConfigurationHelper.SaveConfiguration(url, confDestination, onDuplicatd: _ =>
                {
                    if (result.HasValue)
                        return result != null && result.Value;

                    return Iterate(__ =>
                        {
                            result = YesOrNoIteration();
                            return result != null && result.Value;
                        }, switches, $"##### Do you want to overwrite the existing configuration for {component.Name}? [Y/n] ",
                        DefaultIterationErrorMessage);
                });
            }
        }
    }
}
