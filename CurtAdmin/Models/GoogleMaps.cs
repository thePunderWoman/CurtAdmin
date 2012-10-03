using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CurtAdmin.Models {
    public class GoogleMaps {

        private string API_KEY = string.Empty;

        public GoogleMaps(string api_key) {
            this.API_KEY = api_key;
        }

        public void SetAPI_Key(string key) {
            if(key.Length == 0){
                throw new Exception("Invalid API Key");
            }
            this.API_KEY = key;
        }

        public LatLng GetLatLng(string addr, string city, int stateID) {
            CurtDevDataContext db = new CurtDevDataContext();


            // Get the state abbreviation that matches the stateID
            string state_abbr = (from ps in db.PartStates
                                 where ps.stateID.Equals(stateID)
                                 select ps.abbr).FirstOrDefault<string>();

            string url = "http://maps.google.com/maps/geo?output=csv&key=" + this.API_KEY + "&q=" + HttpContext.Current.Server.UrlEncode(addr + " " + city + ", " + state_abbr);

            WebClient wc = new WebClient();
            wc.Proxy = null;
            string coords = wc.DownloadString(url);
            if (coords != null && coords != "") {

                var parts = coords.Split(',');
                LatLng loc = new LatLng {
                    latitude = parts[2],
                    longitude = parts[3]
                };

                return loc;
            }
            return null;
        }

        public static LatLng GetLatLngStatic(string addr, string city, int stateID, string key) {
            CurtDevDataContext db = new CurtDevDataContext();


            // Get the state abbreviation that matches the stateID
            string state_abbr = (from ps in db.PartStates
                                 where ps.stateID.Equals(stateID)
                                 select ps.abbr).FirstOrDefault<string>();

            string url = "http://maps.google.com/maps/geo?output=csv&key=" + key + "&q=" + HttpContext.Current.Server.UrlEncode(addr + " " + city + ", " + state_abbr);

            var request = WebRequest.Create(url);
            var response = (HttpWebResponse)request.GetResponse();

            if (response.StatusCode == HttpStatusCode.OK) {

                var ms = new MemoryStream();
                var responseStream = response.GetResponseStream();

                var buffer = new Byte[2048];
                int count = responseStream.Read(buffer, 0, buffer.Length);

                while (count > 0) {
                    ms.Write(buffer, 0, count);
                    count = responseStream.Read(buffer, 0, buffer.Length);
                }
                responseStream.Close();
                ms.Close();

                var responseBytes = ms.ToArray();
                var encoding = new System.Text.ASCIIEncoding();

                var coords = encoding.GetString(responseBytes);
                var parts = coords.Split(',');
                LatLng loc = new LatLng {
                    latitude = parts[2],
                    longitude = parts[3]
                };

                return loc;
            }
            return null;
        }

    }

    public class LatLng{
        public string latitude { get; set; }
        public string longitude { get; set; }
    }

    public class CustomerGeo {
        public string name { get; set; }
        public string latitude { get; set; }
        public string longitude { get; set; }
    }
}