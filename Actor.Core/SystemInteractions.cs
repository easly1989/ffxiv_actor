using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32;
using SevenZipExtractor;

namespace Actor.Core
{
    public class SystemInteractions
    {
        private readonly ISubject<bool> _operationCompletedSubject;

        /// <summary>
        /// Returns True when the operation is completed without errors, False otherwise
        /// </summary>
        public IObservable<bool> WhenOperationCompleted => _operationCompletedSubject.AsObservable();

        public SystemInteractions()
        {
            _operationCompletedSubject = new Subject<bool>();
        }

        /// <summary>
        /// Kills all the process that have the same process name
        /// </summary>
        /// <param name="processName">The process name to match with all the active process</param>
        public void KillProcess(string processName)
        {
            if (string.IsNullOrWhiteSpace(processName)) throw new ArgumentException(nameof(processName));

            try
            {
                var processes = Process.GetProcessesByName(processName);
                foreach (var process in processes)
                {
                    process.Kill();
                }
            }
            catch (Exception)
            {
                // do nothing
            }
        }

        /// <summary>
        /// Creates a process based on the executablePath and the arguments given.
        /// Starting/Stopping/Disposing the process is not handled by this method
        /// </summary>
        /// <param name="executablePath">The path to the executable (Cannot be null)</param>
        /// <param name="args">All the arguments needed to run the executable (and to avoid user interaction with it!)</param>
        /// <returns>The created process</returns>
        public Process CreateProcess(string executablePath, string args = null)
        {
            if (string.IsNullOrWhiteSpace(executablePath)) throw new ArgumentException(nameof(executablePath));

            var arguments = " " + args;
            // ReSharper disable AssignNullToNotNullAttribute
            var processStartInfo = new ProcessStartInfo(executablePath, arguments) { WorkingDirectory = Path.GetDirectoryName(executablePath) };
            // ReSharper restore AssignNullToNotNullAttribute
            var process = new Process
            {
                StartInfo = processStartInfo,
                EnableRaisingEvents = true
            };
            return process;
        }

        /// <summary>
        /// Runs the executable with the given arguments, without waiting for the installation process to finish. 
        /// This method is meant for installers/setup executables that can be installed silently!
        /// </summary>
        /// <param name="executablePath">The path to the installer/setup executable</param>
        /// <param name="args">All the arguments needed to run the executable (and to avoid user interaction with it!)</param>
        public async Task InstallAsync(string executablePath, string args)
        {
            var taskCompletionSource = new TaskCompletionSource<int>();
            var disposable = new CompositeDisposable();
            var process = CreateProcess(executablePath, args);

            disposable.Add(Observable.FromEventPattern(
                    handler => process.Exited += handler,
                    handler => process.Exited -= handler)
                .Subscribe(_ =>
                {
                    _operationCompletedSubject.OnNext(process.ExitCode == 0);
                    taskCompletionSource.SetResult(process.ExitCode);
                    disposable.Dispose();
                }));

            process.Start();
            await taskCompletionSource.Task;
        }

        /// <summary>
        /// Runs the executable with the given arguments.
        /// </summary>
        /// <param name="executablePath">The path to the installer/setup executable</param>
        /// <param name="args">All the arguments needed to run the executable (and to avoid user interaction with it!)</param>
        /// <returns>True when the operation is completed without errors, False otherwise</returns>
        public bool Install(string executablePath, string args)
        {
            var process = CreateProcess(executablePath, args);

            process.Start();
            process.WaitForExit();

            return process.ExitCode == 0;
        }

        /// <summary>
        /// Extracts the content of the defined archive to the destination path provided
        /// </summary>
        /// <param name="from">The path to the archive</param>
        /// <param name="to">The extraction path</param>
        public async Task UnzipAsync(string from, string to)
        {
            var task = new Task(() =>
            {
                var result = Unzip(from, to);
                _operationCompletedSubject.OnNext(result);
            }, CancellationToken.None);

            task.Start();
            await task;
        }
        /// <summary>
        /// Extracts the content of the defined archive to the destination path provided
        /// </summary>
        /// <param name="from">The path to the archive</param>
        /// <param name="to">The extraction path</param>
        /// <returns>True when the operation is completed without errors, False otherwise</returns>
        public bool Unzip(string from, string to)
        {
            var result = true;
            try
            {
                var libsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "libs");
                var libraryFilePath = Path.Combine(libsPath, Environment.Is64BitProcess ? "x64" : "x86", "7z.dll");
                using (var archiveFile = new ArchiveFile(from, libraryFilePath))
                {
                    archiveFile.Extract(to);
                }
            }
            catch (Exception)
            {
                result = false;
            }

            return result;
        }

        /// <summary>
        /// Checks the version of the given path.
        /// The path given may be:
        /// 1 - A registry path, must add a '$' at the start, and ';' at the end, followed by the property to check
        /// 2 - A windows shortcut, must start with a '%' (example: %windir%\\system32)
        /// 3 - A relative or absolute path to any file
        /// </summary>
        /// <param name="path">The path to get the version from</param>
        /// <param name="latest">The version to check with</param>
        /// <param name="onError">The action to invoke in case of errors</param>
        /// <returns></returns>
        public bool CheckVersion(string path, string latest, Action onError = null)
        {
            try
            {
                // This is not a normal path, we should check the registry
                if (path.StartsWith("$"))
                {
                    // also, in this case we need to remove the first character,
                    // as it is used just to recognize the string as a registry path
                    path = path.Substring(1, path.Length - 1);
                    // the path needs to be splitted, as before the ';' resides the actual path
                    // and after the ';' resides the property to check to get the installed version
                    var split = path.Split(';');
                    var actualPath = split[0];
                    var versionCheck = split[1];

                    var regKey = (string)Registry.GetValue(actualPath, versionCheck, null);
                    return !string.IsNullOrWhiteSpace(regKey) && regKey.Equals(latest);
                }

                // This is not a normal path, we need to expand the environment variable first
                path = TryExpandSystemVariable(path);

                // at this point, the path should be a normal one (absolute or relative doesn't matter)
                var fileVersionInfo = FileVersionInfo.GetVersionInfo(path);
                return fileVersionInfo.ProductVersion.Equals(latest);
            }
            catch (Exception)
            {
                onError?.Invoke();
                return false;
            }
        }

        /// <summary>
        /// Tries to expand a path that starts with a windows shortcut  (example: %windir%\\system32)
        /// </summary>
        /// <param name="path">The path to check</param>
        /// <returns>The expanded path</returns>
        public static string TryExpandSystemVariable(string path)
        {
            var result = path;
            if (path.StartsWith("%"))
                result = Environment.ExpandEnvironmentVariables(path);
            return result;
        }

        /// <summary>
        /// Renames a File
        /// </summary>
        /// <param name="fullPath">The path to the actual file</param>
        /// <param name="newName">The new name of the file, if null will add Todays.Date and hour to the name of the file</param>
        /// <returns>The path to the renamed file, or the old path in case of error or string empty if the file doesn't exists</returns>
        public static string RenameFile(string fullPath, string newName = "")
        {
            var file = new FileInfo(fullPath);
            if (!file.Exists)
                return string.Empty;

            if (string.IsNullOrWhiteSpace(newName))
            {
                var split = file.Name.Split(new[] { '.' }, 2, StringSplitOptions.RemoveEmptyEntries);
                newName = $"{split[0]}.{DateTime.Now.Ticks}.{split[1]}";
            }

            try
            {
                // if the File.Exists than it is not possible that the Directory doesn't!
                // ReSharper disable PossibleNullReferenceException
                var newPath = Path.Combine(file.Directory.FullName, newName);
                // ReSharper restore PossibleNullReferenceException
                file.MoveTo(newPath);
                return newPath;
            }
            catch (Exception)
            {
                return fullPath;
            }
        }

        /// <summary>
        /// Check if the path given as parameter is valid
        /// </summary>
        /// <param name="path">The path to check</param>
        /// <param name="createDir">Creates the directory if it doesn't exists</param>
        /// <returns>True if the path is in a valid format, false otherwise</returns>
        public static bool IsValidPath(string path, bool createDir = true)
        {
            var driveCheck = new Regex(@"^[a-zA-Z]:\\$");
            if (!driveCheck.IsMatch(path.Substring(0, 3)))
                return false;

            var strTheseAreInvalidFileNameChars = new string(Path.GetInvalidPathChars());
            strTheseAreInvalidFileNameChars += @":/?*" + "\"";
            var containsABadCharacter = new Regex("[" + Regex.Escape(strTheseAreInvalidFileNameChars) + "]");

            if (containsABadCharacter.IsMatch(path.Substring(3, path.Length - 3)))
                return false;

            var dir = new DirectoryInfo(Path.GetFullPath(path));
            if (!dir.Exists && createDir)
                dir.Create();

            return true;
        }

        /// <summary>
        /// Changes the Compatibility settings for the app defined by path
        /// </summary>
        /// <param name="path">The path to the app that needs compatibility changes</param>
        /// <param name="modes">THe comptability flags needed by the app to run properly</param>
        public static void ApplyCompatibilityChanges(string path, params CompatibilityMode[] modes)
        {
            var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Layers", true);
            if (key == null)
                return;

            using (key)
            {
                var value = modes.Aggregate("~", (current, mode) => current + $" {mode.ToString()}");
                key.SetValue(path, value);
            }
        }
    }
}