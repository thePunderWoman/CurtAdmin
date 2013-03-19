using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;

namespace CurtAdmin {
    partial class IPtoDNS {

        public void CheckAddresses() {
            loggingDataContext db = new loggingDataContext();
            List<IPtoDNS> addresses = db.IPtoDNS.Where(x => x.dnsentry == null).Take(10).ToList();
            foreach (IPtoDNS adr in addresses) {
                LookupAsync(adr.ipaddress);
            }
        }

        public IPHostEntry Lookup(string address) {
            IPHostEntry hostEntry = new IPHostEntry();
            try {
                IPAddress hostIPAddress = IPAddress.Parse(address);
                hostEntry = Dns.GetHostEntry(hostIPAddress);
            } catch { }
            return hostEntry;
        }

        public void LookupAsync(string address) {
            AsyncCallback callback = LookupAsyncCompleteCallback;
            ResolveState ioContext= new ResolveState(address);
            IPAddress hostIPAddress = IPAddress.Parse(address);
            Dns.BeginGetHostEntry(hostIPAddress, callback, ioContext);
        }

        /// <summary>
        /// Announce completion of PUT operation
        /// </summary>
        /// <param name="result"></param>
        private void LookupAsyncCompleteCallback(IAsyncResult ar) {
            ResolveState ioContext = (ResolveState)ar.AsyncState;
            loggingDataContext db = new loggingDataContext();
            string hostname = "unknown";
            try {
                ioContext.IPs = Dns.EndGetHostEntry(ar);
                hostname = ioContext.IPs.HostName;
            } catch { };
            IPtoDNS ip = db.IPtoDNS.Where(x => x.ipaddress.Equals(ioContext.host.Trim())).FirstOrDefault();
            if (ip != null && ip.ID > 0) {
                ip.dnsentry = hostname;
                db.SubmitChanges();
            }
            GetHostEntryFinished.Set();
        }

        public class ResolveState {
            string hostName;
            IPHostEntry resolvedIPs;

            public ResolveState(string host) {
                hostName = host;
            }

            public IPHostEntry IPs {
                get { return resolvedIPs; }
                set { resolvedIPs = value; }
            }
            public string host {
                get { return hostName; }
                set { hostName = value; }
            }
        }

        // Signals when the resolve has finished. 
        public static ManualResetEvent GetHostEntryFinished =
            new ManualResetEvent(false);    
    }
}