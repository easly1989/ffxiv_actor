using System;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;

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
        public void Install(string executablePath, params string[] args)
        {
            var arguments = args == null ? string.Empty : string.Join(" ", args).TrimEnd();
            var processStartInfo = new ProcessStartInfo(executablePath, arguments);
            var process = new Process { StartInfo = processStartInfo };

            process.Start();
            process.WaitForExit();
        }
    }
}