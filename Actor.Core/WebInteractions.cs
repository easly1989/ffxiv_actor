using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Newtonsoft.Json;

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
        public async Task DownloadAsync(string from, string to)
        {
            if (string.IsNullOrWhiteSpace(from) || !Uri.IsWellFormedUriString(from, UriKind.Absolute)) throw new ArgumentNullException(nameof(from));
            if (string.IsNullOrWhiteSpace(to)) throw new ArgumentNullException(nameof(to));

            var taskCompletionSource = new TaskCompletionSource<WebInteractionsResultType>();
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
                    taskCompletionSource.SetResult(x.Result);
                    webClient.Dispose();
                    disposable.Dispose();
                }));

            webClient.DownloadFileAsync(uriFrom, to);
            await taskCompletionSource.Task;
        }

        /// <summary>
        /// Download synchronously from the given url to the given path.
        /// </summary>
        /// <param name="from">The download url</param>
        /// <param name="to">The path where the downloaded file will be stored</param>
        /// <param name="onStart">The action to execute before the download starts (can be left null)</param>
        /// <param name="onProgress">The action to execute every time the download progress changes (can be left null)</param>
        /// <param name="onComplete">The action to execute when the download is completed (can be left null)</param>
        /// <returns>The bundle containing the file if the result was successfull</returns>
        public WebInteractionsBundle Download(string from, string to, Action onStart = null, Action<DownloadProgressChangedEventArgs> onProgress = null, Action onComplete = null)
        {
            if (string.IsNullOrWhiteSpace(from) || !Uri.IsWellFormedUriString(from, UriKind.Absolute)) throw new ArgumentNullException(nameof(from));
            if (string.IsNullOrWhiteSpace(to)) throw new ArgumentNullException(nameof(to));

            var uriFrom = new Uri(from, UriKind.Absolute);
            var webClient = new WebClient();

            onStart?.Invoke();
            // webClient will not be disposed before disposable, ever.
            // ReSharper disable AccessToDisposedClosure
            var disposable = Observable.FromEventPattern<DownloadProgressChangedEventHandler, DownloadProgressChangedEventArgs>(
                    handler => webClient.DownloadProgressChanged += handler,
                    handler => webClient.DownloadProgressChanged -= handler)
                .Select(result => result.EventArgs)
                .Subscribe(x => onProgress?.Invoke(x));
            // ReSharper restore AccessToDisposedClosure

            var downloadTask = webClient.DownloadFileTaskAsync(uriFrom, to);
            downloadTask.Wait();

            // the onComplete can be invoked here as the download is synchronous.
            onComplete?.Invoke();
            var resultType = !downloadTask.IsCanceled && !downloadTask.IsFaulted
                ? WebInteractionsResultType.Success
                : WebInteractionsResultType.Fail;

            disposable.Dispose();
            webClient.Dispose();

            return new WebInteractionsBundle(resultType, new FileInfo(to));
        }

        /// <summary>
        /// Parses the browser_download_url from one of the asset of a GitHub repository
        /// </summary>
        /// <param name="gitHubApiUrl">The GitHub repository API url</param>
        /// <param name="assetIndex">The asset index to extrapolate</param>
        /// <param name="onStart">The action to execute before the actual parsing starts (can be left null)</param>
        /// <param name="onError">The action to execute when an error occurs (can be left null)</param>
        /// <returns>The browser_download_url for the defined assets, or string.Empty if there was an error</returns>
        public string ParseAssetFromGitHub(string gitHubApiUrl, int assetIndex, Action onStart = null, Action onError = null)
        {
            if (string.IsNullOrWhiteSpace(gitHubApiUrl) || !Uri.IsWellFormedUriString(gitHubApiUrl, UriKind.Absolute)) throw new ArgumentNullException(nameof(gitHubApiUrl));

            try
            {
                onStart?.Invoke();

                var downloadString = DownloadString(gitHubApiUrl);
                dynamic json = JsonConvert.DeserializeObject(downloadString);
                var githubUrl = json.assets[assetIndex].browser_download_url;
                return githubUrl;
            }
            catch (Exception)
            {
                onError?.Invoke();
                return string.Empty;
            }
        }

        /// <summary>
        /// Loads the configuration file from githubusercontent.com based on current architecture
        /// </summary>
        /// <param name="onError">The action to invoke in case of errors</param>
        /// <returns>An array of Components if deserialized correctly, an empty array otherwise</returns>
        public Component[] LoadConfiguration(Action onError = null)
        {
            const string componentsUrl = "https://raw.githubusercontent.com/easly1989/ffxiv_actor/master/components_x64.json";

            try
            {
                var downloadString = DownloadString(componentsUrl);
                var result = JsonConvert.DeserializeObject<Component[]>(downloadString);
                return result;
            }
            catch (Exception)
            {
                onError?.Invoke();
                return new Component[] {};
            }
        }

        /// <summary>
        /// Download a string from the specified url
        /// </summary>
        /// <param name="url">The link to the string that needs to be donwloaded</param>
        /// <returns>The downloaded string</returns>
        public static string DownloadString(string url)
        {
            var webClient = new WebClient();
            webClient.Headers.Add("user-agent", "avoid 403");
            var downloadString = webClient.DownloadString(url);
            webClient.Dispose();
            return downloadString;
        }
    }
}