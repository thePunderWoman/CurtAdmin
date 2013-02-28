using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CurtAdmin {
    partial class ImportProcess {
        public int average { get; set; }

        public bool isRunning() {
            bool running = false;
            if (this.endTime.Equals(null)) {
                return true;
            }
            return running;
        }
    }
}