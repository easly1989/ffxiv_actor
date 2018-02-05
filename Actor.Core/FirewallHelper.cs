using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NetFwTypeLib;

namespace Actor.Core
{
    /// <summary>
    /// Allows basic access to the windows firewall API.
    /// This can be used to add an exception to the windows firewall
    /// exceptions list, so that our programs can continue to run merrily
    /// even when the nasty windows firewall is running.
    ///
    /// Please note: It is not enforced here, but it might be a good idea
    /// to actually prompt the user before messing with their firewall settings,
    /// just as a matter of politeness.
    /// 
    /// This class was taken from "Paul Campbell, IdiomSoftware"
    /// 
    /// There is no sign of any kind of License, so I'm gonna edit and add to my 
    /// sources under the GPLv3 as the rest of my code
    /// </summary>
    public class FirewallHelper
    {
        private static FirewallHelper _instance;
        private readonly INetFwMgr _fwMgr;

        public static FirewallHelper Instance
        {
            get
            {
                lock (typeof(FirewallHelper))
                {
                    return _instance ?? (_instance = new FirewallHelper());
                }
            }
        }

        public bool IsFirewallInstalled => _fwMgr?.LocalPolicy?.CurrentProfile != null;
        public bool IsFirewallEnabled => IsFirewallInstalled && _fwMgr.LocalPolicy.CurrentProfile.FirewallEnabled;
        public bool AppAuthorizationsAllowed => IsFirewallInstalled && !_fwMgr.LocalPolicy.CurrentProfile.ExceptionsNotAllowed;

        private FirewallHelper()
        {
            // Get the type of HNetCfg.FwMgr, or null if an error occurred
            var fwMgrType = Type.GetTypeFromProgID("HNetCfg.FwMgr", false);
            if (fwMgrType == null)
                return;

            _fwMgr = null;

            try
            {
                _fwMgr = (INetFwMgr)Activator.CreateInstance(fwMgrType);
            }
            catch (Exception)
            {
                // In all other circumnstances, fwMgr is null.
            }
        }

        /// <summary>
        /// Adds an application to the list of authorized applications.
        /// If the application is already authorized, does nothing.
        /// </summary>
        /// <param name="applicationFullPath">The full path to the application executable. This cannot be blank, and cannot be a relative path.</param>
        /// <param name="appName">This is the name of the application, purely for display puposes in the Microsoft Security Center.</param>
        /// <exception cref="ArgumentNullException">When applicationFullPath is null or blank OR when appName is null or blank
        /// OR applicationFullPath contains invalid path characters OR applicationFullPath is not an absolute path.</exception>
        /// <exception cref="FileNotFoundException">applicationFullPath doesn't exists.</exception>
        /// <exception cref="FirewallHelperException">If the firewall is not installed OR If the firewall does not allow specific application 'exceptions' OR
        /// Due to an exception in COM this method could not create thenecessary COM types.</exception>
        public void GrantAuthorization(string applicationFullPath, string appName)
        {
            if (string.IsNullOrWhiteSpace(applicationFullPath))
                throw new ArgumentNullException($"Value for {nameof(applicationFullPath)} is not valid.");
            if (string.IsNullOrWhiteSpace(appName))
                throw new ArgumentNullException($"Value for {nameof(appName)} is not valid.");
            if (applicationFullPath.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
                throw new ArgumentException($"{nameof(applicationFullPath)} must not contain invalid path characters.");
            if (!Path.IsPathRooted(applicationFullPath))
                throw new ArgumentException($"{nameof(applicationFullPath)} is not an absolute path.");
            if (!File.Exists(applicationFullPath))
                throw new FileNotFoundException("File does not exists", applicationFullPath);
            if (!IsFirewallInstalled)
                throw new FirewallHelperException("Cannot grant authorization: Firewall is not installed.");
            if (!AppAuthorizationsAllowed)
                throw new FirewallHelperException("Application exceptions are not allowed.");

            if (HasAuthorization(applicationFullPath))
                return;

            // Get the type of HNetCfg.FwMgr, or null if an error occurred
            var authAppType = Type.GetTypeFromProgID("HNetCfg.FwAuthorizedApplication", false);

            INetFwAuthorizedApplication appInfo = null;

            if (authAppType != null)
            {
                try
                {
                    appInfo = (INetFwAuthorizedApplication)Activator.CreateInstance(authAppType);
                }
                catch (Exception)
                {
                    // In all other circumnstances, appInfo is null.
                }
            }

            if (appInfo == null)
                throw new FirewallHelperException("Could not grant authorization: can't create INetFwAuthorizedApplication instance.");

            appInfo.Name = appName;
            appInfo.ProcessImageFileName = applicationFullPath;
            _fwMgr.LocalPolicy.CurrentProfile.AuthorizedApplications.Add(appInfo);
        }


        /// <summary>
        /// Removes an application from the list of authorized applications.
        /// Note: the specified application must exist or a FileNotFound
        /// exception will be thrown.
        /// If the specified application exists but does not currently has an
        /// authorization, this method will do nothing.
        /// </summary>
        /// <param name="applicationFullPath">The full path to the application executable. This cannot be blank, and cannot be a relative path.</param>
        /// <exception cref="ArgumentNullException">When applicationFullPath is null or blank OR applicationFullPath contains invalid path characters 
        /// OR applicationFullPath is not an absolute path.</exception>
        /// <exception cref="FileNotFoundException">applicationFullPath doesn't exists.</exception>
        /// <exception cref="FirewallHelperException">If the firewall is not installed.</exception>
        public void RemoveAuthorization(string applicationFullPath)
        {
            if (string.IsNullOrWhiteSpace(applicationFullPath))
                throw new ArgumentNullException($"Value for {nameof(applicationFullPath)} is not valid.");
            if (applicationFullPath.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
                throw new ArgumentException($"{nameof(applicationFullPath)} must not contain invalid path characters.");
            if (!Path.IsPathRooted(applicationFullPath))
                throw new ArgumentException($"{nameof(applicationFullPath)} is not an absolute path.");
            if (!File.Exists(applicationFullPath))
                throw new FileNotFoundException("File does not exists", applicationFullPath);
            if (!IsFirewallInstalled)
                throw new FirewallHelperException("Cannot remove authorization: Firewall is not installed.");

            if (HasAuthorization(applicationFullPath))
            {
                _fwMgr.LocalPolicy.CurrentProfile.AuthorizedApplications.Remove(applicationFullPath);
            }
        }


        /// <summary>
        /// Returns whether an application is in the list of authorized applications.
        /// Note: if the file does not exists, this throws a FileNotFound exception.
        /// </summary>
        /// <param name="applicationFullPath">The full path to the application executable. This cannot be blank, and cannot be a relative path.</param>
        /// <returns>True if the application given as argument already has authorizations</returns>
        /// <exception cref="ArgumentNullException">When applicationFullPath is null or blank OR applicationFullPath contains invalid path characters 
        /// OR applicationFullPath is not an absolute path.</exception>
        /// <exception cref="FileNotFoundException">applicationFullPath doesn't exists.</exception>
        /// <exception cref="FirewallHelperException">If the firewall is not installed.</exception>
        public bool HasAuthorization(string applicationFullPath)
        {
            if (string.IsNullOrWhiteSpace(applicationFullPath))
                throw new ArgumentNullException($"Value for {nameof(applicationFullPath)} is not valid.");
            if (applicationFullPath.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
                throw new ArgumentException($"{nameof(applicationFullPath)} must not contain invalid path characters.");
            if (!Path.IsPathRooted(applicationFullPath))
                throw new ArgumentException($"{nameof(applicationFullPath)} is not an absolute path.");
            if (!File.Exists(applicationFullPath))
                throw new FileNotFoundException("File does not exists", applicationFullPath);
            if (!IsFirewallInstalled)
                throw new FirewallHelperException("Cannot check authorization: Firewall is not installed.");

            return GetAuthorizedAppPaths().Any(appName => string.Equals(appName.ToLower(), applicationFullPath.ToLower(), StringComparison.InvariantCultureIgnoreCase));
        }

        /// <summary>
        /// Retrieves a collection of paths to applications that are authorized.
        /// </summary>
        /// <returns>The list of the paths of all the apps already authorized in the windows firewall</returns>
        /// <exception cref="FirewallHelperException">If the Firewall is not installed.</exception>
        public IEnumerable<string> GetAuthorizedAppPaths()
        {
            if (!IsFirewallInstalled)
                throw new FirewallHelperException("Cannot retrieve paths: Firewall is not installed.");

            var list = new List<string>();
            foreach (INetFwAuthorizedApplication app in _fwMgr.LocalPolicy.CurrentProfile.AuthorizedApplications)
            {
                list.Add(app.ProcessImageFileName);
            }

            return list;
        }
    }
}
