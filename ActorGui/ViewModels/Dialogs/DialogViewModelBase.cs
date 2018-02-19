using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace ActorGui.ViewModels.Dialogs
{
    public abstract class DialogViewModelBase<TResult> : ViewModelBase
    {
        private readonly ISubject<TResult> _saveSubject;
        private readonly ISubject<Unit> _cancelSubject;

        public IObservable<TResult> WhenSaveRequested => _saveSubject.AsObservable();
        public IObservable<Unit> WhenCancelRequested => _cancelSubject.AsObservable();

        public bool ShowSave { get; }
        public bool ShowCancel { get; }
        public bool ShowMessage => !string.IsNullOrWhiteSpace(Message);

        public string Message { get; }
        public string SaveText { get; }
        public string SaveHint { get; }
        public string CancelText { get; }
        public string CancelHint { get; }

        public ICommand SaveCommnad { get; }
        public ICommand CancelCommand { get; }

        protected DialogViewModelBase(
            string message,
            string saveText = "Save",
            string saveHint = "Save",
            string cancelText = "Cancel",
            string cancelHint = "Cancel",
            bool showSave = true,
            bool showCancel = false)
        {
            Message = message;
            SaveText = saveText;
            SaveHint = saveHint;
            CancelText = cancelText;
            CancelHint = cancelHint;
            ShowSave = showSave;
            ShowCancel = showCancel;

            _saveSubject = new Subject<TResult>();
            _cancelSubject = new Subject<Unit>();

            SaveCommnad = new RelayCommand(
                InternalSave,
                OnCanSave);

            CancelCommand = new RelayCommand(
                InternalCancel,
                OnCanCancel);
        }

        private void InternalCancel()
        {
            OnCancel();
            _cancelSubject.OnNext(Unit.Default);
        }

        private void InternalSave()
        {
            var result = OnSave();
            _saveSubject.OnNext(result);
        }

        protected virtual TResult OnSave()
        {
            return default(TResult);
        }

        protected virtual bool OnCanSave()
        {
            return true;
        }

        protected virtual void OnCancel()
        {
        }

        protected virtual bool OnCanCancel()
        {
            return true;
        }
    }
}