using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;

namespace CurtAdmin.Models {
    public class ForumModel {

        public static List<FullGroup> GetAllGroups() {
            List<FullGroup> groups = new List<FullGroup>();
            try {
                CurtDevDataContext db = new CurtDevDataContext();

                groups = (from fg in db.ForumGroups
                          orderby fg.name
                          select new FullGroup {
                              forumGroupID = fg.forumGroupID,
                              name = fg.name,
                              description = fg.description,
                              createdDate = fg.createdDate,
                              topics = GetAllTopics(fg.forumGroupID)
                          }).ToList<FullGroup>();
            } catch (Exception e) { }

            return groups;
        }

        public static FullGroup GetGroup(int groupID = 0) {
            FullGroup group = new FullGroup();
            try {
                CurtDevDataContext db = new CurtDevDataContext();

                group = (from fg in db.ForumGroups
                          where fg.forumGroupID.Equals(groupID)
                          orderby fg.name
                          select new FullGroup {
                              forumGroupID = fg.forumGroupID,
                              name = fg.name,
                              description = fg.description,
                              createdDate = fg.createdDate,
                              topics = GetAllTopics(fg.forumGroupID)
                          }).First<FullGroup>();
            } catch (Exception e) { }

            return group;
        }

        public static List<FullTopic> GetAllTopics(int groupID = 0) {
            List<FullTopic> topics = new List<FullTopic>();
            try {
                CurtDevDataContext db = new CurtDevDataContext();

                topics = (from ft in db.ForumTopics
                          where ft.TopicGroupID.Equals(groupID) && ft.active == true
                            orderby ft.name
                            select new FullTopic {
                                topicID = ft.topicID,
                                TopicGroupID = ft.TopicGroupID,
                                name = ft.name,
                                description = ft.description,
                                image = ft.image,
                                createdDate = ft.createdDate,
                                active = ft.active,
                                closed = ft.closed,
                                count = db.ForumThreads.Where(x => x.topicID == ft.topicID).Count()
                            }).ToList<FullTopic>();
            } catch (Exception e) { }

            return topics;
        }

        public static FullTopic GetTopic(int topicID = 0) {
            FullTopic topic = new FullTopic();
            try {
                CurtDevDataContext db = new CurtDevDataContext();

                topic = (from ft in db.ForumTopics
                         where ft.topicID.Equals(topicID) && ft.active == true
                          orderby ft.name
                          select new FullTopic {
                              topicID = ft.topicID,
                              TopicGroupID = ft.TopicGroupID,
                              name = ft.name,
                              description = ft.description,
                              image = ft.image,
                              createdDate = ft.createdDate,
                              active = ft.active,
                              closed = ft.closed,
                              threads = GetAllThreads(ft.topicID)
                          }).First<FullTopic>();
            } catch (Exception e) { }

            return topic;
        }
        
        public static List<Thread> GetAllThreads(int topicID = 0) {
            List<Thread> threads = new List<Thread>();
            try {
                CurtDevDataContext db = new CurtDevDataContext();

                threads = (from ft in db.ForumThreads
                           where ft.topicID.Equals(topicID) && ft.active == true
                           orderby ft.createdDate descending
                           select new Thread {
                               threadID = ft.threadID,
                               topicID = ft.topicID,
                               createdDate = ft.createdDate,
                               active = ft.active,
                               closed = ft.closed,
                               count = db.ForumPosts.Where(x => x.threadID == ft.threadID).Where(x => x.active == true).Where(x => x.flag == false).Count(),
                               latestPost = db.ForumPosts.Where(x => x.threadID == ft.threadID).Where(x => x.active == true).Where(x => x.flag == false).OrderByDescending(x => x.createdDate).FirstOrDefault<ForumPost>(),
                               firstPost = db.ForumPosts.Where(x => x.threadID == ft.threadID).Where(x => x.active == true).Where(x => x.flag == false).OrderBy(x => x.createdDate).FirstOrDefault<ForumPost>()
                          }).ToList<Thread>();
            } catch (Exception e) { }

            return threads;
        }

        public static Thread GetThread(int threadID = 0) {
            Thread thread = new Thread();
            try {
                CurtDevDataContext db = new CurtDevDataContext();

                thread = (from ft in db.ForumThreads
                          where ft.threadID.Equals(threadID) && ft.active == true
                           select new Thread {
                               threadID = ft.threadID,
                               topicID = ft.topicID,
                               createdDate = ft.createdDate,
                               active = ft.active,
                               closed = ft.closed,
                               posts = GetPostsByThread(ft.threadID),
                               latestPost = db.ForumPosts.Where(x => x.threadID == ft.threadID).Where(x => x.active == true).Where(x => x.flag == false).OrderByDescending(x => x.createdDate).FirstOrDefault<ForumPost>(),
                               firstPost = db.ForumPosts.Where(x => x.threadID == ft.threadID).Where(x => x.active == true).Where(x => x.flag == false).OrderBy(x => x.createdDate).FirstOrDefault<ForumPost>()
                           }).First<Thread>();
            } catch (Exception e) { }

            return thread;
        }
        
        public static List<Post> GetPostsByThread(int threadID = 0) {
            List<Post> threads = new List<Post>();
            try {
                CurtDevDataContext db = new CurtDevDataContext();

                threads = (from p in db.ForumPosts
                           where p.threadID.Equals(threadID) && p.active == true && p.flag == false
                           orderby p.sticky descending, p.createdDate
                           select new Post {
                               postID = p.postID,
                               parentID = p.parentID,
                               createdDate = p.createdDate,
                               title = p.title,
                               post = p.post,
                               name = p.name,
                               company = p.company,
                               email = p.email,
                               IPAddress = p.IPAddress,
                               notify = p.notify,
                               active = p.active,
                               approved = p.approved,
                               flag = p.flag,
                               sticky = p.sticky,
                               posts = GetPostsByPost(p.postID)
                           }).ToList<Post>();
            } catch (Exception e) { }

            return threads;
        }

        public static List<Post> GetPostsByPost(int postID = 0) {
            List<Post> threads = new List<Post>();
            try {
                CurtDevDataContext db = new CurtDevDataContext();

                threads = (from p in db.ForumPosts
                           where p.parentID.Equals(postID) && p.active == true
                           orderby p.sticky, p.createdDate descending
                           select new Post {
                               postID = p.postID,
                               parentID = p.parentID,
                               createdDate = p.createdDate,
                               title = p.title,
                               post = p.post,
                               name = p.name,
                               company = p.company,
                               email = p.email,
                               IPAddress = p.IPAddress,
                               notify = p.notify,
                               active = p.active,
                               approved = p.approved,
                               flag = p.flag,
                               sticky = p.sticky,
                               posts = GetPostsByPost(p.postID),
                           }).ToList<Post>();
            } catch (Exception e) { }

            return threads;
        }

        public static List<Post> GetSpam() {
            List<Post> posts = new List<Post>();
            try {
                CurtDevDataContext db = new CurtDevDataContext();

                posts = (from p in db.ForumPosts
                           where p.flag == true && p.active == true
                           orderby p.createdDate descending
                           select new Post {
                               postID = p.postID,
                               parentID = p.parentID,
                               createdDate = p.createdDate,
                               title = p.title,
                               post = p.post,
                               name = p.name,
                               company = p.company,
                               email = p.email,
                               IPAddress = p.IPAddress,
                               notify = p.notify,
                               active = p.active,
                               approved = p.approved,
                               flag = p.flag,
                               sticky = p.sticky,
                               posts = GetPostsByPost(p.postID)
                           }).ToList<Post>();
            } catch (Exception e) { }

            return posts;
        }

        public static List<Post> GetUnapproved() {
            List<Post> posts = new List<Post>();
            try {
                CurtDevDataContext db = new CurtDevDataContext();

                posts = (from p in db.ForumPosts
                         where p.flag == false && p.approved == false && p.active == true
                         orderby p.createdDate descending
                         select new Post {
                             postID = p.postID,
                             parentID = p.parentID,
                             createdDate = p.createdDate,
                             title = p.title,
                             post = p.post,
                             name = p.name,
                             company = p.company,
                             email = p.email,
                             IPAddress = p.IPAddress,
                             notify = p.notify,
                             active = p.active,
                             approved = p.approved,
                             flag = p.flag,
                             posts = GetPostsByPost(p.postID),
                             sticky = p.sticky
                         }).ToList<Post>();
            } catch (Exception e) { }

            return posts;
        }
        
        public static Post GetPost(int postID = 0) {
            Post post = new Post();
            try {
                CurtDevDataContext db = new CurtDevDataContext();

                post = (from p in db.ForumPosts
                           where p.postID.Equals(postID) && p.active == true
                           orderby p.createdDate descending
                           select new Post {
                               postID = p.postID,
                               threadID = p.threadID,
                               parentID = p.parentID,
                               createdDate = p.createdDate,
                               date = String.Format("{0:dddd, MMMM d, yyyy} at {0: h:mm tt}", p.createdDate),
                               title = p.title,
                               post = p.post,
                               name = p.name,
                               company = p.company,
                               email = p.email,
                               IPAddress = p.IPAddress,
                               notify = p.notify,
                               active = p.active,
                               approved = p.approved,
                               flag = p.flag,
                               sticky = p.sticky,
                               posts = GetPostsByPost(p.postID)
                           }).First<Post>();
            } catch (Exception e) { }

            return post;
        }

        public static bool DeleteGroup(int id = 0) {
            CurtDevDataContext db = new CurtDevDataContext();
            try {
                ForumGroup g = db.ForumGroups.Where(x => x.forumGroupID == id).First<ForumGroup>();
                db.ForumGroups.DeleteOnSubmit(g);
                db.SubmitChanges();
                return true;
            } catch {
                return false;
            }
        }

        public static bool DeleteTopic(int id = 0) {
            CurtDevDataContext db = new CurtDevDataContext();
            try {
                ForumTopic t = db.ForumTopics.Where(x => x.topicID == id).First<ForumTopic>();
                t.active = false;
                db.SubmitChanges();
                return true;
            } catch {
                return false;
            }
        }

        public static List<BlockedIPAddress> GetBlockedIPs() {
            CurtDevDataContext db = new CurtDevDataContext();

            List<BlockedIPAddress> ips = new List<BlockedIPAddress>();
            ips = (from i in db.IPBlocks
                   orderby i.IPAddress
                   select new BlockedIPAddress {
                       blockID = i.blockID,
                       IPAddress = i.IPAddress,
                       createdDate = i.createdDate,
                       reason = i.reason,
                       userID = i.userID
                   }).ToList<BlockedIPAddress>();
                db.IPBlocks.OrderBy(x => x.IPAddress).ToList<IPBlock>();
            return ips;
        }

        public static bool RemoveIPBlock(int id = 0) {
            try {
                CurtDevDataContext db = new CurtDevDataContext();
                IPBlock i = db.IPBlocks.Where(x => x.blockID == id).First<IPBlock>();
                db.IPBlocks.DeleteOnSubmit(i);
                db.SubmitChanges();
                return true;
            } catch { return false; }
        }
    }

    public class BlockedIPAddress : IPBlock {
        public string name {
            get {
                string n = "";
                try {
                    DocsLinqDataContext docsdb = new DocsLinqDataContext();
                    n = (from u in docsdb.users
                         where u.userID == this.userID
                         select u.fname + " " + u.lname).FirstOrDefault();
                } catch { };
                return n;
            }
        }
    }
    
    public class FullGroup : ForumGroup {
        public List<FullTopic> topics { get; set; }
    }

    public class FullTopic : ForumTopic {
        public List<Thread> threads { get; set; }
        public int count { get; set; }
    }

    public class Thread : ForumThread {
        public List<Post> posts { get; set; }
        public int count { get; set; }
        public ForumPost firstPost { get; set; }
        public ForumPost latestPost { get; set; }
    }

    public class Post : ForumPost {
        public List<Post> posts { get; set; }
        public string date { get; set; }

        public string getName() {
            if (this.name.Trim() == "") {
                return "Anonymous";
            }
            return this.name;
        }
    }
}