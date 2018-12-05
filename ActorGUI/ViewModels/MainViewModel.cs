using Actor.Core;

namespace ActorGUI.ViewModels
{
    /// <summary>
    /// This ViewModel will handle all the Wizard pages and the logic behind the selection of each child
    /// </summary>
    public class MainViewModel : DisposableViewModel
    {
        public PageViewModel CurrentPage { get; }

        public MainViewModel(string[] args)
        {
            var cmdResult = CommandLineParametersHelper.EvaluateArgs(args);

            CurrentPage = new ActPathSelectionPageViewModel(cmdResult.InstallPath);
        }
    }
}
