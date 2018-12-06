using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Actor.Core;
using ActorGUI.Localization;

namespace ActorGUI.ViewModels
{
    public enum Page
    {
        None, // avoids the page change
        ActPathSelection,
        PreRequisiteInstall,
        MainWizardPage,

        // the page to show at the end, or if ACT is already installed
        MainPage
    }

    /// <summary>
    /// This ViewModel will handle all the Wizard pages and the logic behind the selection of each child
    /// </summary>
    public class MainViewModel : DisposableViewModel
    {
        private static readonly string DownloadPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "download");

        private readonly CommandLineResult _cmdResult;
        private readonly SystemInteractions _systemInteractions;
        private readonly WebInteractions _webInteractions;
        private readonly Component[] _components;

        private IEnumerable<Guid> _currentPageDisposableIds;
        private PageViewModel _currentPage;
        private bool _hasFatalError;

        public string Title => string.Format(Locals.MainWindow_Title, Assembly.GetExecutingAssembly().GetName().Version);
        public string FatalErrorText1 => Locals.MainView_FatalErrorText_1;
        public string FatalErrorText2 => Locals.MainView_FatalErrorText_2;

        public bool HasFatalError
        {
            get => _hasFatalError;
            private set => Set(ref _hasFatalError, value);
        }

        /// <summary>
        /// Handles the current page
        /// </summary>
        public PageViewModel CurrentPage
        {
            get => _currentPage;
            private set => Set(ref _currentPage, value);
        }

        public MainViewModel(string[] args)
        {
            _cmdResult = CommandLineParametersHelper.EvaluateArgs(args);
            _systemInteractions = new SystemInteractions();
            _webInteractions = new WebInteractions();
            _components = _webInteractions.LoadConfiguration(() => HasFatalError = true);
            
            SelectPage(Page.ActPathSelection);
        }

        /// <summary>
        /// Creates and subscribes to page events based on the given <paramref name="pageToSelect"/>
        /// </summary>
        /// <param name="pageToSelect">The page to select</param>
        private void SelectPage(Page pageToSelect)
        {
            if (pageToSelect == Page.None)
                return;

            if (_currentPageDisposableIds != null)
            {
                foreach (var id in _currentPageDisposableIds)
                {
                    TryDispose(id);
                }
            }

            var currentPageDisposableIds = new List<Guid>();
            switch (pageToSelect)
            {
                case Page.ActPathSelection:
                    CurrentPage = new ActPathSelectionPageViewModel(_cmdResult.InstallPath);
                    break;
                case Page.PreRequisiteInstall:
                    CurrentPage = new PreRequisiteInstallPageViewModel(_systemInteractions, _webInteractions, _components.Where(x => x.IsPrerequisite).OrderBy(x => x.InstallOrder), DownloadPath);
                    break;
                case Page.MainWizardPage:
                    break;
                case Page.MainPage:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(pageToSelect), pageToSelect, null);
            }

            currentPageDisposableIds.Add(AddDisposable(CurrentPage));
            currentPageDisposableIds.Add(AddDisposable(CurrentPage.WhenUndoRequested.Subscribe(x => SelectPage(CurrentPage.UndoPage))));
            currentPageDisposableIds.Add(AddDisposable(CurrentPage.WhenSkipRequested.Subscribe(x => SelectPage(CurrentPage.SkipPage))));
            currentPageDisposableIds.Add(AddDisposable(CurrentPage.WhenContinueRequested.Subscribe(x => SelectPage(CurrentPage.ContinuePage))));

            _currentPageDisposableIds = currentPageDisposableIds;
        }
    }
}
