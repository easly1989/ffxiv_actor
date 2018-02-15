using System.Reflection;

namespace ActorGui.ViewModels
{
    public class MainViewModel : DisposableViewModelBase
    {
        public string Title => $"ActorGui ~ v{Assembly.GetExecutingAssembly().GetName().Version}";
    }
}
