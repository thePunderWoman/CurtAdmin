using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CurtAdmin {
    partial class WebPropRequirement {

        public bool GetCheck(int webPropID, int webPropReqID) {
            bool check = false;
            // check if exists
            if (this.WebPropRequirementChecks.Any(x => x.WebPropertiesID == webPropID && x.WebPropRequirementsID == webPropReqID)) {
                // if exists, return bool value for Compliance
                check = this.WebPropRequirementChecks.Where(x => x.WebPropertiesID == webPropID && x.WebPropRequirementsID == webPropReqID).Select(x => x.Compliance).FirstOrDefault();
            }
            return check;
        }

    }
}