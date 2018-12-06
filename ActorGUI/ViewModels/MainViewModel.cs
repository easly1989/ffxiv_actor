using System;
using System.Collections.Generic;
using Actor.Core;

namespace ActorGUI.ViewModels
{
    public enum Page
    {
        ActPathSelection,
        PreRequisiteInstall,
        MainWizardPage,

        // the page to show at the end, or if ACT is already installed
        MainPage,
    }

    /// <summary>
    /// This ViewModel will handle all the Wizard pages and the logic behind the selection of each child
    /// </summary>
    public class MainViewModel : DisposableViewModel
    {
        private readonly CommandLineResult _cmdResult;
        
        private IEnumerable<Guid> _currentPageDisposableIds;
        private PageViewModel _currentPage;

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

            SelectPage(Page.ActPathSelection);
        }

        /// <summary>
        /// Creates and subscribes to page events based on the given <param name="pageToSelect"/>
        /// </summary>
        /// <param name="pageToSelect">The page to select</param>
        private void SelectPage(Page pageToSelect)
        {
            if(_currentPageDisposableIds != null)
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
                    CurrentPage = new PreRequisiteInstallPageViewModel();
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
