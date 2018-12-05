using System.Reflection;
using ReactiveUI;
using Splat;

namespace ActorGUI
{
    public partial class App
    {
        public App()
        {
            Locator.CurrentMutable.RegisterViewsForViewModels(Assembly.GetCallingAssembly());
        }
    }
}
