using System;
using System.Windows.Media.Imaging;
using Actor.UI.Common;

namespace ActorWizard.ViewModels
{
    public class BackgroundImageViewModel : ViewModelBase
    {
        public BitmapImage Source { get; }

        public BackgroundImageViewModel(string absoluteUriPath)
        {
            Source = new BitmapImage();
            Source.BeginInit();
            Source.UriSource = new Uri(absoluteUriPath, UriKind.Absolute);
            Source.EndInit();
        }
    }
}
