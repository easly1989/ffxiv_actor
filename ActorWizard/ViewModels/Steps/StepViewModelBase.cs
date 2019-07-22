using System.Windows.Input;
using Actor.UI.Common;

namespace ActorWizard.ViewModels.Steps
{
    public abstract class StepViewModelBase : ViewModelBase
    {
        public int StepId { get; }
        public string StepTitle { get; }

        protected StepViewModelBase(int stepId, string stepTitle)
        {
            StepId = stepId;
            StepTitle = stepTitle; // Localize
        }
    }
}
