using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wox.Plugin;

namespace WoxEject
{
    public class WoxEject : IPlugin {
        public void Init(PluginInitContext context) {
            Console.WriteLine("[WoxEject.Init]");
        }

        public List<Result> Query(Query query) {
            Console.WriteLine("[WoxEject.Query]");
            var devices = Core.ListUSBDevices();
            List<Result> results = new List<Result>();
            foreach (var device in devices) {
                results.Add(new DriveResult(device.Caption, device.DriveLettersToString()));
            }
            return results;
        }
    }
}
