using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wox.Plugin;

namespace WoxEject {
    public class DefaultIconResult : Result {
        public DefaultIconResult() {
            IcoPath = "Images//icon.png";
        }

        public DefaultIconResult(string title, string subTitle) : base(title, "Images//icon.png", subTitle) {

        }
    }
}
