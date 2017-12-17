using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Xabe.AutoUpdater.Test
{
    [TestClass]
    public class FtpProviderTest
    {
        [TestMethod]
        public async Task Run()
        {
            IUpdater updater = new AutoUpdater.Updater(new AssemblyVersionChecker(),
                new FtpProvider("ftp.itoo.me", "updates", "updates", false, "Test"));
            updater.Updating += Updater_Updating;
            updater.CheckedInstalledVersionNumber += Updater_CheckedInstalledVersionNumber;
            updater.Restarting += Updater_Restarting;
            updater.CheckedLatestVersionNumber += Updater_CheckedLatestVersionNumber;
            if (await updater.IsUpdateAvaiable())
                updater.Update();
        }

        private void Updater_Restarting(object sender, System.EventArgs e)
        {
            Console.WriteLine("Restarting ...");
        }

        private void Updater_CheckedLatestVersionNumber(object sender, System.Version version)
        {
            Console.WriteLine($"Lastest Version : {version}");
        }

        private void Updater_CheckedInstalledVersionNumber(object sender, System.Version version)
        {
            Console.WriteLine($"Installed Version : {version}");
        }

        private void Updater_Updating(object sender, System.EventArgs e)
        {
            Console.WriteLine("Updating ...");
        }
    }
}
