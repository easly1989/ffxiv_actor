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

        public bool CanGoForward()
        {
            return OnCanGoForward();
        }

        public bool CanGoBackward()
        {
            return OnCanGoBackward();
        }

        public bool CanSkip()
        {
            return OnCanSkip();
        }

        protected abstract bool OnCanGoForward();
        protected abstract bool OnCanGoBackward();
        protected abstract bool OnCanSkip();
    }
}
