using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Actor.UI.Common
{
    public abstract class ViewModelBase : INotifyPropertyChanged, IDisposable
    {
        public bool IsDisposed { get; private set; }
        public IObservable<string> WhenPropertyChanged { get; }

        public static bool InvokeRequired => Thread.CurrentThread.ManagedThreadId != _uiContextThreadId;
        
        // ReSharper disable InconsistentNaming
        public static SynchronizationContext UIContext
        {
            get => _uiContext;
            set
            {
                _uiContext = value;
                _uiContextThreadId = Thread.CurrentThread.ManagedThreadId;
            }
        }
        // ReSharper restore InconsistentNaming

        public void Dispose()
        {
            Dispose(true);
        }

        protected ViewModelBase()
        {
            if (UIContext == null)
                throw new Exception("ERROR: UIContext Must be initialized by static property on ViewModelBase");

            _disposables = new List<IDisposable>();

            WhenPropertyChanged = Observable
                .FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>(
                    h => _propertyChanged += h,
                    h => _propertyChanged -= h)
                .Select(x => x.EventArgs.PropertyName);
        }

        ~ViewModelBase()
        {
            Dispose(false);
        }

        /// <summary>
        /// Raises the PropertyChanged event for the property defined with that <paramref name="propertyName"/>
        /// </summary>
        /// <remarks>Must be used in the property Set method</remarks>
        /// <param name="propertyName">Leave empty, as it takes the value using <c>CallerMemberName</c></param>
        protected void RaiseCurrentPropertyChanged([CallerMemberName] string propertyName = "")
        {
            RaisePropertyChangedInternal(propertyName);
        }

        /// <summary>
        /// Raises the PropertyChanged event of a property from a different point of the code (and not the Set method of the property itself)
        /// <example> 
        /// This sample shows how to call the <see cref="RaiseOtherPropertyChanged{TProperty}"/> method.
        /// <code>
        /// class TestClass 
        /// {
        ///     public int PropertyToRaise { get; }
        /// 
        ///     public void Method()
        ///     {
        ///         // do things..
        ///         RaiseOtherPropertyChanged(() => PropertyToRaise);
        ///     }
        /// }
        /// </code>
        /// </example>
        /// </summary>
        protected void RaiseOtherPropertyChanged<TProperty>(Expression<Func<TProperty>> propertyExpression)
        {
            var propertyName = ExtractPropertyName(propertyExpression);
            RaisePropertyChangedInternal(propertyName);
        }

        /// <summary>
        /// Raises the PropertyChanged event for ALL the property of the current ViewModel
        /// </summary>
        /// <see cref="http://msdn.microsoft.com/en-us/library/system.componentmodel.inotifypropertychanged.propertychanged(v=vs.110).aspx"/>
        protected void RaiseAllPropertiesChanged()
        {
            RaisePropertyChangedInternal(string.Empty);
        }

        /// <summary>
        /// Tries to set the given <paramref name="value"/> to the <paramref name="backingField"/> and
        /// if the value actually changed, raises the PropertyChanged event for the <paramref name="propertyName"/>
        /// </summary>
        /// <remarks>Must be used in the property Set method</remarks>
        /// <param name="backingField">The backing field of the property</param>
        /// <param name="value">The value you want to set to the property</param>
        /// <param name="propertyName">Leave empty, as it takes the value using <c>CallerMemberName</c></param>
        /// <returns><c>True</c> if the value changed (and raises the PropertyChanged event), <c>False</c> otherwise.</returns>
        protected bool Set<T>(ref T backingField, T value, [CallerMemberName] string propertyName = "")
        {
            return SetInternal(ref backingField, value, propertyName);
        }

        /// <summary>
        /// Override to handle collection clears and set null disposed object of the current viewmodel
        /// </summary>
        protected virtual void OnSetNullObjectReferences()
        {
        }

        /// <summary>
        /// Override to dispose those object that could not be disposed automatically using the AddDisposable
        /// </summary>
        protected virtual void OnDisposeOwnedObject()
        {
        }

        /// <summary>
        /// Adds the given <paramref name="disposable"/> to the collection of the disposables of this viewmodel
        /// All this disposables will be Disposed with this viewmodel
        /// </summary>
        /// <param name="disposable">The disposable to add</param>
        protected internal void AddDisposable(IDisposable disposable)
        {
            if (IsDisposed)
                return;
            if (!_disposables.Contains(disposable))
                _disposables.Add(disposable);
        }

        /// <summary>
        /// Removes the given <paramref name="disposable"/> from the collection of the disposables of this viewmodel
        /// </summary>
        /// <param name="disposable">The disposable to remove</param>
        protected void RemoveDisposable(IDisposable disposable)
        {
            if (IsDisposed)
                return;
            _disposables.Remove(disposable);
        }

        /// <summary>
        /// Dispatches an asynchronous message to UI context
        /// </summary>
        // ReSharper disable InconsistentNaming
        protected void PostOnUI(SendOrPostCallback action, object state = null)
        {
            if (InvokeRequired)
                UIContext.Post(action, state);
            else
                action(state);
        }
        // ReSharper restore InconsistentNaming

        /// <summary>
        /// Dispatches a synchronous message to UI context
        /// </summary>
        // ReSharper disable InconsistentNaming
        protected void SendOnUI(SendOrPostCallback action, object state = null)
        {
            if (InvokeRequired)
                UIContext.Send(action, state);
            else
                action(state);
        }
        // ReSharper restore InconsistentNaming

        private void RaisePropertyChangedInternal(string propertyName)
        {
            _propertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private static string ExtractPropertyName<T>(Expression<Func<T>> propertyExpression)
        {
            var memberExpression = (MemberExpression)propertyExpression.Body;
            return memberExpression.Member.Name;
        }

        private bool SetInternal<T>(ref T field, T value, string propertyName)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;
            RaisePropertyChangedInternal(propertyName);

            return true;
        }

        private void Dispose(bool disposing)
        {
            if (IsDisposed)
                return;

            if (disposing)
            {
                foreach (var disp in _disposables)
                    disp.Dispose();
                // Dispose di tutti gli oggetti di cui sono owner
                OnDisposeOwnedObject();
            }

            OnSetNullObjectReferences();

            if (disposing)
            {
                GC.SuppressFinalize(this);
            }
            IsDisposed = true;
        }

        #region INotifyPropertyChanged explicit implementation
        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
        {
            add => _propertyChanged += value;
            remove => _propertyChanged -= value;
        }

        // ReSharper disable InconsistentNaming
        private event PropertyChangedEventHandler _propertyChanged;
        // ReSharper restore InconsistentNaming
        #endregion

        #region Private Fields
        private static int _uiContextThreadId = -1;
        private static SynchronizationContext _uiContext;

        private readonly List<IDisposable> _disposables;
        #endregion
    }
}
