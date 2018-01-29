using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Actor.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private string _targetPath;

        protected sealed override async Task OnLoad()
        {
            //var task = new Task(() => { FindTarget(@"C:\", "ACT.exe"); });
            //task.Start();
            //await task;

            FindTarget(@"C:\", "ACT.exe");
        }

        private bool FindTarget(string path, string target)
        {
            
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
    }
}
