using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Xabe.AutoUpdater
{
    /// <summary>
    ///     Version event handler
    /// </summary>
    /// <param name="sender">Object raised event</param>
    /// <param name="version">Version</param>
    /// <returns></returns>
    public delegate void VersionEventHandler(object sender, Version version);

    public interface IUpdater
    {
        /// <summary>
        ///     List of all downloaded files
        /// </summary>
        List<string> DownloadedFiles { get; set; }

        /// <summary>
        ///     Occurs before Update
        /// </summary>
        event EventHandler Updating;

        /// <summary>
        ///     Occurs before restart
        /// </summary>
        event EventHandler Restarting;

        /// <summary>
        ///     Occurs after checked current version number
        /// </summary>
        event VersionEventHandler CheckedInstalledVersionNumber;

        /// <summary>
        ///     Occurs after check latest version number
        /// </summary>
        event VersionEventHandler CheckedLatestVersionNumber;

        /// <summary>
        ///     Tells about avaiable updates.
        /// </summary>
        /// <returns>True if any update is avaiable</returns>
        Task<bool> IsUpdateAvaiable();

        /// <summary>
        ///     Start updating process
        /// </summary>
        void Update();

        /// <summary>
        ///     Restart application
        /// </summary>
        void RestartApp();
    }
}