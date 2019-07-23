using System;
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
        public ICommand BackCommand { get; }
        public ICommand SkipCommnad { get; }

        public string NextCommandText { get; }
        public string BackCommandText { get; }
        public string SkipCommandText { get; }

        public bool ShowNextCommand => _selectedStep.CanGoForward();
        public bool ShowBackCommand => _selectedStep.CanGoBackward();
        public bool ShowSkipCommand => _selectedStep.CanSkip();

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
            SelectedStep = new MainStepViewModel();

            NextCommandText = "Next"; 
            BackCommandText = "Back"; 
            SkipCommandText = "Skip"; 

            InternalUpdateBackgroundImage();

            AddDisposable(WhenPropertyChanged.Subscribe(prop =>
            {
                if (prop != nameof(SelectedStep)) 
                    return;

                RaiseOtherPropertyChanged(() => ShowNextCommand);
                RaiseOtherPropertyChanged(() => ShowBackCommand);
                RaiseOtherPropertyChanged(() => ShowSkipCommand);

                InternalUpdateBackgroundImage();
            }));
        }

        private void InternalUpdateBackgroundImage()
        {
            BackgroundImage?.Dispose();
            var backgroundIndex = new Random().Next(0, BackgroundImages.Length);
            BackgroundImage = new BackgroundImageViewModel(BackgroundImages[backgroundIndex]);
        }

        #region Private Fields
        private StepViewModelBase _selectedStep;
        private BackgroundImageViewModel _backgroundImage;

        #endregion

        #region Background Image links
        private static readonly string[] BackgroundImages = new string []
        {
            "https://wallpaperplay.com/walls/full/4/e/4/174964.jpg",
            "https://wallpaperplay.com/walls/full/5/6/6/174965.jpg",
            "https://wallpaperplay.com/walls/full/1/3/7/174966.jpg",
            "https://wallpaperplay.com/walls/full/c/e/4/174967.jpg",
            "https://wallpaperplay.com/walls/full/a/e/1/174968.jpg",
            "https://wallpaperplay.com/walls/full/f/5/0/174969.jpg",
            "https://wallpaperplay.com/walls/full/d/b/d/174970.jpg",
            "https://wallpaperplay.com/walls/full/7/d/4/174971.jpg",
            "https://wallpaperplay.com/walls/full/a/c/0/174972.jpg",
            "https://wallpaperplay.com/walls/full/1/7/8/174973.jpg",
            "https://wallpaperplay.com/walls/full/9/4/9/174974.jpg",
            "https://wallpaperplay.com/walls/full/a/b/e/174975.jpg",
            "https://wallpaperplay.com/walls/full/c/a/5/174976.jpg",
            "https://wallpaperplay.com/walls/full/b/4/1/174977.jpg",
            "https://wallpaperplay.com/walls/full/8/a/3/174978.jpg",
            "https://wallpaperplay.com/walls/full/8/f/9/175017.jpg"
        };

        #endregion
    }
}
