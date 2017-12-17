using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Xabe.AutoUpdater
{
    /// <summary>
    /// Provider for NugetReleases
    /// </summary>
    public class NugetProvider : IReleaseProvider
    {
        private readonly string _appName;
        protected string _PackageId;
        protected string _NugetApi;

        public NugetProvider(string pAppName, string nugetApiUrl, string pPackageId)
        {
            _appName = pAppName;
            _PackageId = pPackageId;
            _NugetApi = string.IsNullOrEmpty(nugetApiUrl) ? "https://www.nuget.org/api/v2/package/" : nugetApiUrl;
        }

        public Task<string> GetLatestVersionNumber()
        {
            var t = Task.Run(() =>
            {
                string version = "0.0.0.0";
                var url = new Uri(string.Concat(_NugetApi, _PackageId));
                WebRequest webRequest = WebRequest.Create(url);
                using (WebResponse webResponse = webRequest.GetResponse())
                {
                    // File Fomat : appName.Major.Minor.Build.Revision.nupkg
                    version = Path.GetFileNameWithoutExtension(webResponse.ResponseUri.Segments[2].Replace(_appName.ToLower() + ".", ""));
                }
                return version;
            });
            t.Wait();
            return t;
        }

        public Task<List<string>> DownloadCurrentVersion()
        {
            var t = Task.Run(async () =>
            {
                var url = new Uri(string.Concat(_NugetApi, _PackageId));
                var tempFile = Path.GetTempFileName();
                if (File.Exists(tempFile))
                    File.Delete(tempFile);

                using (WebClient myWebClient = new WebClient())
                {
                    myWebClient.DownloadFile(url, tempFile);
                    if (File.Exists(tempFile))
                    {
                        var outputDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
                        ZipFile.ExtractToDirectory(tempFile, outputDir);

                        var sourceFolder = Directory.GetDirectories(Path.Combine(outputDir, "lib")).FirstOrDefault();
                        if (!string.IsNullOrEmpty(sourceFolder))
                            return Directory.GetFiles(sourceFolder, "*", SearchOption.AllDirectories)
                            .ToList();
                    }
                }
                return null;
            });
            t.Wait();
            return t;
        }
    }
}
