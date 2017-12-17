using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Octokit;
using FileMode = System.IO.FileMode;

namespace Xabe.AutoUpdater
{
    /// <summary>
    ///     Provider for FtpReleases
    /// </summary>
    public class FtpProvider : IReleaseProvider
    {
        private readonly string _ftpHosts;
        private readonly string _ftpLogin;
        private readonly string _ftpPassword;
        private readonly string _ftpFolder;
        private readonly bool _ftpPassiveMode;

        /// <summary>
        ///     Create instance of FtpProvider. 
        /// </summary>
        /// <param name="pHosts">FTP HOST.</param>
        /// <param name="pLogin">FTP Account Login</param>
        /// <param name="pPassword">FTP Account Password</param>
        /// <param name="pPassiveMode">FTP Passive Mode</param>
        /// <param name="pFolder">FTP Release Folder (optional)</param>
        public FtpProvider(string pHosts, string pLogin, string pPassword, bool pPassiveMode, string pFolder = null)
        {
            _ftpHosts = pHosts;
            _ftpLogin = pLogin;
            _ftpPassiveMode = pPassiveMode;
            _ftpPassword = pPassword;
            _ftpFolder = string.IsNullOrEmpty(pFolder) ? "/" : pFolder;
        }

        /// <inheritdoc />
        public async Task<List<string>> DownloadCurrentVersion()
        {
            return await Task.Run(async () =>
            {
                List<string> lstFiles = new List<string>();
                var client = BuildClient();
                var latestVersion = await GetLatestVersionNumber();
                if (latestVersion == "0.0.0.0") return lstFiles;
                var remoteFile = Path.Combine(_ftpFolder, $"{latestVersion}.zip");
                var tempFile = Path.GetTempFileName();
                client.Download(remoteFile, tempFile);
                if (!File.Exists(tempFile)) return lstFiles;
                var outputDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
                ZipFile.ExtractToDirectory(tempFile, outputDir);
                lstFiles = Directory.GetFiles(outputDir, "*", SearchOption.AllDirectories)
                    .ToList();
                return lstFiles;
            });
        }

        /// <inheritdoc />
        public async Task<string> GetLatestVersionNumber()
        {
            return await Task.Run(() =>
            {
                string version = "0.0.0.0";
                var client = BuildClient();
                var remoteDirList = client.DirectoryListSimple(_ftpFolder);
                if (!remoteDirList.Any())
                    return version;

                var lastestVersionFile = remoteDirList.Where(w => !string.IsNullOrEmpty(w)).OrderBy(x =>  new Version(Path.GetFileNameWithoutExtension(x))).Max();
                if (lastestVersionFile == null) return version;
                return Path.GetFileNameWithoutExtension(lastestVersionFile);
            });
        }

        private FtpClient BuildClient()
        {
            return new FtpClient
            {
                Host = _ftpHosts.StartsWith("ftp:") ? _ftpHosts : $"ftp://{_ftpHosts}",
                User = _ftpLogin,
                Pass = _ftpPassword,
                Passive = _ftpPassiveMode,
                KeepAlive = true
            };
        }

    }

}
