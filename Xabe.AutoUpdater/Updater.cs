using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;

namespace Xabe.AutoUpdater
{
    public class Updater
    {
        private readonly IVersionChecker _versionChecker;
        private readonly IReleaseProvider _releaseProvider;

        /// <summary>
        ///     List of all downloaded files
        /// </summary>
        public List<string> DownloadedFiles;

        /// <summary>
        ///     Occurs before Update
        /// </summary>
        public event EventHandler Updating;

        /// <summary>
        ///     Occurs before restart
        /// </summary>
        public event EventHandler Restarting;

        public Updater(IVersionChecker versionChecker, IReleaseProvider releaseProvider)
        {
            _versionChecker = versionChecker;
            _releaseProvider = releaseProvider;
        }

        public async Task<bool> IsUpdateAvaiable()
        {
            var currentVersion = new Version(await _releaseProvider.GetLatestVersionNumber());
            var installedVersion = new Version(await _versionChecker.GetInstalledVersionNumber());
            return currentVersion > installedVersion;
        }

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

        private static void MoveFiles(System.Collections.Generic.List<string> files, string outputDir, System.Collections.Generic.IEnumerable<string> fileNames)
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

        private void RestartApp()
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
