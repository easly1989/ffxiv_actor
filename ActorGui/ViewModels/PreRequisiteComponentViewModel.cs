using Actor.Core;

namespace ActorGui.ViewModels
{
    public class PreRequisiteComponentViewModel : ComponentViewModelBase
    {
        public PreRequisiteComponentViewModel(
            Component component, 
            SystemInteractions systemInteractions) 
            : base(component, systemInteractions, component.VersionCheck)
        {
        }
    }
}