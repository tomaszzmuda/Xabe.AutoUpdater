using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Octokit;

namespace Xabe.AutoUpdater
{
    /// <summary>
    ///     Provider for GitHubReleases
    /// </summary>
    public class GithubProvider : IReleaseProvider
    {
        private readonly string _appName;
        private readonly string _repositoryOwner;
        private readonly string _repositoryName;

        /// <summary>
        ///     Create instance of GithubProvider. 
        /// </summary>
        /// <param name="appName">Name of current application.</param>
        /// <param name="repositoryOwner">Github repository owner</param>
        /// <param name="repositoryName">Github repository name</param>
        public GithubProvider(string appName, string repositoryOwner, string repositoryName)
        {
            _appName = appName; 
            _repositoryOwner = repositoryOwner;
            _repositoryName = repositoryName;
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public async Task<string> GetLatestVersionNumber()
        {
            var client = new GitHubClient(new ProductHeaderValue(_appName));
            var releases = await client.Repository.Release.GetAll(_repositoryOwner, _repositoryName);
            var latest = releases[0];
            return latest.TagName;
        }

    }
}
