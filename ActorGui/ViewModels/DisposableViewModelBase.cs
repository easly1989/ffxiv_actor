using System;
using GalaSoft.MvvmLight;

namespace ActorGui.ViewModels
{
    public abstract class DisposableViewModelBase : ViewModelBase, IDisposable
    {
        public void Dispose()
        {
            OnDispose();
        }

        /// <summary>
        /// Override to ensure that all the resources used gets destroyed when necessary
        /// There is no need to do "base.OnDispose()", as the method is empty on the base class
        /// </summary>
        protected virtual void OnDispose()
        {
        }
    }
}