using Microsoft.VisualStudio.TestTools.UnitTesting;
using WoxEject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WoxEject.Tests {
    [TestClass()]
    public class CoreTests {
        [TestMethod()]
        public void EjectDriveTest() {
            var devices = Core.ListUSBDevices();
            Core.EjectDrive(devices[0]);
        }

        [TestMethod()]
        public void ListUSBDrivesTest() {
            var devices = Core.ListUSBDevices();
            foreach (var device in devices) {
                Console.WriteLine(device);
            }
        }

        [TestMethod()]
        public void NotifyIconTest() {
            NotifyIcon icon = new NotifyIcon();
            string title = "Safe To Remove Hardware";
            string body = string.Format("Device '{0}' can now be safely removed from the computer", "haha");
            icon.Visible = true;
            icon.Icon = System.Drawing.SystemIcons.Information;
            icon.ShowBalloonTip(1000, title, body, ToolTipIcon.Info);
        }
    }
}
