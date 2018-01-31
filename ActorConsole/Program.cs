using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using Newtonsoft.Json;
using SharpCompress.Archives;
using SharpCompress.Readers;

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


        private static void Main()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;

            var downloadPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "download");
            var installPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ACT");
            // this is the simple version of the Actor installer.
            Console.WriteLine($"##### ~ Actor v{version}");
            Console.WriteLine($"##### Going to install ACT in '{installPath}'");

            while (true)
            {
                Console.Write("##### Would you like to change it?' [y/n] ");
                var result = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(result))
                {
                    if (result.Equals("n", StringComparison.InvariantCultureIgnoreCase))
                    {
                        break;
                    }

                    if (result.Equals("y", StringComparison.InvariantCultureIgnoreCase))
                    {
                        while (true)
                        {
                            Console.Write("##### Write the path you prefer: ");
                            installPath = Console.ReadLine();
                            if (Uri.IsWellFormedUriString(installPath, UriKind.Absolute))
                                break;

                            Console.WriteLine("##### The path inserted is not valid...");
                        }

                        break;
                    }
                }

                Console.WriteLine("##### You should answer just with 'y' or 'n'...");
            }

            Console.WriteLine("##### To ensure that ACT works correctly you should first install:");
            Console.WriteLine("##### 1. Microsoft Visual C++ Redistributable");
            Console.WriteLine("##### 2. Microsoft .NET Framework 4.7");
            Console.WriteLine("##### 3. Win10Pcap");
            Console.WriteLine("##### If you have already installed then you can skip this step.");

            while (true)
            {
                Console.Write("##### Do you want to install the prerequisites? [y/n] ");
                var result = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(result))
                {
                    if (result.Equals("n", StringComparison.InvariantCultureIgnoreCase))
                    {
                        break;
                    }

                    if (result.Equals("y", StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (!Directory.Exists(downloadPath))
                            Directory.CreateDirectory(downloadPath);

                        using (var webClient = new WebClient())
                        {
                            string pre;
                            var downloadText = "##### Downloading Microsoft Visual C++ Redistributable -> ";
                            var installText = "##### Installing Microsoft Visual C++ Redistributable";
                            if (Environment.Is64BitOperatingSystem)
                            {
                                pre = Path.Combine(downloadPath, "vcx64.exe");
                                Download(downloadText, webClient, VCx64, pre);
                                Install(installText, pre, "/passive /promptrestart");
                            }
                            else
                            {
                                pre = Path.Combine(downloadPath, "vcx86.exe");
                                Download(downloadText, webClient, VCx86, pre);
                                Install(installText, pre, "/passive /promptrestart");
                            }

                            downloadText = "##### Downloading Microsoft .NET Framework 4.7 -> ";
                            installText = "##### Installing Microsoft .NET Framework 4.7";
                            pre = Path.Combine(downloadPath, "dotnetfx.exe");
                            Download(downloadText, webClient, DotNetFx, pre);
                            Install(installText, pre, "/passive /promptrestart");

                            downloadText = "##### Downloading Win10Pcap -> ";
                            installText = "##### Installing Win10Pcap";
                            pre = Path.Combine(downloadPath, "win10pcap.msi");
                            Download(downloadText, webClient, Win10Pcap, pre);
                            Install(installText, pre, "/passive /promptrestart");
                        }

                        break;
                    }
                }

                Console.WriteLine("##### You should answer just with 'y' or 'n'...");
            }

            Console.Clear();
            Console.WriteLine($"##### ~ Actor v{version}");

            using (var webClient = new WebClient())
            {
                var download = Path.Combine(downloadPath, "act.zip");
                var downText = "##### Downloading Advanced Combat Tracker -> ";
                var instText = "##### Unzipping Advanced Combat Tracker -> ";
                Download(downText, webClient, Act, download);
                Unzip(instText, download, installPath, true);

                var pluginPath = Path.Combine(installPath, "plugin");
                if (!Directory.Exists(pluginPath))
                    Directory.CreateDirectory(pluginPath);

                download = Path.Combine(downloadPath, "FFXIV_ACT_Plugin.zip");
                var parseText = "##### Parsing latest github api for FFXIV Parsing Plugin...";
                downText = "##### Downloading FFXIV Parsing Plugin -> ";
                instText = "##### Unzipping FFXIV Parsing Plugin -> ";
                GitHubLatestDownload(parseText, downText, download, FFxivPlugin);
                Unzip(instText, download, Path.Combine(pluginPath, "FFXIV_ACT_Plugin"));

                download = Path.Combine(downloadPath, "Hojoring.7z");
                parseText = "##### Parsing latest github api for Hojoring Plugin...";
                downText = "##### Downloading Hojoring Plugin -> ";
                instText = "##### Unzipping Hojoring Plugin -> ";
                GitHubLatestDownload(parseText, downText, download, HojoringPlugin);
                Unzip(instText, download, Path.Combine(pluginPath, "Hojoring"));

                download = Path.Combine(downloadPath, "Overlay_Plugin.zip");
                parseText = "##### Parsing latest github api for Overlay Plugin...";
                downText = "##### Downloading Overlay Plugin -> ";
                instText = "##### Unzipping Overlay Plugin -> ";
                GitHubLatestDownload(parseText, downText, download, OverlayPlugin, Environment.Is64BitOperatingSystem ? 0 : 2);
                Unzip(instText, download, Path.Combine(pluginPath, "Overlay_Plugin"));
            }

            Console.WriteLine("Finally we are done!\nPress any button to close this windows...");
            Console.ReadLine();
        }

        private static void GitHubLatestDownload(string parseText, string downText, string downloadPath, string parseUrl, int asset = 0)
        {
            using (var webClient = new WebClient())
            {
                Console.WriteLine(parseText);
                webClient.Headers.Add("user-agent", "avoid 403");
                var downloadString = webClient.DownloadString(parseUrl);
                dynamic json = JsonConvert.DeserializeObject(downloadString);
                var githubUrl = json.assets[asset].browser_download_url;
                Download(downText, webClient, githubUrl.Value, downloadPath);
            }
        }

        private static void Unzip(string installText, string zip, string destination, bool deleteDir = false)
        {
            if(deleteDir && Directory.Exists(destination))
                Directory.Delete(destination, true);

            Console.Write(installText);
            using (var archive = ArchiveFactory.Open(zip))
            {
                var archiveEntries = archive.Entries.Where(x => !x.IsDirectory && !x.IsEncrypted).ToArray();
                var count = 1;
                foreach (var entry in archiveEntries)
                {
                    try
                    {
                        var value = (100 * count) / archiveEntries.Length;
                        Console.Write($"\r{installText} {value}%");
                        count++;

                        entry.WriteToDirectory(destination, new ExtractionOptions { ExtractFullPath = true, Overwrite = true });
                    }
                    catch (Exception)
                    {
                        // do nothing...
                    }

                }
            }

            Console.Write("\n");
        }

        private static void Install(string installText, string setup, string args)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo(setup)
                {
                    Arguments = args
                }
            };

            Console.WriteLine(installText);
            process.Start();
            process.WaitForExit();
        }
        
        private static void Download(string message, WebClient webClient, string url, string downloadPath)
        {
            Console.Write(message);

            void Progress(object sender, DownloadProgressChangedEventArgs args)
            {
                Console.Write($"\r{message} {args.ProgressPercentage}%");
            }

            void Completed(object sender, AsyncCompletedEventArgs args)
            {
                webClient.DownloadProgressChanged -= Progress;
                webClient.DownloadFileCompleted -= Completed;
                Console.Write("\n");
            }

            webClient.DownloadProgressChanged += Progress;
            webClient.DownloadFileCompleted += Completed;
            var downloadTask = webClient.DownloadFileTaskAsync(new Uri(url), downloadPath);
            downloadTask.Wait();
        }
    }
}
