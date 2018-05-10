using Microsoft.VisualStudio.TestTools.UnitTesting;
using WoxEject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoxEject.Tests {
    [TestClass()]
    public class CoreTests {
        [TestMethod()]
        public void EjectDriveTest() {
            
        }

        [TestMethod()]
        public void ListUSBDrivesTest() {
            var devices = Core.ListUSBDevices();
            foreach (var device in devices) {
                Console.WriteLine(device);
            }
        }
    }
}
