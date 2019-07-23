namespace ActorWizard.ViewModels.Steps
{
    public class MainStepViewModel : StepViewModelBase
    {
        public MainStepViewModel() 
            : base(0, "Welcome!")
        {
        }

        protected override bool OnCanGoForward()
        {
            return true;
        }

        protected override bool OnCanGoBackward()
        {
            return false;
        }

        protected override bool OnCanSkip()
        {
            return false;
        }
    }
}