using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Actor.Core
{
    public class WebInteractions
    {
        private readonly ISubject<DownloadProgressChangedEventArgs> _downloadProgressSubject;
        private readonly ISubject<WebInteractionsBundle> _downloadCompletedSubject;

        public IObservable<DownloadProgressChangedEventArgs> WhenDownloadProgressChanged => _downloadProgressSubject.AsObservable();
        public IObservable<WebInteractionsBundle> WhenDownloadCompleted => _downloadCompletedSubject.AsObservable();

        public WebInteractions()
        {
            _downloadProgressSubject = new Subject<DownloadProgressChangedEventArgs>();
            _downloadCompletedSubject = new Subject<WebInteractionsBundle>();
        }

        /// <summary>
        /// Download asynchronously from the given url to the given path
        /// </summary>
        /// <param name="from">The download url</param>
        /// <param name="to">The path where the downloaded file will be stored</param>>
        public void DownloadAsync(string from, string to)
        {
            if (string.IsNullOrWhiteSpace(from) || !Uri.IsWellFormedUriString(from, UriKind.Absolute)) throw new ArgumentNullException(nameof(from));
            if (string.IsNullOrWhiteSpace(to)) throw new ArgumentNullException(nameof(to));

            var uriFrom = new Uri(from, UriKind.Absolute);
            var disposable = new CompositeDisposable();
            var webClient = new WebClient();

            disposable.Add(Observable.FromEventPattern<DownloadProgressChangedEventHandler, DownloadProgressChangedEventArgs>(
                    handler => webClient.DownloadProgressChanged += handler,
                    handler => webClient.DownloadProgressChanged -= handler)
                .Select(result => result.EventArgs)
                .Subscribe(x => _downloadProgressSubject.OnNext(x)));

            disposable.Add(Observable.FromEventPattern<AsyncCompletedEventHandler, AsyncCompletedEventArgs>(
                    handler => webClient.DownloadFileCompleted += handler,
                    handler => webClient.DownloadFileCompleted -= handler)
                .Select(result =>
                {
                    var resultType = !result.EventArgs.Cancelled && result.EventArgs.Error == null
                        ? WebInteractionsResultType.Success
                        : WebInteractionsResultType.Fail;

                    return new WebInteractionsBundle(resultType, new FileInfo(to));
                })
                .Subscribe(x =>
                {
                    _downloadCompletedSubject.OnNext(x);
                    webClient.Dispose();
                    disposable.Dispose();
                }));

            webClient.DownloadFileAsync(uriFrom, to);
        }

        /// <summary>
        /// Download synchronously from the given url to the given path.
        /// </summary>
        /// <param name="from">The download url</param>
        /// <param name="to">The path where the downloaded file will be stored</param>
        /// <param name="onStart">The action to execute before the download starts (can be left null)</param>
        /// <param name="onProgress">The action to execute every time the download progress changes (can be left null)</param>
        /// <param name="onComplete">The action to execute when the download is completed (can be left null)</param>
        public void Download(string from, string to, Action onStart = null, Action<DownloadProgressChangedEventArgs> onProgress = null, Action onComplete = null)
        {
            if (string.IsNullOrWhiteSpace(from) || !Uri.IsWellFormedUriString(from, UriKind.Absolute)) throw new ArgumentNullException(nameof(from));
            if (string.IsNullOrWhiteSpace(to)) throw new ArgumentNullException(nameof(to));

            var uriFrom = new Uri(from, UriKind.Absolute);
            var disposable = new CompositeDisposable();
            var webClient = new WebClient();

            onStart?.Invoke();

            disposable.Add(Observable.FromEventPattern<DownloadProgressChangedEventHandler, DownloadProgressChangedEventArgs>(
                    handler => webClient.DownloadProgressChanged += handler,
                    handler => webClient.DownloadProgressChanged -= handler)
                .Select(result => result.EventArgs)
                .Subscribe(x => onProgress?.Invoke(x)));

            disposable.Add(Observable.FromEventPattern<AsyncCompletedEventHandler, AsyncCompletedEventArgs>(
                handler => webClient.DownloadFileCompleted += handler,
                handler => webClient.DownloadFileCompleted -= handler)
                .Subscribe(x =>
                {
                    webClient.Dispose();
                    disposable.Dispose();
                }));

            var downloadTask = webClient.DownloadFileTaskAsync(uriFrom, to);
            downloadTask.Wait();

            // the onComplete can be invoked here as the download is synchronous.
            onComplete?.Invoke();
        }
    }
}