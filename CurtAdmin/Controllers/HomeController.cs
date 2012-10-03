using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CurtAdmin.Models;
using System.Configuration;
using System.Xml.Linq;

namespace CurtAdmin.Controllers {
    public class HomeController : UserBaseController {

        /*IOAuthCredentials credentials = new SessionStateCredentials();
        private MvcAuthorizer auth;
        private TwitterContext twitterCtx;*/

        public ActionResult Index() {

            ViewBag.Message = "Welcome "+ Session["username"];

            /******************************************************/
            /*
             *       This section will redirect to Twitter OAuth
             *       login and bring the user back to the 
             *       application after they have authenticated 
             *       
             ******************************************************/

            // Get the twitter posts for curtlabs
            /*if (credentials.ConsumerKey == null || credentials.ConsumerSecret == null) {
                credentials.ConsumerKey = ConfigurationManager.AppSettings["twitterConsumerKey"];
                credentials.ConsumerSecret = ConfigurationManager.AppSettings["twitterConsumerSecret"];
            }

            auth = new MvcAuthorizer { Credentials = credentials };
            auth.CompleteAuthorization(Request.Url);
            /*if (!auth.IsAuthorized) {
                return auth.BeginAuthorization();
            }

            twitterCtx = new TwitterContext(;
            List<TweetViewModel> tweets = new List<TweetViewModel>();

            tweets = 

            tweets = (from tweet in twitterCtx.Status
                                where tweet.Type == StatusType.Public && tweet.User.ScreenName.Equals("curtmfg")
                                select new TweetViewModel{
                                    ImageUrl = (tweet.User.ProfileImageUrl != null)?tweet.User.ProfileImageUrl:"",
                                    ScreenName = (tweet.User.ScreenName != null)?tweet.User.ScreenName:"",
                                    Tweet = (tweet.Text != null)?tweet.Text:""
                                }).ToList<TweetViewModel>();
            ViewBag.tweets = tweets;*/

            /************* End OAuth ********************************/
            /*try {
                // Get the people we follow on curtlabs
                XDocument friends_xml = XDocument.Load("http://api.twitter.com/1/statuses/friends.xml?screen_name=curtlabs");
                List<TwitterUserModel> friends = new List<TwitterUserModel>();
                friends = (from f in friends_xml.Descendants("user")
                           select new TwitterUserModel {
                               userID = Convert.ToInt32((string)f.Element("id")),
                               name = (string)f.Element("name"),
                               screen_name = (string)f.Element("screen_name"),
                               location = (string)f.Element("location"),
                               description = (string)f.Element("description"),
                               profile_image = (string)f.Element("profile_image_url"),
                               url = (string)f.Element("url"),
                               friend_count = Convert.ToInt32((string)f.Element("friend_count"))
                           }).ToList<TwitterUserModel>();

                List<TweetViewModel> tweets = new List<TweetViewModel>();
                foreach (TwitterUserModel friend in friends) {

                    // Get this friends tweets
                    XDocument tweets_xml = XDocument.Load("http://api.twitter.com/1/statuses/user_timeline.xml?user_id=" + friend.userID);
                    List<TweetViewModel> friend_tweets = new List<TweetViewModel>();
                    friend_tweets = (from t in tweets_xml.Descendants("status")
                                     select new TweetViewModel {
                                         profie_image = friend.profile_image,
                                         screen_name = friend.screen_name,
                                         tweet = (string)t.Element("text"),
                                         created_at = (string)t.Element("created_at"),
                                         tweet_id = Convert.ToInt64((string)t.Element("id")),
                                         source = (string)t.Element("source")
                                     }).ToList<TweetViewModel>();
                    tweets.AddRange(friend_tweets);
                }
                List<TweetViewModel> sorted_tweets = tweets.OrderByDescending(x => x.created_at).ToList<TweetViewModel>();
                ViewBag.tweets = sorted_tweets;
            } catch (Exception e) {
                ViewBag.tweets = new List<TweetViewModel>();
            }*/
            ViewBag.tweets = new List<TweetViewModel>();

            // Get the modules for this user
            List<module> modules = new List<module>();
            modules = Users.GetUserModules(Convert.ToInt32(Session["userID"]));
            ViewBag.Modules = modules;
            
            return View();
        }
    }
}
