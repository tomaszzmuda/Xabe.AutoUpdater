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
        private readonly string _appName;
        private readonly string _ftpHosts;
        private readonly string _ftpLogin;
        private readonly string _ftpPassword;
        private readonly string _ftpFolder;
        private readonly bool _ftpPassiveMode;

        /// <summary>
        ///     Create instance of FtpProvider. 
        /// </summary>
        /// <param name="appName">Name of current application.</param>
        /// <param name="pHosts">FTP HOST.</param>
        /// <param name="pLogin">FTP Account Login</param>
        /// <param name="pPassword">FTP Account Password</param>
        /// <param name="pPassiveMode">FTP Passive Mode</param>
        /// <param name="pFolder">FTP Release Folder (optional)</param>
        public FtpProvider(string appName, string pHosts, string pLogin, string pPassword, bool pPassiveMode, string pFolder = null)
        {
            _appName = appName;
            _ftpHosts = pHosts;
            _ftpLogin = pLogin;
            _ftpPassiveMode = pPassiveMode;
            _ftpPassword = pPassword;
            _ftpFolder = string.IsNullOrEmpty(pFolder) ? "/" : pFolder;
        }

        /// <inheritdoc />
        public Task<List<string>> DownloadCurrentVersion()
        {
            var t = Task.Run(async () =>
            {
                var client = BuildClient();
                var latestVersion = await GetLatestVersionNumber();
                if (!string.IsNullOrEmpty(latestVersion))
                {
                    var remoteFile = string.Concat(_ftpFolder, $"{_appName}.{latestVersion}.zip");
                    var tempFile = Path.GetTempFileName();
                    if (File.Exists(tempFile))
                        File.Delete(tempFile);
                    client.Download(remoteFile, tempFile);
                    if (File.Exists(tempFile))
                    {
                        var outputDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
                        ZipFile.ExtractToDirectory(tempFile, outputDir);
                        return Directory.GetFiles(outputDir, "*", SearchOption.AllDirectories)
                            .ToList();
                    }
                }
                return null;
            });
            t.Wait();
            return t;
        }

        /// <inheritdoc />
        public Task<string> GetLatestVersionNumber()
        {
            var t = Task.Run(() =>
            {
                string version = "0.0.0.0";
                var client = BuildClient();
                var remoteDirList = client.DirectoryListSimple(_ftpFolder);
                if (remoteDirList != null && remoteDirList.Any())
                {
                    var filteredItems = remoteDirList.ToList().Where(p => p.StartsWith(_appName)).ToList();
                    if (filteredItems.Any())
                    {
                        var lastItem = filteredItems.OrderByDescending(p => p).FirstOrDefault();
                        if (lastItem != null)
                        {
                            // File Fomat : appName.Major.Minor.Build.Revision.zip
                            version = Path.GetFileNameWithoutExtension(lastItem.Replace(_appName + ".", ""));
                        }
                    }
                }
                return version;
            });
            t.Wait();
            return t;
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

    #region Ftp Client

    public class FtpClient
    {
        public string Host { get; set; }

        public string User { get; set; }

        public string Pass { get; set; }

        public bool Passive { get; set; }

        public bool KeepAlive { get; set; }  

        private FtpWebRequest _ftpRequest;
        private FtpWebResponse _ftpResponse;
        private Stream _ftpStream;
        private int _bufferSize = 2048;

        /* Construct Object */
        public FtpClient()
        {
        }

        /* Download File */
        public void Download(string remoteFile, string localFile)
        {
            /* Create an FTP Request */
            _ftpRequest = (FtpWebRequest)WebRequest.Create(Host + "/" + remoteFile);
            /* Log in to the FTP Server with the User Name and Password Provided */
            _ftpRequest.Credentials = new NetworkCredential(User, Pass);
            /* When in doubt, use these options */
            _ftpRequest.UseBinary = true;
            _ftpRequest.UsePassive = Passive;
            _ftpRequest.KeepAlive = KeepAlive;
            /* Specify the Type of FTP Request */
            _ftpRequest.Method = WebRequestMethods.Ftp.DownloadFile;
            /* Establish Return Communication with the FTP Server */
            _ftpResponse = (FtpWebResponse)_ftpRequest.GetResponse();
            /* Get the FTP Server's Response Stream */
            _ftpStream = _ftpResponse.GetResponseStream();
            /* Open a File Stream to Write the Downloaded File */
            FileStream localFileStream = new FileStream(localFile, FileMode.Create);
            /* Buffer for the Downloaded Data */
            byte[] byteBuffer = new byte[_bufferSize];
            int bytesRead = _ftpStream.Read(byteBuffer, 0, _bufferSize);
            /* Download the File by Writing the Buffered Data Until the Transfer is Complete */
            try
            {
                while (bytesRead > 0)
                {
                    localFileStream.Write(byteBuffer, 0, bytesRead);
                    bytesRead = _ftpStream.Read(byteBuffer, 0, _bufferSize);
                }
            }
            catch (Exception ex) { Console.WriteLine(ex.ToString()); }
            /* Resource Cleanup */
            localFileStream.Close();
            _ftpStream.Close();
            _ftpResponse.Close();
            _ftpRequest = null;
        }

        /* Upload File */
        public void Upload(string remoteFile, string localFile)
        {
            /* Create an FTP Request */
            _ftpRequest = (FtpWebRequest)FtpWebRequest.Create(Host + "/" + remoteFile);
            /* Log in to the FTP Server with the User Name and Password Provided */
            _ftpRequest.Credentials = new NetworkCredential(User, Pass);
            /* When in doubt, use these options */
            _ftpRequest.UseBinary = true;
            _ftpRequest.UsePassive = Passive;
            _ftpRequest.KeepAlive = KeepAlive;
            /* Specify the Type of FTP Request */
            _ftpRequest.Method = WebRequestMethods.Ftp.UploadFile;
            /* Establish Return Communication with the FTP Server */
            _ftpStream = _ftpRequest.GetRequestStream();
            /* Open a File Stream to Read the File for Upload */
            FileStream localFileStream = new FileStream(localFile, FileMode.Create);
            /* Buffer for the Downloaded Data */
            byte[] byteBuffer = new byte[_bufferSize];
            int bytesSent = localFileStream.Read(byteBuffer, 0, _bufferSize);
            /* Upload the File by Sending the Buffered Data Until the Transfer is Complete */
            try
            {
                while (bytesSent != 0)
                {
                    _ftpStream.Write(byteBuffer, 0, bytesSent);
                    bytesSent = localFileStream.Read(byteBuffer, 0, _bufferSize);
                }
            }
            catch (Exception ex) { Console.WriteLine(ex.ToString()); }
            /* Resource Cleanup */
            localFileStream.Close();
            _ftpStream.Close();
            _ftpRequest = null;
        }

        /* Upload File */
        public void UploadByteArray(string remoteFile, Byte[] byteArray)
        {
            /* Create an FTP Request */
            _ftpRequest = (FtpWebRequest)FtpWebRequest.Create(Host + "/" + remoteFile);
            /* Log in to the FTP Server with the User Name and Password Provided */
            _ftpRequest.Credentials = new NetworkCredential(User, Pass);
            /* When in doubt, use these options */
            _ftpRequest.UseBinary = true;
            _ftpRequest.UsePassive = Passive;
            _ftpRequest.KeepAlive = KeepAlive;
            /* Specify the Type of FTP Request */
            _ftpRequest.Method = WebRequestMethods.Ftp.UploadFile;
            /* Establish Return Communication with the FTP Server */
            _ftpStream = _ftpRequest.GetRequestStream();
            /* Open a File Stream to Read the File for Upload */
            Stream localFileStream = new MemoryStream(byteArray);
            //FileStream localFileStream = new FileStream(new File, FileMode.Create);
            /* Buffer for the Downloaded Data */
            byte[] byteBuffer = new byte[_bufferSize];
            int bytesSent = localFileStream.Read(byteBuffer, 0, _bufferSize);
            /* Upload the File by Sending the Buffered Data Until the Transfer is Complete */
            try
            {
                while (bytesSent != 0)
                {
                    _ftpStream.Write(byteBuffer, 0, bytesSent);
                    bytesSent = localFileStream.Read(byteBuffer, 0, _bufferSize);
                }
            }
            catch (Exception ex) { Console.WriteLine(ex.ToString()); }
            /* Resource Cleanup */
            localFileStream.Close();
            _ftpStream.Close();
            _ftpRequest = null;
        }
        /* Delete File */
        public void Delete(string deleteFile)
        {
            /* Create an FTP Request */
            _ftpRequest = (FtpWebRequest)WebRequest.Create(Host + "/" + deleteFile);
            /* Log in to the FTP Server with the User Name and Password Provided */
            _ftpRequest.Credentials = new NetworkCredential(User, Pass);
            /* When in doubt, use these options */
            _ftpRequest.UseBinary = true;
            _ftpRequest.UsePassive = Passive;
            _ftpRequest.KeepAlive = KeepAlive;
            /* Specify the Type of FTP Request */
            _ftpRequest.Method = WebRequestMethods.Ftp.DeleteFile;
            /* Establish Return Communication with the FTP Server */
            _ftpResponse = (FtpWebResponse)_ftpRequest.GetResponse();
            /* Resource Cleanup */
            _ftpResponse.Close();
            _ftpRequest = null;
        }

        /* Rename File */
        public void Rename(string currentFileNameAndPath, string newFileName)
        {
            /* Create an FTP Request */
            _ftpRequest = (FtpWebRequest)WebRequest.Create(Host + "/" + currentFileNameAndPath);
            /* Log in to the FTP Server with the User Name and Password Provided */
            _ftpRequest.Credentials = new NetworkCredential(User, Pass);
            /* When in doubt, use these options */
            _ftpRequest.UseBinary = true;
            _ftpRequest.UsePassive = Passive;
            _ftpRequest.KeepAlive = KeepAlive;
            /* Specify the Type of FTP Request */
            _ftpRequest.Method = WebRequestMethods.Ftp.Rename;
            /* Rename the File */
            _ftpRequest.RenameTo = newFileName;
            /* Establish Return Communication with the FTP Server */
            _ftpResponse = (FtpWebResponse)_ftpRequest.GetResponse();
            /* Resource Cleanup */
            _ftpResponse.Close();
            _ftpRequest = null;
        }

        /* Create a New Directory on the FTP Server */
        public void CreateDirectory(string newDirectory)
        {
            /* Create an FTP Request */
            _ftpRequest = (FtpWebRequest)WebRequest.Create(Host + "/" + newDirectory);
            /* Log in to the FTP Server with the User Name and Password Provided */
            _ftpRequest.Credentials = new NetworkCredential(User, Pass);
            /* When in doubt, use these options */
            _ftpRequest.UseBinary = true;
            _ftpRequest.UsePassive = Passive;
            _ftpRequest.KeepAlive = true;
            /* Specify the Type of FTP Request */
            _ftpRequest.Method = WebRequestMethods.Ftp.MakeDirectory;
            /* Establish Return Communication with the FTP Server */
            _ftpResponse = (FtpWebResponse)_ftpRequest.GetResponse();
            /* Resource Cleanup */
            _ftpResponse.Close();
            _ftpRequest = null;
        }

        /* Get the Date/Time a File was Created */
        public string GetFileCreatedDateTime(string fileName)
        {
            /* Create an FTP Request */
            _ftpRequest = (FtpWebRequest)FtpWebRequest.Create(Host + "/" + fileName);
            /* Log in to the FTP Server with the User Name and Password Provided */
            _ftpRequest.Credentials = new NetworkCredential(User, Pass);
            /* When in doubt, use these options */
            _ftpRequest.UseBinary = true;
            _ftpRequest.UsePassive = Passive;
            _ftpRequest.KeepAlive = KeepAlive;
            /* Specify the Type of FTP Request */
            _ftpRequest.Method = WebRequestMethods.Ftp.GetDateTimestamp;
            /* Establish Return Communication with the FTP Server */
            _ftpResponse = (FtpWebResponse)_ftpRequest.GetResponse();
            /* Establish Return Communication with the FTP Server */
            _ftpStream = _ftpResponse.GetResponseStream();
            /* Get the FTP Server's Response Stream */
            StreamReader ftpReader = new StreamReader(_ftpStream);
            /* Store the Raw Response */
            string fileInfo = null;
            /* Read the Full Response Stream */
            try { fileInfo = ftpReader.ReadToEnd(); }
            catch (Exception ex) { Console.WriteLine(ex.ToString()); }
            /* Resource Cleanup */
            ftpReader.Close();
            _ftpStream.Close();
            _ftpResponse.Close();
            _ftpRequest = null;
            /* Return File Created Date Time */
            return fileInfo;
        }

        /* Get the Size of a File */
        public string GetFileSize(string fileName)
        {
            /* Create an FTP Request */
            _ftpRequest = (FtpWebRequest)FtpWebRequest.Create(Host + "/" + fileName);
            /* Log in to the FTP Server with the User Name and Password Provided */
            _ftpRequest.Credentials = new NetworkCredential(User, Pass);
            /* When in doubt, use these options */
            _ftpRequest.UseBinary = true;
            _ftpRequest.UsePassive = Passive;
            _ftpRequest.KeepAlive = KeepAlive;
            /* Specify the Type of FTP Request */
            _ftpRequest.Method = WebRequestMethods.Ftp.GetFileSize;
            /* Establish Return Communication with the FTP Server */
            _ftpResponse = (FtpWebResponse)_ftpRequest.GetResponse();
            /* Establish Return Communication with the FTP Server */
            _ftpStream = _ftpResponse.GetResponseStream();
            /* Get the FTP Server's Response Stream */
            StreamReader ftpReader = new StreamReader(_ftpStream);
            /* Store the Raw Response */
            string fileInfo = null;
            /* Read the Full Response Stream */
            try { while (ftpReader.Peek() != -1) { fileInfo = ftpReader.ReadToEnd(); } }
            catch (Exception ex) { Console.WriteLine(ex.ToString()); }
            /* Resource Cleanup */
            ftpReader.Close();
            _ftpStream.Close();
            _ftpResponse.Close();
            _ftpRequest = null;
            /* Return File Size */
            return fileInfo;
        }

        /* List Directory Contents File/Folder Name Only */
        public string[] DirectoryListSimple(string directory)
        {
            /* Create an FTP Request */
            _ftpRequest = (FtpWebRequest)FtpWebRequest.Create(Host + "/" + directory);
            /* Log in to the FTP Server with the User Name and Password Provided */
            _ftpRequest.Credentials = new NetworkCredential(User, Pass);
            /* When in doubt, use these options */
            _ftpRequest.UseBinary = true;
            _ftpRequest.UsePassive = Passive;
            _ftpRequest.KeepAlive = KeepAlive;
            /* Specify the Type of FTP Request */
            _ftpRequest.Method = WebRequestMethods.Ftp.ListDirectory;
            /* Establish Return Communication with the FTP Server */
            _ftpResponse = (FtpWebResponse)_ftpRequest.GetResponse();
            /* Establish Return Communication with the FTP Server */
            _ftpStream = _ftpResponse.GetResponseStream();
            /* Get the FTP Server's Response Stream */
            StreamReader ftpReader = new StreamReader(_ftpStream);
            /* Store the Raw Response */
            string directoryRaw = null;
            /* Read Each Line of the Response and Append a Pipe to Each Line for Easy Parsing */
            try
            {
                while (ftpReader.Peek() != -1)
                {
                    directoryRaw += ftpReader.ReadLine() + "|";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            /* Resource Cleanup */
            ftpReader.Close();
            _ftpStream.Close();
            _ftpResponse.Close();
            _ftpRequest = null;
            /* Return the Directory Listing as a string Array by Parsing 'directoryRaw' with the Delimiter you Append (I use | in This Example) */
            if (directoryRaw != null)
            {
                string[] directoryList = directoryRaw.Split("|".ToCharArray());
                return directoryList;
            }
            /* Return an Empty string Array if an Exception Occurs */
            return new[] { "" };
        }

        /* List Directory Contents in Detail (Name, Size, Created, etc.) */
        public string[] DirectoryListDetailed(string directory)
        {
            /* Create an FTP Request */
            _ftpRequest = (FtpWebRequest)FtpWebRequest.Create(Host + "/" + directory);
            /* Log in to the FTP Server with the User Name and Password Provided */
            _ftpRequest.Credentials = new NetworkCredential(User, Pass);
            /* When in doubt, use these options */
            _ftpRequest.UseBinary = true;
            _ftpRequest.UsePassive = Passive;
            _ftpRequest.KeepAlive = KeepAlive;
            /* Specify the Type of FTP Request */
            _ftpRequest.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
            /* Establish Return Communication with the FTP Server */
            _ftpResponse = (FtpWebResponse)_ftpRequest.GetResponse();
            /* Establish Return Communication with the FTP Server */
            _ftpStream = _ftpResponse.GetResponseStream();
            /* Get the FTP Server's Response Stream */
            StreamReader ftpReader = new StreamReader(_ftpStream);
            /* Store the Raw Response */
            string directoryRaw = null;
            /* Read Each Line of the Response and Append a Pipe to Each Line for Easy Parsing */
            try { while (ftpReader.Peek() != -1) { directoryRaw += ftpReader.ReadLine() + "|"; } }
            catch (Exception ex) { Console.WriteLine(ex.ToString()); }
            /* Resource Cleanup */
            ftpReader.Close();
            _ftpStream.Close();
            _ftpResponse.Close();
            _ftpRequest = null;
            /* Return the Directory Listing as a string Array by Parsing 'directoryRaw' with the Delimiter you Append (I use | in This Example) */
                if (directoryRaw != null)
                {
                    string[] directoryList = directoryRaw.Split("|".ToCharArray()); return directoryList;
                }
            /* Return an Empty string Array if an Exception Occurs */
            return new[] { "" };
        }
    }

    #endregion

}
