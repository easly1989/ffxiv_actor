using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace ActorGUI.ViewModels
{
    public abstract class PageViewModel : DisposableViewModel
    {
        private readonly ISubject<Unit> _continueSubject;
        private readonly ISubject<Unit> _undoSubject;

        public IObservable<Unit> WhenContinueRequested => _continueSubject.AsObservable();
        public IObservable<Unit> WhenUndoRequested => _undoSubject.AsObservable();

        public string Title { get; }

        protected PageViewModel(string title)
        {
            _undoSubject = new Subject<Unit>();
            _continueSubject = new Subject<Unit>();

            Title = title;
        }

        protected void RequestContinue()
        {
            _continueSubject.OnNext(Unit.Default);
        }

        protected void RequestUndo()
        {
            _undoSubject.OnNext(Unit.Default);
        }
    }
}