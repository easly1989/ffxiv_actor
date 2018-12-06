using ActorGUI.Localization;

namespace ActorGUI.ViewModels
{
    public class PreRequisiteInstallPageViewModel : PageViewModel
    {
        public override Page UndoPage => Page.ActPathSelection;
        public override Page SkipPage => Page.MainWizardPage;
        public override Page ContinuePage => Page.MainWizardPage;

        public PreRequisiteInstallPageViewModel() 
            : base(Locals.PreRequisiteInstall_Title)
        {
        }

        protected override bool OnCanUndo()
        {
            return true;
        }
    }
}