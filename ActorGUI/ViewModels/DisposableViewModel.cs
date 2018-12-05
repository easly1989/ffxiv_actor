using System;
using System.Collections.Generic;
using System.Linq;
using GalaSoft.MvvmLight;

namespace ActorGUI.ViewModels
{
    public abstract class DisposableViewModel : ViewModelBase, IDisposable
    {
        private readonly Dictionary<Guid, IDisposable> _disposables;

        protected DisposableViewModel()
        {
            _disposables = new Dictionary<Guid, IDisposable>();
        }

        /// <summary>
        /// Handles the disposable of the current viewmodel;
        /// Loosing the Guid means the disposable will be disposd when the current viewmodel will be disposed
        /// </summary>
        /// <param name="disposable">the disposable to handle</param>
        /// <returns>the Guid of the disposable, usefull to remove an entry if needed</returns>
        protected Guid AddDisposable(IDisposable disposable)
        {
            var disposableGuid = Guid.NewGuid();
            _disposables.Add(disposableGuid, disposable);
            return disposableGuid;
        }

        protected virtual void OnDispose()
        {
        }

        public void Dispose()
        {
            OnDispose();

            foreach (var disposable in _disposables.Select(x => x.Value))
            {
                disposable.Dispose();
            }
            _disposables.Clear();
        }
    }
}