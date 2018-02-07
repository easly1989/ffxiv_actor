using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Xml.Linq;

namespace Actor.Core
{
    public static class ActConfigurationHelper
    {
        private static readonly IDictionary<string, bool> Plugins;
        private static string _installPath;

        static ActConfigurationHelper()
        {
            Plugins = new Dictionary<string, bool>();
        }

        /// <summary>
        /// Handles the plugin in the ACT configuration
        /// </summary>
        /// <param name="pluginPath">The path where the plugin is installed</param>
        /// <param name="isEnabled">Default is true, enables the plugin inside ACT</param>
        public static void AddPlugin(string pluginPath, bool isEnabled = true)
        {
            if (Plugins.ContainsKey(pluginPath))
                return;

            Plugins.Add(pluginPath, isEnabled);
        }

        /// <summary>
        /// Saves the configuration from the given url to the desired path
        /// </summary>
        /// <param name="from">Url of the configuration</param>
        /// <param name="to">Path where the configuration will be saved</param>
        /// <param name="isAct">The action to invoke when an error occurs</param>
        /// <param name="onDuplicatd">The action to invoke in case the configuration file already exists, the return value should be
        ///                           true if you want to overwrite an existing configuration, false if you want to keep it</param>
        /// <param name="onError">The action to invoke when an error occurs</param>
        public static void SaveConfiguration(string from, string to, bool isAct = false, Func<Unit, bool> onDuplicatd = null, Action<Exception> onError = null)
        {
            try
            {
                if (to.StartsWith("\\"))
                    to = _installPath + to;
                else if (to.StartsWith("%"))
                    to = SystemInteractions.TryExpandSystemVariable(to);

                // ensures that the directory exists
                // ReSharper disable AssignNullToNotNullAttribute
                Directory.CreateDirectory(Path.GetDirectoryName(to));
                // ReSharper restore AssignNullToNotNullAttribute

                if (File.Exists(to) && onDuplicatd != null && !onDuplicatd.Invoke(Unit.Default))
                    return;

                SystemInteractions.RenameFile(to);

                // for the kagerou db, which is not a xml file :\
                if (from.EndsWith("localstorage"))
                {
                    var wi = new WebInteractions();
                    wi.Download(from, to);
                    return;
                }

                var config = WebInteractions.DownloadString(from);
                var document = XDocument.Parse(config);

                if (isAct)
                {
                    var pluginsNode = document.Descendants("ActPlugins").FirstOrDefault();
                    if (pluginsNode == null)
                        throw new Exception($"The document {from} is missing the node \"ActPlugins\"\nContact the developer!");

                    foreach (var plugin in Plugins)
                    {
                        var pluginElement = new XElement("Plugin");
                        pluginElement.Add(new XAttribute("Enabled", plugin.Value));
                        pluginElement.Add(new XAttribute("Path", SystemInteractions.TryExpandSystemVariable(plugin.Key)));
                        pluginsNode.Add(pluginElement);
                    }
                }

                document.Save(to);
            }
            catch (Exception e)
            {
                onError?.Invoke(e);
            }
        }

        /// <summary>
        /// Updates the current path where ACT is installed
        /// </summary>
        /// <param name="installPath">The path where ACT is installed</param>
        public static void UpdateActInstallPath(string installPath)
        {
            _installPath = installPath;
        }
    }
}
