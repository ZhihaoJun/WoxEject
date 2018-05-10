using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management;

namespace WoxEject {
    public class Core {
        const uint GENERIC_READ = 0x80000000;
        const uint GENERIC_WRITE = 0x40000000;
        const int FILE_SHARE_READ = 0x1;
        const int FILE_SHARE_WRITE = 0x2;
        const int FSCTL_LOCK_VOLUME = 0x00090018;
        const int FSCTL_DISMOUNT_VOLUME = 0x00090020;
        const int IOCTL_STORAGE_EJECT_MEDIA = 0x2D4808;
        const int IOCTL_STORAGE_MEDIA_REMOVAL = 0x002D4804;

        private Core() { }

        public static bool EjectDrive(USBDeviceInfo drive) {
            if (drive.DriveLetters.Count <= 0) {
                return false;
            }
            return EjectDriveLetter(drive.DriveLetters[0]);
        }

        /*
         * driveLetter eg. "H:"
         */
        private static bool EjectDriveLetter(string driveLetter) {
            /*
             https://stackoverflow.com/questions/3918248/how-to-eject-a-usb-removable-disk-volume-similar-to-the-eject-function-in-win
             1. obtain a handle to the volume (CreateFile)
             2. try to lock the volume (FSCTL_LOCK_VOLUME)
             3. try to dismount it (FSCTL_DISMOUNT_VOLUME)
             4. disable the prevent storage media removal (IOCTL_STORAGE_MEDIA_REMOVAL)
             5. eject media (IOCTL_STORAGE_EJECT_MEDIA) 
             */

            var volumeHandle = DriveFileHandle(driveLetter);

            if (LockVolume(volumeHandle) == false) {
                return false;
            }

            if (DismountVolume(volumeHandle) == false) {
                return false;
            }

            if (PreventVolumeRemoval(volumeHandle, false) == false) {
                return false;
            }

            if (EjectVolume(volumeHandle) == false) {
                return false;
            }

            if (CloseHandle(volumeHandle) == false) {
                return false;
            }
            
            return true;
        }

        private static IntPtr DriveFileHandle(string driveLetter) {
            string filename = string.Format(@"\\.\{0}", driveLetter);
            return OSDelegate.CreateFile(filename, GENERIC_READ | GENERIC_WRITE, FILE_SHARE_READ | FILE_SHARE_WRITE, IntPtr.Zero, 0x3, 0, IntPtr.Zero);
        }

        private static bool LockVolume(IntPtr handle) {
            uint byteReturned;
            return OSDelegate.DeviceIoControl(handle, FSCTL_LOCK_VOLUME, IntPtr.Zero, 0, IntPtr.Zero, 0, out byteReturned, IntPtr.Zero);
        }

        private static bool DismountVolume(IntPtr handle) {
            uint byteReturned;
            return OSDelegate.DeviceIoControl(handle, FSCTL_DISMOUNT_VOLUME, IntPtr.Zero, 0, IntPtr.Zero, 0, out byteReturned, IntPtr.Zero);
        }

        private static bool PreventVolumeRemoval(IntPtr handle, bool prevent) {
            byte[] buf = new byte[1];
            uint retVal;
            buf[0] = prevent ? (byte)1 : (byte)0;
            return OSDelegate.DeviceIoControl(handle, IOCTL_STORAGE_MEDIA_REMOVAL, buf, 1, IntPtr.Zero, 0, out retVal, IntPtr.Zero);
        }

        private static bool EjectVolume(IntPtr handle) {
            uint byteReturned;
            return OSDelegate.DeviceIoControl(handle, IOCTL_STORAGE_EJECT_MEDIA, IntPtr.Zero, 0, IntPtr.Zero, 0, out byteReturned, IntPtr.Zero);
        }

        private static bool CloseHandle(IntPtr handle) {
            return OSDelegate.CloseHandle(handle);
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
