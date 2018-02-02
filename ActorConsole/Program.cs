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

        private static void Main()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            var downloadPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "download");
            var installPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ACT");

            Console.WriteLine($"##### ~ ActorConsole v{version}");
            Console.WriteLine($"##### Going to install ACT in '{installPath}'");

            if (Iterate(_ => YesOrNoIteration(), "##### Would you like to change it?' [y/n] ", DefaultIterationErrorMessage))
            {
                Iterate(__ => 
                {
                    installPath = Console.ReadLine();
                    return Uri.IsWellFormedUriString(installPath, UriKind.Absolute);
                }, "##### Write the path you prefer: ", "##### The path inserted is not valid...");
            }

            Console.WriteLine("##### To ensure that ACT works correctly you should first install:");
            Console.WriteLine("#####   1. Microsoft Visual C++ Redistributable");
            Console.WriteLine("#####   2. Microsoft .NET Framework 4.7");
            Console.WriteLine("#####   3. Win10Pcap");
            Console.WriteLine("##### If you have already installed then you can skip this step.");

            if (!Directory.Exists(downloadPath))
                Directory.CreateDirectory(downloadPath);

            var systemInteractions = new SystemInteractions();
            var webInteractions = new WebInteractions();

            var components = webInteractions.LoadConfiguration(() =>
            {
                Console.WriteLine("##### An error occurred when reading the configuration file.\nThe program will be terminated!\n");
                Console.ReadLine();
                Environment.Exit(1);
            });

            if (Iterate(_ => YesOrNoIteration(), "##### Do you want to install the prerequisites? [y/n] ", DefaultIterationErrorMessage))
            {
                foreach (var component in components.Where(x => x.IsPrerequisite).OrderBy(x => x.InstallOrder))
                {
                    Handle(webInteractions, systemInteractions, component, downloadPath);
                }
            }

            Console.Clear();
            Console.WriteLine($"##### ~ Actor v{version}");

            foreach (var component in components.Where(x => !x.IsPrerequisite).OrderBy(x => x.InstallOrder))
            {
                Handle(webInteractions, systemInteractions, component, downloadPath, installPath);
            }

            Console.WriteLine("##### Clearing Download folder...");
            Directory.Delete(downloadPath, true);
            Console.WriteLine("##### Finally we are done!");

            var actComponent = components.First(x => x.InstallOrder == 3);
            if (Iterate(_ => YesOrNoIteration(), $"##### Do you want to run {actComponent.Name}? [y/n] ", DefaultIterationErrorMessage))
                systemInteractions.CreateProcess(Path.Combine(installPath, actComponent.Name + ".exe")).Start();

            Console.WriteLine("##### Press any key to close this windows...");
            Console.ReadLine();
        }

        private static bool? YesOrNoIteration()
        {
            var result = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(result))
                return null;

            if (result.Equals("n", StringComparison.InvariantCultureIgnoreCase))
                return false;

            if (result.Equals("y", StringComparison.InvariantCultureIgnoreCase))
                return true;

            return null;
        }

        private static bool Iterate(Func<Unit, bool?> action, string question = null, string errorMessage = null)
        {
            bool? result = null;
            {
                while (!result.HasValue)
                {
                    if (!string.IsNullOrWhiteSpace(question))
                        Console.Write(question);

                    result = action.Invoke(Unit.Default);

                    if (!result.HasValue && !string.IsNullOrWhiteSpace(errorMessage))
                        Console.WriteLine(errorMessage);
                }
            }

            return result.Value;
        }

        private static void Handle(WebInteractions webInteractions,
                                   SystemInteractions systemInteractions,
                                   Component component,
                                   string downloadTo,
                                   string installTo = "")
        {
            if(component.CanBeSkipped && component.IsPlugin)
            {
                if (!Iterate(_ => YesOrNoIteration(), $"##### Do you want to install {component.Name}? [y/n] ", DefaultIterationErrorMessage))
                    return;
            }

            var componentVersionCheck = component.IsPrerequisite ? component.VersionCheck : Path.Combine(installTo, component.VersionCheck);
            if (systemInteractions.CheckVersion(componentVersionCheck, component.Version, () => Console.WriteLine($"##### Unable to check the version for {component.Name}...")))
            {
                Console.WriteLine($"##### The latest version of {component.Name} is already installed!");
                return;
            }

            var downloadToFullPath = Path.Combine(downloadTo, component.FileName);
            var parsingText = $"##### Parsing latest github api for {component.Name}...";
            var downloadingText = $"##### Downloading {component.Name} -> ";
            const string installText = "##### {0} {1}...";

            var downloadFrom = component.Url;
            if (component.IsFromGitHub)
            {
                downloadFrom = webInteractions.ParseAssetFromGitHub(downloadFrom, int.Parse(component.InstallArguments), () => Console.WriteLine(parsingText));
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
                    installTo = Path.Combine(installTo, "plugin");
                    if (!Directory.Exists(installTo))
                        Directory.CreateDirectory(installTo);

                    installTo = Path.Combine(installTo, component.Name);
                }

                Console.WriteLine(installText, "Unzipping", component.Name);
                systemInteractions.Unzip(file.FullName, installTo);
            }
        }
    }
}
