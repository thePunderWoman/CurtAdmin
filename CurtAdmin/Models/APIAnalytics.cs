using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CurtAdmin {
    partial class APIAnalytic {
        public List<APIAnalytic> GetAnalytics(int page, int perpage) {
            loggingDataContext db = new loggingDataContext();
            int skip = (page - 1) * perpage;
            List<APIAnalytic> analytics = db.APIAnalytics.Where(x => x.date < DateTime.Now.AddMinutes(-5)).OrderByDescending(x => x.date).Skip(skip).Take(perpage).ToList();
            return analytics;
        }
    }
}