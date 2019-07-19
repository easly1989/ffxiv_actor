using System;

namespace Actor.UI.Common.Extensions
{
    public static class ViewModelExtensions
    {
        public static void DisposeWith(this ViewModelBase viewModel, IDisposable disposable)
        {
            viewModel.AddDisposable(disposable);
        }
    }
}
