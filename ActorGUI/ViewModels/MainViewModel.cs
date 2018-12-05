using ReactiveUI;

namespace ActorGUI.ViewModels
{
    public class MainViewModel : ReactiveObject
    {
        public string Title { get; }

        public MainViewModel(string title)
        {
            Title = title;
        }

    }
}
