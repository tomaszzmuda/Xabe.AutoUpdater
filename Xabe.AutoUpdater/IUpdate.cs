using System.Collections.Generic;
using System.Threading.Tasks;

namespace Xabe.AutoUpdater
{
    public interface IUpdate
    {
        Task<string> GetCurrentVersion();
        Task<string> GetInstalledVersion();
        Task<List<string>> DownloadCurrentVersion();
        void RestartApp();
    }
}
