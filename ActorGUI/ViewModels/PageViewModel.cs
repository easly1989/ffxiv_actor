using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows.Input;
using ActorGUI.Localization;
using GalaSoft.MvvmLight.CommandWpf;

namespace ActorGUI.ViewModels
{
    public abstract class PageViewModel : DisposableViewModel
    {
        private readonly ISubject<Unit> _continueSubject;
        private readonly ISubject<Unit> _undoSubject;
        private readonly ISubject<Unit> _skipSubject;

        public abstract Page UndoPage { get; }
        public abstract Page SkipPage { get; }
        public abstract Page ContinuePage { get; }

        public string UndoText => Locals.PageCommon_UndoText;
        public string ContinueText => Locals.PageCommon_ContinueText;
        public string SkipText => Locals.PageCommon_SkipText;
        public IObservable<Unit> WhenContinueRequested => _continueSubject.AsObservable();
        public IObservable<Unit> WhenUndoRequested => _undoSubject.AsObservable();
        public IObservable<Unit> WhenSkipRequested => _skipSubject.AsObservable();

        public string Title { get; }
        public ICommand ContinueCommnad { get; }
        public ICommand SkipCommand { get; }
        public ICommand UndoCommand { get; }

        protected PageViewModel(string title)
        {
            _undoSubject = new Subject<Unit>();
            _continueSubject = new Subject<Unit>();
            _skipSubject = new Subject<Unit>();

            Title = title;

            ContinueCommnad = new RelayCommand(Continue, CanContinue);
            UndoCommand = new RelayCommand(Undo, CanUndo);
            SkipCommand = new RelayCommand(Skip, CanSkip);
        }

        private bool CanSkip()
        {
            return OnCanSkip();
        }

        /// <summary>
        /// The base call can be avoided, this method does nothing on its own
        /// </summary>
        /// <returns>Always true by default (the binded button will be always enabled)</returns>
        protected virtual bool OnCanSkip()
        {
            return true;
        }

        /// <summary>
        /// Is called from the SkipCommand.Execute
        /// Fires an Rx event to notify WhenSkipRequested
        /// </summary>
        private void Skip()
        {
            OnSkip();
            _skipSubject.OnNext(Unit.Default);
        }

        /// <summary>
        /// The base call can be avoided, this method does nothing on its own
        /// </summary>
        protected virtual void OnSkip()
        {
        }

        private bool CanUndo()
        {
            return OnCanUndo();
        }

        /// <summary>
        /// The base call can be avoided, this method does nothing on its own
        /// </summary>
        /// <returns>Always false by default (the binded button will be always disabled)</returns>
        protected virtual bool OnCanUndo()
        {
            return false;
        }

        /// <summary>
        /// Is called from the UndoCommand.Execute
        /// Fires an Rx event to notify WhenUndoRequested
        /// </summary>
        private void Undo()
        {
            OnUndo();
            _undoSubject.OnNext(Unit.Default);
        }

        /// <summary>
        /// The base call can be avoided, this method does nothing on its own
        /// </summary>
        protected virtual void OnUndo()
        {
        }

        private bool CanContinue()
        {
            return OnCanContinue();
        }

        /// <summary>
        /// The base call can be avoided, this method does nothing on its own
        /// </summary>
        /// <returns>Always true by default (the binded button will be always enabled)</returns>
        protected virtual bool OnCanContinue()
        {
            return true;
        }

        /// <summary>
        /// Is called from the ContinueCommand.Execute
        /// Fires an Rx event to notify WhenContinueRequested
        /// </summary>
        private void Continue()
        {
            OnContinue();
            _continueSubject.OnNext(Unit.Default);
        }

        /// <summary>
        /// The base call can be avoided, this method does nothing on its own
        /// </summary>
        protected virtual void OnContinue()
        {
        }
    }
}