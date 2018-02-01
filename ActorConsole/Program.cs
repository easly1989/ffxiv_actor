using System;
using System.IO;
using System.Reactive;
using System.Reflection;
using Actor.Core;

namespace ActorConsole
{
    internal class Program
    {
        private const string DotNetFx = "http://go.microsoft.com/fwlink/?linkid=825298";
        private const string Win10Pcap = "http://www.win10pcap.org/download/Win10Pcap-v10.2-5002.msi";
        private const string VCx64 = "https://go.microsoft.com/fwlink/?LinkId=746572";
        private const string VCx86 = "https://go.microsoft.com/fwlink/?LinkId=746571";
        private const string Act = "http://advancedcombattracker.com/includes/page-download.php?id=57";
        private const string FFxivPlugin = "https://api.github.com/repos/ravahn/FFXIV_ACT_Plugin/releases/latest";
        private const string HojoringPlugin = "https://api.github.com/repos/anoyetta/ACT.Hojoring/releases/latest";
        private const string OverlayPlugin = "https://api.github.com/repos/hibiyasleep/OverlayPlugin/releases/latest";
        private const string DfAssistPlugin = "https://api.github.com/repos/wanaff14/ACTFate/releases/latest";

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

            var webInteractions = new WebInteractions();
            var systemInteractions = new SystemInteractions();
            if (Iterate(_ => YesOrNoIteration(), "##### Do you want to install the prerequisites? [y/n] ", DefaultIterationErrorMessage))
            {
                if (!Directory.Exists(downloadPath))
                    Directory.CreateDirectory(downloadPath);

                Handle(webInteractions, systemInteractions, "vc.exe", Environment.Is64BitOperatingSystem ? VCx64 : VCx86, downloadPath, "Microsoft Visual C++ Redistributable", installArguments: new[] { "/passive", "/promptrestart" });
                Handle(webInteractions, systemInteractions, "dotnetfx4_7.exe", DotNetFx, downloadPath, "Microsoft .NET Framework 4.7", installArguments: new[] { "/passive", "/promptrestart" });
                Handle(webInteractions, systemInteractions, "win10pcap.msi", Win10Pcap, downloadPath, "Win10Pcap", installArguments: new[] { "/passive", "/promptrestart" });
            }

            Console.Clear();
            Console.WriteLine($"##### ~ Actor v{version}");

            Handle(webInteractions, systemInteractions, "act.zip", Act, downloadPath, "Advanced Combat Tracker", installPath);
            Handle(webInteractions, systemInteractions, "ffxiv_act_plugin.zip", FFxivPlugin, downloadPath, "FFXIV Parsing", installPath, true);
            Handle(webInteractions, systemInteractions, "hojoring.7z", HojoringPlugin, downloadPath, "Hojoring", installPath, true);
            Handle(webInteractions, systemInteractions, "overlay.zip", OverlayPlugin, downloadPath, "Overlay", installPath, true, Environment.Is64BitOperatingSystem ? 0 : 2);
            Handle(webInteractions, systemInteractions, "dfassist.zip", DfAssistPlugin, downloadPath, "DFAssist", installPath, true);

            Console.WriteLine("##### Clearing Download folder...");
            Directory.Delete(downloadPath, true);
            Console.WriteLine("##### Finally we are done!");
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

            return true;
        }

        private static void Handle(WebInteractions webInteractions,
                                   SystemInteractions systemInteractions,
                                   string fileName,
                                   string downloadFrom,
                                   string downloadTo,
                                   string componentName,
                                   string installTo = null,
                                   bool isPlugin = false,
                                   int assetToInstall = 0,
                                   string[] installArguments = null)
        {
            var actualComponentName = isPlugin ? componentName + " Plugin" : componentName;
            if(isPlugin)
            {
                if (!Iterate(_ => YesOrNoIteration(), $"##### Do you want to install {componentName}? [y/n] ", DefaultIterationErrorMessage))
                    return;

            }

            var downloadToFullPath = Path.Combine(downloadTo, fileName);
            var parsingText = $"##### Parsing latest github api for {actualComponentName}...";
            var downloadingText = $"##### Downloading {actualComponentName} -> ";
            const string installText = "##### {0} {1}...";

            if (downloadFrom.Contains("api.github.com"))
            {
                downloadFrom = webInteractions.ParseAssetFromGitHub(downloadFrom, assetToInstall, () => Console.WriteLine(parsingText));
            }

            var bundle = webInteractions.Download(downloadFrom, downloadToFullPath, () => Console.Write(downloadingText), args => Console.Write($"\r{downloadingText} {args.ProgressPercentage}%"), () => Console.Write("\n"));
            if (bundle.Result == WebInteractionsResultType.Fail)
            {
                Console.WriteLine($"\nThere was an error while downloading {actualComponentName}.\nThe program will be terminated!\n");
                Console.ReadLine();
                Environment.Exit(1);
            }

            var file = bundle.DownloadedFile;
            if (systemInteractions.CheckIfFileIsExecutable(file.FullName))
            {
                Console.WriteLine(installText, "Installing", actualComponentName);
                systemInteractions.Install(file.FullName, installArguments);
            }
            else
            {
                if (string.IsNullOrWhiteSpace(installTo))
                {
                    Console.WriteLine($"\nThere was an error while unzipping {actualComponentName}.\nThe install path was not valid, the program will be terminated!\n");
                    Console.ReadLine();
                    Environment.Exit(1);
                }

                if (isPlugin)
                {
                    installTo = Path.Combine(installTo, "plugin");
                    if (!Directory.Exists(installTo))
                        Directory.CreateDirectory(installTo);

                    installTo = Path.Combine(installTo, actualComponentName);
                }

                Console.WriteLine(installText, "Unzipping", actualComponentName);
                systemInteractions.Unzip(file.FullName, installTo);
            }
        }
    }
}
