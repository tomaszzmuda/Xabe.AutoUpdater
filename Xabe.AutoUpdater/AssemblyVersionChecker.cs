using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Xabe.AutoUpdater
{
    public class AssemblyVersionChecker : IVersionChecker
    {
        /// <inheritdoc />
        public async Task<string> GetInstalledVersionNumber()
        {
            Version version = Assembly.GetEntryAssembly()
                                      .GetName()
                                      .Version;
            string versionNumber = string.Join('.', version.Major, version.Minor, version.Build);
            return versionNumber;
        }
    }
}
