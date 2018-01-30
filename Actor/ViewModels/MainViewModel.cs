using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Actor.Configuration;

namespace Actor.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private string _targetPath;
        
        protected sealed override async Task OnLoad()
        {
            // todo, handle a possible configuration to avoid long search
            IsLoadingCancelVisible = true;
            Config.Load();
            if(!Config.HasAnyComponent())
                FindTarget(@"C:\", "ACT.exe");
        }

        private bool FindTarget(string path, string target)
        {
            if (!IsLoading)
                return false;

            try
            {
                if (Directory.GetDirectories(path).Any(dir => FindTarget(dir, target)))
                {
                    return true;
                }

                foreach (var file in Directory.GetFiles(path))
                {
                    if (!file.Contains(target))
                        continue;

                    _targetPath = file;
                    return true;
                }
            }
            catch (Exception)
            {
                // nothing to do
            }

            return false;
        }

        public void WhenLoadingDialogIsClosing()
        {

        }
    }
}
