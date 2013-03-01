using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace CurtAdmin {
    partial class ScheduledTask {
        public List<ScheduledTask> getTasks() {
            CurtDevDataContext db = new CurtDevDataContext();
            List<ScheduledTask> tasks = db.ScheduledTasks.OrderBy(x => x.name).ToList();
            return tasks;
        }

        public ScheduledTask Get(int id) {
            CurtDevDataContext db = new CurtDevDataContext();
            ScheduledTask task = db.ScheduledTasks.Where(x => x.ID.Equals(id)).FirstOrDefault<ScheduledTask>();
            return task;
        }

        public void Run() {
            string url = this.url;
            WebClient wc = new WebClient();
            wc.Proxy = null;
            wc.DownloadString(url);

            CurtDevDataContext db = new CurtDevDataContext();
            ScheduledTask t = db.ScheduledTasks.Where(x => x.ID.Equals(this.ID)).FirstOrDefault<ScheduledTask>();
            t.lastRan = DateTime.Now;
            db.SubmitChanges();
        }

    }
}
