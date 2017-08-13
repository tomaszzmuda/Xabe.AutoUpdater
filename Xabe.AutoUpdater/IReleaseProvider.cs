using System.Collections.Generic;
using System.Threading.Tasks;

namespace Xabe.AutoUpdater
{
    public interface IReleaseProvider
    {
        Task<string> GetLatestVersionNumber();
        Task<List<string>> DownloadCurrentVersion();
    }
}