using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Xabe.AutoUpdater
{
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