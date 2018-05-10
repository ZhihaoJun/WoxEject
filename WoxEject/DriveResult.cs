using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wox.Plugin;

namespace WoxEject {
    class DriveResult : Result {
        private string mDriveName;
        private string mDriveLetters;

        public DriveResult(string driveName, string driveLetters) : base(driveName, null, driveLetters) {
            mDriveName = driveName;
            mDriveLetters = driveLetters;
            Title = driveName;
            SubTitle = driveLetters;
            IcoPath = "Images//icon.png";
            Action = OnSelect;
        }

        public bool OnSelect(ActionContext ctx) {
            Console.WriteLine("[DriveResult.OnSelect] %s selected", mDriveName);
            return true;
        }
    }
}
