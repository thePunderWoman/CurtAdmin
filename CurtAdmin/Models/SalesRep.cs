using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;

namespace CurtAdmin.Models {
    public class SalesRepModel {

        public static List<SalesRepresentative> GetAll() {
            CurtDevDataContext db = new CurtDevDataContext();
            List<SalesRepresentative> salesreps = new List<SalesRepresentative>();
            try {
                salesreps = db.SalesRepresentatives.OrderBy(x => x.name).ToList<SalesRepresentative>();
            } catch { };
            return salesreps;
        }

        public static SalesRepresentative Get(int id = 0) {
            CurtDevDataContext db = new CurtDevDataContext();
            SalesRepresentative rep = new SalesRepresentative();
            try {
                rep = db.SalesRepresentatives.Where(x => x.salesRepID.Equals(id)).FirstOrDefault<SalesRepresentative>();
            } catch { };
            return rep;
        }

    }

}