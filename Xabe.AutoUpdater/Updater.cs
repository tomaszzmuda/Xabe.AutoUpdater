using System;
using System.IO;
using System.Linq;

namespace Xabe.AutoUpdater
{
    public class Updater<T> where T : IUpdate, new()
    {
        private readonly T _updater;

        public Updater()
        {
            _updater = new T();
        }

        public bool CheckForUpdate()
        {
            return true;
        }

        public void Update()
        {
            var files = _updater.DownloadCurrentVersion();
            var outputDir = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly()
                                                        .Location);

            var fileNames = files.Select(Path.GetFileName);

            foreach(var fileName in fileNames)
            {
                var path = Path.Combine(outputDir, fileName);
                if(File.Exists(path))
                {
                    try
                    {
                        var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid()
                                                                            .ToString());
                        File.Move(path, tempPath);
                    }
                    catch(FileNotFoundException)
                    {
                    }
                }
            }

            foreach(var file in files)
            {
                var outputPath = Path.Combine(outputDir, Path.GetFileName(file));
                File.Copy(file, outputPath);
            }

            _updater.RestartApp();
        }
    }
}
