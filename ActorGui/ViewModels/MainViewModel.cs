using System.Collections.ObjectModel;
using System.Reflection;
using Actor.Core;

namespace ActorGui.ViewModels
{
    public class MainViewModel : DisposableViewModelBase
    {
        private readonly CommandLineResult _commandLineResult;

        public ObservableCollection<ComponentViewModel> Components { get; }

        public MainViewModel(CommandLineResult commandLineResult)
        {
            _commandLineResult = commandLineResult;

            Components = new ObservableCollection<ComponentViewModel>();
        }

        public string Title => $"ActorGui ~ v{Assembly.GetExecutingAssembly().GetName().Version}";
    }

}
