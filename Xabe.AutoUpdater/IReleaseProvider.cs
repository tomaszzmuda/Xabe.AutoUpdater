using System.Collections.Generic;
using System.Threading.Tasks;

namespace Xabe.AutoUpdater
{
    public interface IReleaseProvider
    {
        /// <summary>
        ///     Get latest version number
        /// </summary>
        /// <returns>String with version number eg. 1.0.1</returns>
        Task<string> GetLatestVersionNumber();

        /// <summary>
        ///     Download current version. 
        /// </summary>
        /// <returns>Aall files included in this version</returns>
        Task<List<string>> DownloadCurrentVersion();
    }
}