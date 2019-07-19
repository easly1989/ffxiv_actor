using System;
using System.Reactive.Linq;
using System.Reflection;
using Actor.UI.Common;

namespace ActorWizard.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public string WindowTitle => $"Actor Wizard ~ v{Assembly.GetExecutingAssembly().GetName().Version}";

        public BackgroundImageViewModel BackgroundImage
        {
            get => _backgroundImage;
            set => Set(ref _backgroundImage, value);
        }

        private int _deleteMe;
        public MainViewModel()
        {
            _deleteMe = 0;
            Observable.Interval(TimeSpan.FromSeconds(5)).Subscribe(_ =>
            {
                BackgroundImage?.Dispose();

                PostOnUI(__ =>
                {
                    BackgroundImage = _deleteMe % 2 == 0
                        ? new BackgroundImageViewModel("https://i.pinimg.com/originals/b9/fa/05/b9fa05f7aa2e9ded35d8e29ea79557a3.jpg") 
                        : new BackgroundImageViewModel("https://wallpaperplay.com/walls/full/c/e/4/174967.jpg");
                });
                
                _deleteMe++;
            });
        }

        #region Private Fields
        private BackgroundImageViewModel _backgroundImage;
        #endregion
    }
}
