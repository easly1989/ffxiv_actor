using System;
using System.IO;
using System.Linq;
using Actor.Core;
using ActorGUI.Localization;
using GalaSoft.MvvmLight;

namespace ActorGUI.ViewModels
{
    public class PreRequisiteInstallPageViewModel : PageViewModel
    {
        private readonly SystemInteractions _systemInteractions;
        private readonly WebInteractions _webInteractions;
        private readonly IOrderedEnumerable<Component> _preReqComponents;
        private readonly string _downloadPath;

        private bool _isDownloading;
        private int _downloadPercentage;
        private string _componentName;

        public override Page UndoPage => Page.ActPathSelection;
        public override Page SkipPage => Page.MainWizardPage;
        public override Page ContinuePage => Page.MainWizardPage;

        public string PreRequisiteInstallHelp1 => Locals.PreRequisiteInstall_Help_1;
        public string PreRequisiteInstallHelp2 => Locals.PreRequisiteInstall_Help_2;
        public string PreRequisiteInstallHelp3 => Locals.PreRequisiteInstall_Help_3;
        public string PreRequisiteInstallHelp4 => Locals.PreRequisiteInstall_Help_4;
        public string PreRequisiteInstallHelp5 => Locals.PreRequisiteInstall_Help_5;

        public bool IsDownloading
        {
            get => _isDownloading;
            private set => Set(ref _isDownloading, value);
        }

        public string DownloadText
        {
            get => _componentName;
            private set => Set(ref _componentName, value);
        }

        public int DownloadPercentage
        {
            get => _downloadPercentage;
            set => Set(ref _downloadPercentage, value);
        }

        public PreRequisiteInstallPageViewModel(SystemInteractions systemInteractions, WebInteractions webInteractions, IOrderedEnumerable<Component> preReqComponents, string downloadPath) 
            : base(Locals.PreRequisiteInstall_Title)
        {
            _systemInteractions = systemInteractions;
            _webInteractions = webInteractions;
            _preReqComponents = preReqComponents;
            _downloadPath = downloadPath;
        }

        protected override void OnContinue()
        {
            _systemInteractions.KillProcess("Advanced Combat Tracker");

            if (!Directory.Exists(_downloadPath))
                Directory.CreateDirectory(_downloadPath);

            foreach (var component in _preReqComponents)
            {
                if(_systemInteractions.CheckVersion(component.VersionCheck, component.Version))
                    continue;

                DownloadText = string.Format(Locals.PreRequisiteInstall_DownloadText, component.Name);

                var bundle = _webInteractions.Download(component.Url, Path.Combine(_downloadPath, component.FileName), 
                    () => IsDownloading = true, // on start
                    args => DownloadPercentage = args.ProgressPercentage, // on progress
                    () => IsDownloading = false); // on complete

                if(bundle.Result == WebInteractionsResultType.Fail)
                    continue; // todo: handle error

                _systemInteractions.Install(bundle.DownloadedFile.FullName, component.InstallArguments);
            }
        }

        protected override bool OnCanUndo()
        {
            return true;
        }
    }
}