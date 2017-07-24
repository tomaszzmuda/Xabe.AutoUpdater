using System;
using System.IO;
using System.Net;

namespace Xabe.AutoUpdater
{
    public class Updater
    {
        private readonly string _url;

        public Updater(string url)
        {
            _url = url;
        }

        public void Get()
        {
            string fileName = Path.GetTempFileName();
            using(var client = new WebClient())
            {
                client.DownloadFile(_url, fileName);
            }

            string tmpDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            System.IO.Compression.ZipFile.ExtractToDirectory(fileName, tmpDir);
        }
    }
}
