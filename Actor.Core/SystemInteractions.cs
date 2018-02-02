using System;
using System.Diagnostics;
using System.IO;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
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
            var processStartInfo = new ProcessStartInfo(executablePath, arguments);
            var process = new Process { StartInfo = processStartInfo };
            return process;
        }

        /// <summary>
        /// Runs the executable with the given arguments, without waiting for the installation process to finish. 
        /// This method is meant for installers/setup executables that can be installed silently!
        /// </summary>
        /// <param name="executablePath">The path to the installer/setup executable</param>
        /// <param name="args">All the arguments needed to run the executable (and to avoid user interaction with it!)</param>
        public void InstallAsync(string executablePath, string args)
        {
            var disposable = new CompositeDisposable();
            var process = CreateProcess(executablePath, args);

            disposable.Add(Observable.FromEventPattern(
                    handler => process.Exited += handler,
                    handler => process.Exited -= handler)
                .Subscribe(_ =>
                {
                    _operationCompletedSubject.OnNext(process.ExitCode == 0);
                    disposable.Dispose();
                }));

            process.Start();
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
        public void UnzipAsync(string from, string to)
        {
            var task = new Task(() =>
            {
                var result = Unzip(from, to);
                _operationCompletedSubject.OnNext(result);
            }, CancellationToken.None);

            task.ContinueWith(_ => task.Dispose());
            task.Start();
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
                var libraryFilePath = Path.Combine(libsPath, "x86", "7z.dll");
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
                if (path.StartsWith("%"))
                    path = Environment.ExpandEnvironmentVariables(path);

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
    }
}