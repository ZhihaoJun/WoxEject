using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wox.Plugin;
using System.Windows.Forms;

namespace WoxEject {
    class DriveResult : DefaultIconResult {
        private string mDriveName;
        private string mDriveLetters;
        private USBDeviceInfo mDeviceInfo;

        public DriveResult(string driveName, string driveLetters, USBDeviceInfo deviceInfo) : base(driveName, driveLetters) {
            mDriveName = driveName;
            mDriveLetters = driveLetters;
            mDeviceInfo = deviceInfo;

            Title = driveName;
            SubTitle = driveLetters;
            Action = OnSelect;
        }

        public bool OnSelect(ActionContext ctx) {
            Console.WriteLine("[DriveResult.OnSelect] %s selected", mDriveName);

            if (Core.EjectDrive(mDeviceInfo)) {
                NotifyIcon icon = new NotifyIcon();
                string title = "Safe To Remove Hardware";
                string body = string.Format("Device '{0}' can now be safely removed from the computer", mDeviceInfo.Caption);
                icon.Visible = true;
                icon.Icon = System.Drawing.SystemIcons.Information;
                icon.ShowBalloonTip(800, title, body, ToolTipIcon.Info);
            }

            return true;
        }
    }
}
