namespace ActorGUI.ViewModels
{
    /// <summary>
    /// This page is shown if ACT is already installed or at the end of the wizard
    /// It shows a summary of all plugins installed and their versions
    /// with the possibility to update/modify/remove them
    /// </summary>
    public class MainPageViewModel : PageViewModel
    {
        public MainPageViewModel() 
            : base("ActorGUI")
        {
        }
    }
}