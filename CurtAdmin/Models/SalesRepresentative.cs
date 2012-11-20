using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;

namespace CurtAdmin {
    partial class SalesRepresentative {
        public int count { get; set; }

        public List<SalesRepresentative> GetAll() {
            CurtDevDataContext db = new CurtDevDataContext();
            List<SalesRepresentative> salesreps = new List<SalesRepresentative>();
            try {
                salesreps = db.SalesRepresentatives.OrderBy(x => x.name).ToList<SalesRepresentative>();
                foreach (SalesRepresentative r in salesreps) {
                    r.count = db.Customers.Where(x => x.salesRepID.Equals(r.salesRepID)).Count();
                }
            } catch { };
            return salesreps;
        }

        public SalesRepresentative Get(int id = 0) {
            CurtDevDataContext db = new CurtDevDataContext();
            SalesRepresentative rep = new SalesRepresentative();
            try {
                rep = db.SalesRepresentatives.Where(x => x.salesRepID.Equals(id)).FirstOrDefault<SalesRepresentative>();
            } catch { };
            return rep;
        }

        public SalesRepresentative Save(int id = 0, string name = "", string code = "") {
            CurtDevDataContext db = new CurtDevDataContext();
            SalesRepresentative rep = new SalesRepresentative();
            if (id > 0) {
                try {
                    rep = db.SalesRepresentatives.Where(x => x.salesRepID.Equals(id)).First<SalesRepresentative>();
                    rep.name = name.Trim();
                    rep.code = code.Trim();
                    db.SubmitChanges();
                } catch {}
            } else {
                rep = new SalesRepresentative {
                    name = name.Trim(),
                    code = code.Trim()
                };
                db.SalesRepresentatives.InsertOnSubmit(rep);
                db.SubmitChanges();
            }
            return rep;
        }

        public void Delete(int id = 0) {
            try {
                CurtDevDataContext db = new CurtDevDataContext();
                SalesRepresentative rep = db.SalesRepresentatives.Where(x => x.salesRepID.Equals(id)).First<SalesRepresentative>();
                db.SalesRepresentatives.DeleteOnSubmit(rep);
                db.SubmitChanges();
            } catch { }
        }

    }

}