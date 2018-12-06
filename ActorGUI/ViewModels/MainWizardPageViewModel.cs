namespace ActorGUI.ViewModels
{
    /// <summary>
    /// The introduction page of the wizard
    /// </summary>
    public class MainWizardPageViewModel : PageViewModel
    {
        public override Page UndoPage { get; }
        public override Page SkipPage { get; }
        public override Page ContinuePage { get; }

        public MainWizardPageViewModel() 
            : base("translate")
        {
        }
    }
}