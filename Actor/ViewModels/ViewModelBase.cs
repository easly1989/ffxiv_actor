using System;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using ReactiveUI;

namespace Actor.ViewModels
{
    public abstract class ViewModelBase : ReactiveObject, IDisposable
    {
        private readonly CompositeDisposable _disposables;

        private bool _isLoading;
        private bool _isLoadingCancelVisible;

        public string LoadingText { get; private set; }
        public string LoadingCancelText { get; private set; }

        public bool IsLoadingCancelVisible
        {
            get => _isLoadingCancelVisible;
            set => this.RaiseAndSetIfChanged(ref _isLoadingCancelVisible, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => this.RaiseAndSetIfChanged(ref _isLoading, value);
        }

        protected ViewModelBase()
        {
            // todo: localize
            LoadingText = "Loading...";
            LoadingCancelText = "Cancel";
            _disposables = new CompositeDisposable();
        }

        protected void AddDisposable(IDisposable disposable)
        {
            _disposables.Add(disposable);
        }

        protected virtual void OnDispose()
        {
        }

        public async Task Load()
        {
            IsLoading = true;
            await Task.Run(OnLoad);
            IsLoading = false;
            IsLoadingCancelVisible = false;
        }

        protected virtual async Task OnLoad()
        {
        }

        public void Dispose()
        {
            OnDispose();
            _disposables?.Dispose();
        }
    }
}