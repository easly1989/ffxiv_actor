using System.Reflection;
using Actor.UI.Common;

namespace ActorWizard.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public string WindowTitle => $"Actor Wizard ~ v{Assembly.GetExecutingAssembly().GetName().Version}";
    }
}
