using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CurtAdmin.Models;

namespace CurtAdmin.Models {

    public class APIPart {

        private List<APIAttribute> _content = new List<APIAttribute>();
        private List<APIAttribute> _attributes = new List<APIAttribute>();
        private List<APIAttribute> _vehicleattributes = new List<APIAttribute>();
        private List<APIAttribute> _pricing = new List<APIAttribute>();
        private List<APIReview> _reviews = new List<APIReview>();
        private List<APIImage> _images = new List<APIImage>();
        private int _partID = 0;
        private int _custPartID = 0;
        private int _status = 0;
        private string _dateModified = "";
        private string _dateAdded = "";
        private string _shortDesc = "";
        private string _oldPartNumber = "";
        private string _listPrice = "";
        private string _pClass = "";
        private int _relatedCount = 0;
        private int _installTime = 0;
        private double? _averageReview = 0;
        private string _drilling = "";
        private string _exposed = "";
        private int? _vehicleID = 0;


        public int partID {
            get {
                return this._partID;
            }
            set {
                if (value != null && value != this._partID) {
                    this._partID = value;
                }
            }
        }
        public int custPartID {
            get {
                return this._custPartID;
            }
            set {
                if (value != null && value != this._custPartID) {
                    this._custPartID = value;
                }
            }
        }
        public int status {
            get {
                return this._status;
            }
            set {
                if (value != null && value != this._status) {
                    this._status = value;
                }
            }
        }
        public string dateModified {
            get {
                return this._dateModified;
            }
            set {
                if (value != null && value != this._dateModified) {
                    this._dateModified = value;
                }
            }
        }
        public string dateAdded {
            get {
                return this._dateAdded;
            }
            set {
                if (value != null && value != this._dateAdded) {
                    this._dateAdded = value;
                }
            }
        }
        public string shortDesc {
            get {
                return this._shortDesc;
            }
            set {
                if (value != null && value != this._shortDesc) {
                    this._shortDesc = value;
                }
            }
        }
        public string oldPartNumber {
            get {
                return this._oldPartNumber;
            }
            set {
                if (value != null && value != this._oldPartNumber) {
                    this._oldPartNumber = value;
                }
            }
        }
        public string listPrice {
            get {
                return this._listPrice;
            }
            set {
                if (value != null && value != this._listPrice) {
                    this._listPrice = value;
                }
            }
        }
        public List<APIAttribute> attributes {
            get {
                return this._attributes;
            }
            set {
                if (value != null && value != this._attributes) {
                    this._attributes = value;
                }
            }
        }
        public List<APIAttribute> vehicleAttributes {
            get {
                return this._vehicleattributes;
            }
            set {
                if (value != null && value != this._vehicleattributes) {
                    this._vehicleattributes = value;
                }
            }
        }
        public List<APIAttribute> content {
            get {
                return this._content;
            }
            set {
                if (value != null && value != this._content) {
                    this._content = value;
                }
            }
        }
        public List<APIAttribute> pricing {
            get {
                return this._pricing;
            }
            set {
                if (value != null && value != this._pricing) {
                    this._pricing = value;
                }
            }
        }
        public List<APIReview> reviews {
            get {
                return this._reviews;
            }
            set {
                if (value != null && value != this._reviews) {
                    this._reviews = value;
                }
            }
        }
        public List<APIImage> images {
            get {
                return this._images;
            }
            set {
                if (value != null && value != this._images) {
                    this._images = value;
                }
            }
        }
        public string pClass {
            get {
                return this._pClass;
            }
            set {
                if (value != null && value != this._pClass) {
                    this._pClass = value;
                }
            }
        }

        public int relatedCount {
            get {
                return this._relatedCount;
            }
            set {
                if (value != null && value != this._relatedCount) {
                    this._relatedCount = value;
                }
            }
        }

        public int installTime {
            get {
                return this._installTime;
            }
            set {
                if (value != null && value != this._installTime) {
                    this._installTime = value;
                }
            }
        }

        public double? averageReview {
            get {
                return this._averageReview;
            }
            set {
                if (value != null && value != this._averageReview) {
                    this._averageReview = value;
                }
            }
        }

        public string drilling {
            get {
                return this._drilling;
            }
            set {
                if (value != null && value != this._drilling) {
                    this._drilling = value;
                }
            }
        }

        public string exposed {
            get {
                return this._exposed;
            }
            set {
                if (value != null && value != this._exposed) {
                    this._exposed = value;
                }
            }
        }
        public int? vehicleID {
            get {
                return this._vehicleID;
            }
            set {
                if (value != null && value != this._vehicleID) {
                    this._vehicleID = value;
                }
            }
        }
    }

    public class KioskPart {

        private List<KeyValuePair<string,string>> _content = new List<KeyValuePair<string, string>>();
        private List<KeyValuePair<string, string>> _attributes = new List<KeyValuePair<string,string>>();
        private int _partID = 0;
        private int _status = 0;
        private string _dateModified = "";
        private string _dateAdded = "";
        private string _shortDesc = "";
        private string _oldPartNumber = "";
        private string _listPrice = "";
        private string _pClass = "";

        
        public int partID {
            get {
                return this._partID;
            }
            set {
                if (value != null && value != this._partID) {
                    this._partID = value;
                }
            }
        }
        public int status {
            get {
                return this._status;
            }
            set {
                if (value != null && value != this._status) {
                    this._status = value;
                }
            }
        }
        public string dateModified {
            get {
                return this._dateModified;
            }
            set {
                if (value != null && value != this._dateModified) {
                    this._dateModified = value;
                }
            }
        }
        public string dateAdded {
            get {
                return this._dateAdded;
            }
            set {
                if (value != null && value != this._dateAdded) {
                    this._dateAdded = value;
                }
            }
        }
        public string shortDesc {
            get {
                return this._shortDesc;
            }
            set {
                if (value != null && value != this._shortDesc) {
                    this._shortDesc = value;
                }
            }
        }
        public string oldPartNumber {
            get {
                return this._oldPartNumber;
            }
            set {
                if (value != null && value != this._oldPartNumber) {
                    this._oldPartNumber = value;
                }
            }
        }
        public string listPrice {
            get {
                return this._listPrice;
            }
            set {
                if (value != null && value != this._listPrice) {
                    this._listPrice = value;
                }
            }
        }
        public List<KeyValuePair<string, string>> attributes {
            get {
                return new List<KeyValuePair<string,string>>();
            }
            set {
                if (value != null && value != this._attributes) {
                    this._attributes = value;
                    indexed_attributes = new Dictionary<string, string>();
                    int i = 0;
                    foreach(var pair in value){
                        string newKey = pair.Key + i;
                        indexed_attributes.Add(newKey,pair.Value);
                        i++;
                    }
                }
            }
        }
        public IDictionary<string, string> indexed_attributes { get; set; }
        public List<KeyValuePair<string ,string>> content {
            get {
                return new List<KeyValuePair<string, string>>();
            }
            set {
                if (value != null && value != this._content) {
                    this._content = value;
                    indexed_content = new Dictionary<string, string>();
                    int i = 0;
                    foreach (var pair in value) {
                        string newKey = pair.Key + i;
                        indexed_content.Add(newKey, pair.Value);
                        i++;
                    }
                }
            }
        }
        public IDictionary<string, string> indexed_content { get; set; }
        public string pClass {
            get {
                return this._pClass;
            }
            set {
                if (value != null && value != this._pClass) {
                    this._pClass = value;
                }
            }
        }
    }

    public class APIAttribute {
        public string key { get; set; }
        public string value { get; set; }
    }

    public class APIReview {
        public int reviewID { get; set; }
        public int partID { get; set; }
        public int rating { get; set; }
        public string subject { get; set; }
        public string review_text { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public string createdDate { get; set; }
    }

    public class CommentResponseObject {
        public string error { get; set; }
        public int itemID { get; set; }
        public int commentID { get; set; }
        public string comment { get; set; }
        public int userID { get; set; }
        public string fname { get; set; }
        public string lname { get; set; }
        public int isAdmin { get; set; }
    }

    /// <summary>
    /// Class to hold the part information. Modifies SQL Server's datetime format into something that is actually readable.
    /// </summary>
    public class ConvertedPart {
        private DateTime _dateModified;
        private DateTime _dateAdded;
        private string _listPrice;

        public int partID { get; set; }
        public int status { get; set; }
        public string dateModified { get; set; }
        public string dateAdded { get; set; }
        public string shortDesc { get; set; }
        public string oldPartNumber { get; set; }
        public int priceCode { get; set; }
        public int pClass { get; set; }
        public bool featured { get; set; }

        public string listPrice { get; set; }
    }

    public class DetailedCategories {
        public int catID { get; set; }

        public DateTime? dateAdded { get; set; }

        public int parentID { get; set; }

        public string parentCat { get; set; }

        public string catTitle { get; set; }

        public string shortDesc { get; set; }

        public string longDesc { get; set; }

        public string image { get; set; }

        public int isLifestyle { get; set; }

        public bool vehicleSpecific { get; set; }

        public int partCount { get; set; }

        public List<FullContent> content { get; set; }

    }

    /// <summary>
    /// This object will hold strings containing the actual values of the year, make, model, and style for a vehicle.
    /// </summary>
    public class FullVehicle {
        public int vehicleID { get; set; }
        public int yearID { get; set; }
        public int makeID { get; set; }
        public int modelID { get; set; }
        public int styleID { get; set; }
        public int aaiaID { get; set; }
        public double year { get; set; }
        public string make { get; set; }
        public string model { get; set; }
        public string style { get; set; }
        public int installTime { get; set; }
        public string drilling { get; set; }
        public string exposed { get; set; }
        public List<APIAttribute> attributes { get; set; }
    }

    public class CarryOverInfo : FullVehicle {
        public int vehicleID { get; set; }
        public int yearID { get; set; }
        public int makeID { get; set; }
        public int modelID { get; set; }
        public int styleID { get; set; }
        public int aaiaID { get; set; }
        public double year { get; set; }
        public string make { get; set; }
        public string model { get; set; }
        public string style { get; set; }
        public int installTime { get; set; }
        public string drilling { get; set; }
        public string exposed { get; set; }
        public List<APIAttribute> attributes { get; set; }
        public List<int> partids { get; set; }
    }
    
    /// <summary>
    /// Info on friend tweets
    /// </summary>
    public class TweetViewModel {
        /// <summary>
        /// User's avatar
        /// </summary>
        public string profie_image { get; set; }

        /// <summary>
        /// User's Twitter name
        /// </summary>
        public string screen_name { get; set; }

        /// <summary>
        /// Text containing user's tweet
        /// </summary>
        public string tweet { get; set; }

        public string created_at { get; set; }

        public long tweet_id { get; set; }
        public string source { get; set; }

    }

    public class TwitterUserModel {

        public int userID { get; set; }
        public string name { get; set; }
        public string screen_name { get; set; }
        public string location { get; set; }
        public string description { get; set; }
        public string profile_image { get; set; }
        public string url { get; set; }
        public int friend_count { get; set; }

    }

    public class UserComment {
        public int commentID { get; set; }
        public int userID { get; set; }
        public string comment { get; set; }
        public DateTime dateAdded { get; set; }
        public int itemID { get; set; }
        public int parentComment { get; set; }
        public string fname { get; set; }
        public string lname { get; set; }
        public int isAdmin { get; set; }
    }

    public class FullContent {
        public string content_type { get; set; }
        public int content_type_id { get; set; }
        public int contentID { get; set; }
        public string content { get; set; }
    }
}