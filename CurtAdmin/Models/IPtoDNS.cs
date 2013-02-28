using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;

namespace CurtAdmin {
    partial class IPtoDNS {

        public void CheckAddresses() {
            CurtDevDataContext db = new CurtDevDataContext();
            List<IPtoDNS> addresses = db.IPtoDNS.Where(x => x.dnsentry == null).ToList();
            foreach (IPtoDNS adr in addresses) {
                LookupAsync(adr.ipaddress);
            }
        }

        public string Lookup(string address) {
            IPHostEntry hostEntry = Dns.GetHostEntry(address);
            string url = hostEntry.HostName;
            return url;
        }

        public void LookupAsync(string address) {
            AsyncCallback callback = LookupAsyncCompleteCallback;
            ResolveState ioContext= new ResolveState(address);
            Dns.BeginGetHostEntry(address,callback,ioContext);
        }

        /// <summary>
        /// Announce completion of PUT operation
        /// </summary>
        /// <param name="result"></param>
        private void LookupAsyncCompleteCallback(IAsyncResult ar) {
            ResolveState ioContext = (ResolveState)ar.AsyncState;

            ioContext.IPs = Dns.EndGetHostEntry(ar);
            CurtDevDataContext db = new CurtDevDataContext();
            IPtoDNS ip = db.IPtoDNS.Where(x => x.ipaddress.Equals(ioContext.host.Trim())).FirstOrDefault();
            if (ip != null && ip.ID > 0) {
                ip.dnsentry = ioContext.IPs.HostName;
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