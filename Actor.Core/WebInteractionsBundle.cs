using System.IO;

namespace Actor.Core
{
    public class WebInteractionsBundle
    {
        public WebInteractionsResultType Result { get; }
        public FileInfo DownloadedFile { get; }

        public WebInteractionsBundle(WebInteractionsResultType result, FileInfo downloadedFile)
        {
            Result = result;
            DownloadedFile = downloadedFile;
        }
    }
}