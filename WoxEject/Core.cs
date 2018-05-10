using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management;

namespace WoxEject {
    public class Core {
        public static bool EjectDrive(USBDeviceInfo drive) {
            return true;
        }

        public static List<USBDeviceInfo> ListUSBDevices() {
            List<USBDeviceInfo> devices = new List<USBDeviceInfo>();
            ManagementObjectCollection col;
            //string query = @"select * from Win32_USBControllerDevice";
            //string query = @"select * from Win32_USBHub";
            string query = @"select * from Win32_DiskDrive where InterfaceType = 'USB'";
            using (var searcher = new ManagementObjectSearcher(query)) {
                col = searcher.Get();
            }

            foreach (var device in col) {
                // query for partitions
                string deviceID = device.Properties["DeviceID"].Value.ToString();

                var deviceIDs = GetPartitionsDeviceIDs(deviceID);
                List<string> allLetters = new List<string>();
                foreach (var partitionDeviceID in deviceIDs) {
                    var letters = GetParitionDriveLetters(partitionDeviceID);
                    foreach (string letter in letters) {
                        allLetters.Add(letter);
                    }
                }

                var deviceInfo = new USBDeviceInfo {
                    Name = device.Properties["Name"].Value.ToString(),
                    Caption = device.Properties["Caption"].Value.ToString(),
                    DeviceID = deviceID,
                    PnPDeviceID = device.Properties["PNPDeviceID"].Value.ToString(),
                    Description = device.Properties["Description"].Value.ToString(),
                    Status = device.Properties["Status"].Value.ToString(),
                    DriveLetters = allLetters
                };

                devices.Add(deviceInfo);
            }
            col.Dispose();
            return devices;
        }

        static List<string> GetPartitionsDeviceIDs(string deviceID) {
            ManagementObjectCollection partitions;
            List<string> deviceIDs = new List<string>();
            string query = string.Format("associators of {{Win32_DiskDrive.DeviceID='{0}'}} where AssocClass = Win32_DiskDriveToDiskPartition", deviceID);
            using (var searcher = new ManagementObjectSearcher(query)) {
                partitions = searcher.Get();
            }

            foreach (var parition in partitions) {
                deviceIDs.Add(parition.Properties["DeviceID"].Value.ToString());
            }

            partitions.Dispose();
            return deviceIDs;
        }

        static List<string> GetParitionDriveLetters(string partitionDeviceID) {
            ManagementObjectCollection disks;
            List<string> letters = new List<string>();
            string query = string.Format("associators of {{Win32_DiskPartition.DeviceID='{0}'}} where AssocClass = Win32_LogicalDiskToPartition", partitionDeviceID);
            using (var searcher = new ManagementObjectSearcher(query)) {
                disks = searcher.Get();
            }

            foreach (var disk in disks) {
                letters.Add(disk.Properties["Name"].Value.ToString());
            }

            disks.Dispose();
            return letters;
        }
    }
}
