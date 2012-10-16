using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CurtAdmin.Models.B2b
{
    public class B2B
    {
        //////////////////////==  Create  ==////////////////////////////

        public static void addCert(string title, string text, int reqNum, string image_path, bool inActive)
        {
            try
            {
                B2BDataContext db = new B2BDataContext();

                B2BCertificate newCertificate = new B2BCertificate();

                newCertificate.title = title;
                newCertificate.text = text;
                newCertificate.requirementNum = reqNum;
                newCertificate.image_path = image_path;
                newCertificate.date_added = DateTime.Now;
                newCertificate.date_modified = DateTime.Now;
                newCertificate.inactive = inActive;


                db.B2BCertificates.InsertOnSubmit(newCertificate);
                db.SubmitChanges();

            }
            catch (Exception e)
            {
                throw new Exception("Could not add certificate: " + e.Message);
            }
        }
        public static void addCat(int certID, string title, string text, string image_path, bool inActive)
        {
            try
            {
                B2BDataContext db = new B2BDataContext();

                B2BCategory newCat = new B2BCategory();
                newCat.certID = certID;
                newCat.title = title;
                newCat.text = text;
                newCat.image_path = image_path;
                newCat.date_added = DateTime.Now;
                newCat.date_modified = DateTime.Now;
                newCat.inactive = inActive;
                db.B2BCategories.InsertOnSubmit(newCat);
                db.SubmitChanges();
            }
            catch (Exception e)
            {
                throw new Exception("Could not add category: " + e.Message);
            }
        }
        public static void addLesson(int catID, string title, string text, string pdf, bool inActive)
        {
            try
            {
                B2BDataContext db = new B2BDataContext();

                B2BLesson newLesson = new B2BLesson();
                newLesson.catID = catID;
                newLesson.title = title;
                newLesson.Text = text;
                newLesson.date_added = DateTime.Now;
                newLesson.date_modified = DateTime.Now;
                newLesson.inactive = inActive;

                db.B2BLessons.InsertOnSubmit(newLesson);
                db.SubmitChanges();

                B2BResource newPDF = new B2BResource();

                newPDF.file_path = pdf;
                newPDF.date_added = DateTime.Now;
                newPDF.sort = 1;
                newPDF.lessonID = newLesson.id;
                newPDF.title = title + " PDF";
                newPDF.image_path = "http://curtmfg.com/Content/img/pdf.png";
                db.B2BResources.InsertOnSubmit(newPDF);
                db.SubmitChanges();

            }
            catch (Exception e)
            {
                throw new Exception("Could not add Lesson: " + e.Message);
            }
        }
        public static void addTest(int catID, string title, string text, double min_pass_percentage, bool inActive)
        {
            try
            {
                B2BDataContext db = new B2BDataContext();
                B2BTest newTest = new B2BTest();
                newTest.catID = catID;
                newTest.title = title;
                newTest.text = text;
                newTest.min_pass_percent = min_pass_percentage;
                newTest.date_added = DateTime.Now;
                newTest.date_modified = DateTime.Now;
                newTest.isRandomOrder = false;
                newTest.inactive = inActive;

                db.B2BTests.InsertOnSubmit(newTest);
                db.SubmitChanges();
            }
            catch (Exception e)
            {
                throw new Exception("Could not add test: " + e.Message);
            }
        }
        public static void addQuestion(int testID, string text, bool inActive)
        {
            try
            {
                B2BDataContext db = new B2BDataContext();
                B2BQuestion newQuestion = new B2BQuestion();
                newQuestion.testID = testID;
                newQuestion.text = text;
                newQuestion.date_added = DateTime.Now;
                newQuestion.date_modified = DateTime.Now;
                newQuestion.sort = 1;
                newQuestion.inactive = inActive;
                db.B2BQuestions.InsertOnSubmit(newQuestion);
                db.SubmitChanges();
            }
            catch (Exception e)
            {
                throw new Exception("Could not add Question: " + e.Message);
            }
        }
        public static void addAnswer(int questionID, string text, bool isCorrect, bool inActive)
        {
            try
            {
                B2BDataContext db = new B2BDataContext();
                B2BAnswer newAnswer = new B2BAnswer();
                newAnswer.questionID = questionID;
                newAnswer.text = text;
                newAnswer.isCorrect = isCorrect;
                newAnswer.date_added = DateTime.Now;
                newAnswer.date_modified = DateTime.Now;
                newAnswer.inactive = inActive;
                db.B2BAnswers.InsertOnSubmit(newAnswer);
                db.SubmitChanges();
            }
            catch (Exception e)
            {
                throw new Exception("Could not add Answer: " + e.Message);
            }
        }
        public static void addVideo(int lessonID, string title, string mp4, string ogg, string webm, bool inActive)
        {
            try
            {
                // New Video
                B2BDataContext db = new B2BDataContext();
                B2BVideo newVideo = new B2BVideo();
                newVideo.title = title;
                newVideo.date_added = DateTime.Now;
                newVideo.sort = 1;
                newVideo.lessonID = lessonID;
                newVideo.inactive = inActive;
                db.B2BVideos.InsertOnSubmit(newVideo);
                db.SubmitChanges();
                // New Video Sources
                int videoID = newVideo.id;

                foreach (B2BVideoType videoType in getVideoTypes())
                {
                    string type = videoType.type;
                    B2BVideoSource newVideoSource = new B2BVideoSource();
                    newVideoSource.videoID = videoID;       
                    if (type == "mp4")
                    {
                        newVideoSource.filePath = mp4;
                        newVideoSource.typeID = videoType.id;
                    }
                    else if (type == "ogg")
                    {
                        newVideoSource.filePath = ogg;
                        newVideoSource.typeID = videoType.id;
                    }
                    else if (type == "webm")
                    {
                        newVideoSource.filePath = webm;
                        newVideoSource.typeID = videoType.id;
                    }
                    else
                    {
                        continue;
                    }
                    db.B2BVideoSources.InsertOnSubmit(newVideoSource);
                    db.SubmitChanges();
                }// end foreach videoType

            }
            catch (Exception e)
            {
                throw new Exception("Could not add video: " + e.Message);
            }
        }
        //////////////////////==  Read   ==/////////////////////////////

        public static List<B2BCertificate> getCertificates()
        {
            try
            {
                B2BDataContext db = new B2BDataContext();
                List<B2BCertificate> listOfCertificates = new List<B2BCertificate>();
                listOfCertificates = db.B2BCertificates.Select(x => x).ToList<B2BCertificate>();
                return listOfCertificates;
            }
            catch (Exception e)
            {
                throw new Exception("Could not load certificates: " + e.Message);
            }

        }
        public static List<B2BCategory> getCategories(int certID)
        {
            try
            {
                B2BDataContext db = new B2BDataContext();
                List<B2BCategory> listOfCategories = new List<B2BCategory>();
                listOfCategories = db.B2BCategories.Where(x => x.certID == certID).Select(x => x).ToList<B2BCategory>();
                return listOfCategories;
            }
            catch (Exception e)
            {
                throw new Exception("Could not load categories: " + e.Message);
            }

        }
        public static List<B2BLesson> getLessons(int catID)
        {
            try
            {
                B2BDataContext db = new B2BDataContext();
                List<B2BLesson> listOfLessons = new List<B2BLesson>();
                listOfLessons = db.B2BLessons.Where(x => x.catID == catID).Select(x => x).ToList<B2BLesson>();
                return listOfLessons;
            }
            catch (Exception e)
            {
                throw new Exception("Could not load lessons: " + e.Message);
            }

        }
        public static List<B2BTest> getTests(int catID)
        {
            try
            {
                B2BDataContext db = new B2BDataContext();
                List<B2BTest> listOfTests = new List<B2BTest>();
                listOfTests = db.B2BTests.Where(x => x.catID == catID).Select(x => x).ToList<B2BTest>();
                return listOfTests;
            }
            catch (Exception e)
            {
                throw new Exception("could not load tests: " + e.Message);
            }
        }
        public static List<B2BFullUser> getB2BUsers()
        {
            try
            {
                List<B2BUser> listOfB2BUsers = new List<B2BUser>();
                B2BDataContext db = new B2BDataContext();

                listOfB2BUsers = db.B2BUsers.ToList<B2BUser>();

                List<B2BFullUser> listOfFullUsers = new List<B2BFullUser>();


                foreach (B2BUser user in listOfB2BUsers)
                {
                    listOfFullUsers.Add(B2BFullUser.castToFullUser(user)); // add the generated full user to the list of Full Users
                }// end foreach b2b user

                return listOfFullUsers;
            }
            catch (Exception e)
            {
                throw new Exception("Could not load B2B Users: " + e.Message);
            }

        }
        public static List<B2BVideoType> getVideoTypes()
        {
            try
            {
                List<B2BVideoType> listOfVideoTypes = new List<B2BVideoType>();
                B2BDataContext db = new B2BDataContext();
                listOfVideoTypes = db.B2BVideoTypes.ToList<B2BVideoType>();
                return listOfVideoTypes;
            }
            catch (Exception e)
            {
                throw new Exception("Could not load B2B video types: " + e.Message );
            }
        }

        // get individual objects
        public static B2BAnswer getAnswer(int answerID)
        {
            try
            {
                B2BDataContext db = new B2BDataContext();
                B2BAnswer answer = new B2BAnswer();
                answer = db.B2BAnswers.Where(x => x.id == answerID).Select(x => x).FirstOrDefault<B2BAnswer>();
                return answer;
            }
            catch (Exception e)
            {
                throw new Exception("could not retrieve the answer: " + e.Message);
            }

        }
        public static B2BQuestion getQuestion(int questionID)
        {
            try
            {
                B2BDataContext db = new B2BDataContext();
                B2BQuestion question = new B2BQuestion();
                question = db.B2BQuestions.Where(x => x.id == questionID).Select(x => x).FirstOrDefault<B2BQuestion>();
                return question;
            }
            catch (Exception e)
            {
                throw new Exception("Could not load the question: " + e.Message);
            }

        }
        public static B2BTest getTest(int testID)
        {
            try
            {
                B2BDataContext db = new B2BDataContext();
                B2BTest test = new B2BTest();
                test = db.B2BTests.Where(x => x.id == testID).Select(x => x).FirstOrDefault<B2BTest>();
                return test;
            }
            catch (Exception e)
            {
                throw new Exception("Could not load the test: " + e.Message);
            }

        }
        public static B2BLesson getLesson(int lessonID)
        {
            try
            {
                B2BDataContext db = new B2BDataContext();
                B2BLesson lesson = new B2BLesson();
                lesson = db.B2BLessons.Where(x => x.id == lessonID).Select(x => x).FirstOrDefault<B2BLesson>();
                return lesson;
            }
            catch (Exception e)
            {
                throw new Exception("Could not load the lesson: " + e.Message);
            }

        }
        public static B2BVideo getVideo(int videoID)
        {
            try
            {
                B2BDataContext db = new B2BDataContext();
                B2BVideo video = new B2BVideo();
                video = db.B2BVideos.Where(x => x.id == videoID).Select(x => x).FirstOrDefault<B2BVideo>();
                return video;
            }
            catch (Exception e)
            {
                throw new Exception("Could not laod the video: " + e.Message);
            }
        }
        public static B2BCategory getCategory(int catID)
        {
            try
            {
                B2BDataContext db = new B2BDataContext();
                B2BCategory cat = new B2BCategory();
                cat = db.B2BCategories.Where(x => x.id == catID).Select(x => x).FirstOrDefault<B2BCategory>();
                return cat;
            }
            catch (Exception e)
            {
                throw new Exception("Could not load category: " + e.Message);
            }
        }
        public static B2BCertificate getCertificate(int certID)
        {
            try
            {
                B2BDataContext db = new B2BDataContext();
                B2BCertificate cert = new B2BCertificate();
                cert = db.B2BCertificates.Where(x => x.id == certID).Select(x => x).FirstOrDefault<B2BCertificate>();
                return cert;
            }
            catch (Exception e)
            {
                throw new Exception("Could not load certificate: " + e.Message);
            }

        }
        public static B2BFullUser getB2BFullUser(int b2bUserID)
        {
            try
            {
                // get b2b user with the id being passed in
                B2BDataContext db = new B2BDataContext();
                B2BUser b2bUser = db.B2BUsers.Where(x => x.id == b2bUserID).Select(x => x).FirstOrDefault<B2BUser>();
                // cast b2b user to full user and return it
                return B2BFullUser.castToFullUser(b2bUser);
            }
            catch (Exception e)
            {
                throw new Exception("Could get get B2B user: " + e.Message);
            }

        }
        public static B2BUser getB2BUser(int b2bUserID)
        {
            try
            {
                // get b2b user with the id being passed in
                B2BDataContext db = new B2BDataContext();
                B2BUser b2bUser = db.B2BUsers.Where(x => x.id == b2bUserID).Select(x => x).FirstOrDefault<B2BUser>();
                return b2bUser;
            }
            catch (Exception e)
            {
                throw new Exception("Could not load B2B User Info: " + e.Message);
            }
        }



        ///////////////////////==  Delete    ==/////////////////////////
        public static string DeleteCert(int id)
        {
            try
            {
                B2BDataContext db = new B2BDataContext();
                B2BCertificate cert = new B2BCertificate();
                cert = db.B2BCertificates.Where(x => x.id == id).FirstOrDefault<B2BCertificate>();
                db.B2BCertificates.DeleteOnSubmit(cert);
                db.SubmitChanges();
                return "";
            }
            catch (Exception e)
            {
                return "Error while deleting" + e.Message;
            }
        }
        public static string DeleteCat(int id)
        {
            try
            {
                B2BDataContext db = new B2BDataContext();
                B2BCategory cat = new B2BCategory();
                cat = db.B2BCategories.Where(x => x.id == id).FirstOrDefault<B2BCategory>();
                db.B2BCategories.DeleteOnSubmit(cat);
                db.SubmitChanges();
                return "";
            }
            catch (Exception)
            {
                return "Error while deleting";
            }
        }
        public static string DeleteLesson(int id)
        {
            try
            {
                B2BDataContext db = new B2BDataContext();
                B2BLesson lesson = new B2BLesson();
                lesson = db.B2BLessons.Where(x => x.id == id).FirstOrDefault<B2BLesson>();
                db.B2BLessons.DeleteOnSubmit(lesson);
                db.SubmitChanges();
                return "";
            }
            catch (Exception)
            {
                return "Error while deleting";
            }
        }
        public static string DeleteTest(int id)
        {
            try
            {
                B2BDataContext db = new B2BDataContext();
                B2BTest test = new B2BTest();
                test = db.B2BTests.Where(x => x.id == id).FirstOrDefault<B2BTest>();
                db.B2BTests.DeleteOnSubmit(test);
                db.SubmitChanges();
                return "";
            }
            catch (Exception)
            {
                return "Error while deleting";
            }
        }
        public static string DeleteQuestion(int id)
        {
            try
            {
                B2BDataContext db = new B2BDataContext();
                B2BQuestion question = new B2BQuestion();
                question = db.B2BQuestions.Where(x => x.id == id).FirstOrDefault<B2BQuestion>();
                db.B2BQuestions.DeleteOnSubmit(question);
                db.SubmitChanges();
                return "";
            }
            catch (Exception)
            {
                return "Error while deleting";
            }
        }
        public static string DeleteAnswer(int id)
        {
            try
            {
                B2BDataContext db = new B2BDataContext();
                B2BAnswer answer = new B2BAnswer();
                answer = db.B2BAnswers.Where(x => x.id == id).FirstOrDefault<B2BAnswer>();
                db.B2BAnswers.DeleteOnSubmit(answer);
                db.SubmitChanges();
                return "";
            }
            catch (Exception)
            {
                return "Error while deleting";
            }
        }
        public static string DeleteVideo(int id)
        {
            try
            {
                B2BDataContext db = new B2BDataContext();
                B2BVideo video = new B2BVideo();
                video = db.B2BVideos.Where(x => x.id == id).FirstOrDefault<B2BVideo>();
                db.B2BVideos.DeleteOnSubmit(video);
                db.SubmitChanges();
                return "";
            }
            catch (Exception)
            {
                return "Error while deleting";
            }
        }

        ///////////////////== AJAX == ////////////////////////////////////
        public static string SetPlaqueStatus(int certID, string custID)
        {
            try
            {
                B2BDataContext db = new B2BDataContext();
                B2BCompletedCert compCert = new B2BCompletedCert();

                compCert = db.B2BCompletedCerts.Where(x => x.certID == certID && x.custID == custID).FirstOrDefault<B2BCompletedCert>();
                if (compCert.hasPlaque == false)
                {
                    compCert.hasPlaque = true;
                }
                else
                {
                    compCert.hasPlaque = false;
                }
                db.SubmitChanges();

                return "";
            }
            catch (Exception e)
            {
                return "Error setting plaque status";
            }

        }

        //////////////////////==  B2B User Interaction ==/////////////////
        public static CustomerUser getCustomerUser(string customerUserID)
        {
            try
            {
                CurtDevDataContext db = new CurtDevDataContext();
                CustomerUser customerUser = db.CustomerUsers.Where(x => x.id.ToString() == customerUserID.ToString()).Select(x => x).FirstOrDefault<CustomerUser>();
                return customerUser;
            }
            catch (Exception e)
            {
                throw new Exception("Could not get the customer user: " + e.Message);
            }
        }
        public static Customer getCustomer(string customerUserID)
        {
            try
            {
                CurtDevDataContext db = new CurtDevDataContext();
                Customer customer = db.Customers.Where(x => x.APIKey.ToString() == customerUserID.ToString()).Select(x => x).FirstOrDefault<Customer>();
                return customer;
            }
            catch (Exception e)
            {
                throw new Exception("Could not get the customer: " + e.Message);
            }
        }
        public static bool isCustomerUser(string custID)
        {
            B2BDataContext dbB2B = new B2BDataContext();
            CurtDevDataContext db = new CurtDevDataContext();

            CustomerUser customerUserCheck = db.CustomerUsers.Where(x => x.id.ToString() == custID.ToString()).Select(x => x).FirstOrDefault<CustomerUser>();
            if (customerUserCheck == null)
            {
                // the b2bUser is a customer
                return false;
            }
            else
            {
                // the b2bUser is a customer User
                return true;
            }
        } // end isCustomerUser

    } // end class B2B

    public class B2BFullUser
    {
        public int B2BUserID { get; set; }
        public string custID { get; set; }
        public int customerID { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public bool isCustomerUser { get; set; }
        public int numLessonsCompleted { get; set; }
        public int numCertsCompleted { get; set; }
        public DateTime join_date { get; set; }


        public static B2BFullUser castToFullUser(B2BUser user)
        {

            B2BFullUser fullUser = new B2BFullUser();

            fullUser.isCustomerUser = B2B.isCustomerUser(user.custID);
            fullUser.B2BUserID = user.id;
            fullUser.custID = user.custID;
            fullUser.join_date = user.join_date;
            fullUser.numCertsCompleted = user.numCertsCompleted;
            fullUser.numLessonsCompleted = user.numLessonsCompleted;
            if (fullUser.isCustomerUser)
            {
                CustomerUser customerUser = B2B.getCustomerUser(user.custID);
                fullUser.email = customerUser.email;
                fullUser.name = customerUser.name;
                fullUser.customerID = Convert.ToInt16(customerUser.customerID);
            }
            else
            {
                Customer customer = B2B.getCustomer(user.custID);
                fullUser.email = customer.email;
                fullUser.name = customer.name;
                fullUser.customerID = Convert.ToInt16(customer.cust_id);
            }
            return fullUser;

        } // end of castToFullUser


    }


}// end namespace