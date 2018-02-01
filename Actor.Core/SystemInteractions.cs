using System;
using System.Diagnostics;
using System.IO;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
        /// Runs the executable with the given arguments, without waiting for the installation process to finish. 
        /// This method is meant for installers/setup executables that can be installed silently!
        /// </summary>
        /// <param name="executablePath">The path to the installer/setup executable</param>
        /// <param name="args">All the arguments needed to run the executable (and to avoid user interaction with it!)</param>
        public void InstallAsync(string executablePath, params string[] args)
        {
            var disposable = new CompositeDisposable();
            var arguments = args == null ? string.Empty : string.Join(" ", args).TrimEnd();
            var processStartInfo = new ProcessStartInfo(executablePath, arguments);
            var process = new Process { StartInfo = processStartInfo };

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
        public bool Install(string executablePath, params string[] args)
        {
            var arguments = args == null ? string.Empty : string.Join(" ", args).TrimEnd();
            var processStartInfo = new ProcessStartInfo(executablePath, arguments);
            var process = new Process { StartInfo = processStartInfo };

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
        /// Checks if the file path points to an executable file (where the first 2 bytes must be 'MZ')
        /// or, for msi files, simply checks the extension
        /// </summary>
        /// <param name="file">the file to chec</param>
        /// <returns>True if the file is an executable, False otherwise</returns>
        public bool CheckIfFileIsExecutable(string file)
        {
            if (Path.GetExtension(file) == ".msi")
                return true;

            try
            {
                var firstTwoBytes = new byte[2];
                using (var fileStream = File.Open(file, FileMode.Open))
                {
                    fileStream.Read(firstTwoBytes, 0, 2);
                }
                return Encoding.UTF8.GetString(firstTwoBytes) == "MZ";
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}