using System;
using System.IO;
using System.Windows.Input;
using Actor.Core;
using ActorGUI.Localization;

namespace ActorGUI.ViewModels
{
    /// <summary>
    /// Page used at the start of the application to let the user decided the installation path
    /// </summary>
    public class ActPathSelectionPageViewModel : PageViewModel
    {
        private string _actPath;

        public override Page UndoPage => default(Page); // not used
        public override Page SkipPage => default(Page); // not used
        public override Page ContinuePage => Page.PreRequisiteInstall; 

        public string SelectionHelpText => Locals.ActPathSelection_Help;

        public string ActPath
        {
            get => _actPath;
            set
            {
                if(Set(ref _actPath, value))
                    CommandManager.InvalidateRequerySuggested();
            }
        }

        public ActPathSelectionPageViewModel(string cmdResultInstallPath) 
            : base(Locals.ActPathSelection_Title)
        {
            var hasInstallPath = !string.IsNullOrWhiteSpace(cmdResultInstallPath);

            _actPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ACT");
            if (hasInstallPath && SystemInteractions.IsValidPath(cmdResultInstallPath))
                _actPath = cmdResultInstallPath;
        }

        protected override void OnContinue()
        {
            if (!SystemInteractions.IsValidPath(_actPath)) 
                return;

            ActConfigurationHelper.UpdateActInstallPath(_actPath);
        }

        protected override bool OnCanContinue()
        {
            return SystemInteractions.IsValidPath(_actPath, false);
        }

        protected override bool OnCanUndo()
        {
            return false;
        }

        protected override bool OnCanSkip()
        {
            return false;
        }
    }
}