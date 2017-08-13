using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Octokit;

namespace Xabe.AutoUpdater
{
    public class GithubProvider : IReleaseProvider
    {
        private readonly string _appName;
        private readonly string _repositoryOwner;
        private readonly string _repositoryName;

        public GithubProvider(string appName, string repositoryOwner, string repositoryName)
        {
            _appName = appName; 
            _repositoryOwner = repositoryOwner;
            _repositoryName = repositoryName;
        }

        public async Task<List<string>> DownloadCurrentVersion()
        {
            var client = new GitHubClient(new ProductHeaderValue(_appName));
            var releases = await client.Repository.Release.GetAll(_repositoryOwner, _repositoryName);
            var latest = releases[0];
            var url = latest.Assets[0].BrowserDownloadUrl;
            var tempFile = Path.GetTempFileName();
            using (var webClient = new WebClient())
            {
                webClient.DownloadFile(url, tempFile);
            }
            var outputDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid()
                                                                 .ToString());
            System.IO.Compression.ZipFile.ExtractToDirectory(tempFile, outputDir);

            return Directory.GetFiles(outputDir, "*", SearchOption.AllDirectories).ToList();
        }

        public async Task<string> GetLatestVersionNumber()
        {
            var client = new GitHubClient(new ProductHeaderValue(_appName));
            var releases = await client.Repository.Release.GetAll(_repositoryOwner, _repositoryName);
            var latest = releases[0];
            return latest.TagName;
        }

    }
}
