using System.Reflection;
using Actor.Core;

namespace ActorGui.ViewModels
{
    public class MainViewModel : DisposableViewModelBase
    {
        private readonly CommandLineResult _commandLineResult;

        public MainViewModel(CommandLineResult commandLineResult)
        {
            _commandLineResult = commandLineResult;
        }

        public string Title => $"ActorGui ~ v{Assembly.GetExecutingAssembly().GetName().Version}";
    }
}
