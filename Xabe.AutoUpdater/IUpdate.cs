using System.Collections.Generic;

namespace Xabe.AutoUpdater
{
    public interface IUpdate
    {
        string GetCurrentVersion();
        string GetInstalledVersion();
        List<string> DownloadCurrentVersion();
        void RestartApp();
    }
}
