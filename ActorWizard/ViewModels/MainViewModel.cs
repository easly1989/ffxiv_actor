using System;
using System.Reactive.Linq;
using System.Reflection;
using System.Windows.Input;
using Actor.UI.Common;
using ActorWizard.ViewModels.Steps;

namespace ActorWizard.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public string WindowTitle => $"Actor Wizard ~ v{Assembly.GetExecutingAssembly().GetName().Version}";

        public ICommand NextCommand { get; }
        public ICommand PreviousCommand { get; }
        public ICommand SkipCommnad { get; }

        public string NextCommandText { get; }
        public string PreviousCommandText { get; }
        public string SkipCommandText { get; }

        public bool ShowNextCommand
        {
            get => _showNextCommand;
            set => Set(ref _showNextCommand, value);
        }

        public bool ShowPreviousCommand
        {
            get => _showPreviousCommand;
            set => Set(ref _showPreviousCommand, value);
        }

        public bool ShowSkipCommand
        {
            get => _showSkipCommand;
            set => Set(ref _showSkipCommand, value);
        }

        public StepViewModelBase SelectedStep
        {
            get => _selectedStep;
            set => Set(ref _selectedStep, value);
        }

        public BackgroundImageViewModel BackgroundImage
        {
            get => _backgroundImage;
            set => Set(ref _backgroundImage, value);
        }

        public MainViewModel()
        {
            NextCommandText = "Next"; //localize
            PreviousCommandText = "Previous"; //localize
            SkipCommandText = "Skip"; //localize
        }

        #region Private Fields
        private bool _showNextCommand;
        private bool _showSkipCommand;
        private bool _showPreviousCommand;
        private StepViewModelBase _selectedStep;
        private BackgroundImageViewModel _backgroundImage;

        #endregion
    }
}
