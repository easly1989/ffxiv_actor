using System;
using System.IO;
using System.Windows.Input;
using Actor.Core;
using GalaSoft.MvvmLight.CommandWpf;

namespace ActorGUI.ViewModels
{
    /// <summary>
    /// Page used at the start of the application to let the user decided the installation path
    /// </summary>
    public class ActPathSelectionPageViewModel : PageViewModel
    {
        private string _actPath;

        public ICommand SavePathCommand { get; }

        public string SelectionHelpText => "Would you like to change the install path of ACT?";
        public string SavePathText => "Continue";

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
            : base("ActPathSelectionPage")
        {
            var hasInstallPath = !string.IsNullOrWhiteSpace(cmdResultInstallPath);

            _actPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ACT");
            if (hasInstallPath && SystemInteractions.IsValidPath(cmdResultInstallPath))
                _actPath = cmdResultInstallPath;

            SavePathCommand = new RelayCommand(OnExecute, OnCanExecute);
        }

        private void OnExecute()
        {
            if (!SystemInteractions.IsValidPath(_actPath)) 
                return;

            ActConfigurationHelper.UpdateActInstallPath(_actPath);
            RequestContinue();
        }

        private bool OnCanExecute()
        {
            return SystemInteractions.IsValidPath(_actPath, false);
        }
    }
}