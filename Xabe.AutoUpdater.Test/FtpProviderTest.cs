using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Xabe.AutoUpdater.Test
{
    [TestClass]
    public class FtpProviderTest
    {
        private readonly List<string> _receivedEvents = new List<string>();
        private Version _lastVersion;
        private Version _installedVersion;

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
            Assert.IsTrue(_receivedEvents.Contains("CheckedLatestVersionNumber"), "Last version event received");
            Assert.IsTrue(_lastVersion != null, $"Last Version : {_lastVersion}");

            Assert.IsTrue(_receivedEvents.Contains("CheckedInstalledVersionNumber"), "Installed version event received");
            Assert.IsTrue(_installedVersion != null, $"Installed Version : {_installedVersion}");

            Assert.IsTrue(_lastVersion > _installedVersion, $"New version available, waiting");
            Assert.IsTrue(_receivedEvents.Contains("Updating"), "Updating event received");
        }

        private void Updater_CheckedLatestVersionNumber(object sender, System.Version version)
        {
			_receivedEvents.Add("CheckedLatestVersionNumber");
            _lastVersion = version;
        }

        private void Updater_CheckedInstalledVersionNumber(object sender, System.Version version)
        {
			_receivedEvents.Add("CheckedInstalledVersionNumber");
            _installedVersion = version;
        }

        private void Updater_Updating(object sender, System.EventArgs e)
        {
			_receivedEvents.Add("Updating");
        }
    }
}
