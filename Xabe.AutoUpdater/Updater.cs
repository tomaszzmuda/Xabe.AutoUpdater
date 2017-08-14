using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Xabe.AutoUpdater
{
    public class Updater : IUpdater
    {
        private readonly IVersionChecker _versionChecker;
        private readonly IReleaseProvider _releaseProvider;

        /// <inheritdoc />
        public List<string> DownloadedFiles { get; set; }

        /// <inheritdoc />
        public event EventHandler Updating;

        /// <inheritdoc />
        public event EventHandler Restarting;

        /// <summary>
        ///     Create instance of Updater with specific version checking options and provider
        /// </summary>
        /// <param name="versionChecker">Version checker</param>
        /// <param name="releaseProvider">Release provider</param>
        public Updater(IVersionChecker versionChecker, IReleaseProvider releaseProvider)
        {
            _versionChecker = versionChecker;
            _releaseProvider = releaseProvider;
        }

        /// <inheritdoc />
        public async Task<bool> IsUpdateAvaiable()
        {
            var currentVersion = new Version(await _releaseProvider.GetLatestVersionNumber());
            var installedVersion = new Version(await _versionChecker.GetInstalledVersionNumber());
            return currentVersion > installedVersion;
        }

        /// <inheritdoc />
        public void Update()
        {
            Updating(this, null);
            DownloadedFiles = _releaseProvider.DownloadCurrentVersion()
                                .Result;
            var outputDir = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly()
                                                        .Location);

            var fileNames = DownloadedFiles.Select(Path.GetFileName);

            MoveFiles(DownloadedFiles, outputDir, fileNames);

            RestartApp();
        }

        private void MoveFiles(List<string> files, string outputDir, IEnumerable<string> fileNames)
        {
            foreach (var fileName in fileNames)
            {
                var path = Path.Combine(outputDir, fileName);
                if (File.Exists(path))
                {
                    try
                    {
                        var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid()
                                                                            .ToString());
                        File.Move(path, tempPath);
                    }
                    catch (FileNotFoundException)
                    {
                    }
                }
            }

            foreach (var file in files)
            {
                var outputPath = Path.Combine(outputDir, Path.GetFileName(file));
                File.Copy(file, outputPath);
            }
        }

        /// <inheritdoc />
        public void RestartApp()
        {
            Restarting(this, null);
            var args = Environment.GetCommandLineArgs();
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    Arguments = string.Join(' ', args),
                    FileName = "dotnet",
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            Environment.Exit(0);
        }
    }
}
