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
        public static void addLesson(int catID, string title, string text, string video, string pdf, bool inActive)
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

                B2BVideo newVideo = new B2BVideo();
                newVideo.embed_link = video;
                newVideo.date_added = DateTime.Now;
                newVideo.sort = 1;
                newVideo.lessonID = newLesson.id;

                db.B2BVideos.InsertOnSubmit(newVideo);
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


        ///////////////////////==  Delete    ==/////////////////////////
        public static string DeleteCert(int id){
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


        //////////////////////==  B2B User Interaction ==/////////////////
        public static CustomerUser getCustomerUser(Guid customerUserID)
        {
            try
            {
                CurtDevDataContext db = new CurtDevDataContext();
                CustomerUser customerUser = db.CustomerUsers.Where(x => x.id == customerUserID).Select(x => x).FirstOrDefault<CustomerUser>();
                return customerUser;
            }
            catch (Exception e)
            {
                throw new Exception("Could not get the customer user: " + e.Message);
            }
        }
        public static Customer getCustomer(Guid customerUserID)
        {
            try
            {
                CurtDevDataContext db = new CurtDevDataContext();
                Customer customer = db.Customers.Where(x => x.APIKey == customerUserID).Select(x => x).FirstOrDefault<Customer>();
                return customer;
            }
            catch (Exception e)
            {
                throw new Exception("Could not get the customer: " + e.Message);
            }
        }
        public static Boolean isCustomerUser(Customer customer = null, CustomerUser customerUser = null)
        {
            // WIP
            return false;
        }
        public static B2BUser castToB2BUser(int customerID, string name, string email, bool isCustomerUser, int testCompleted)
        {
            try
            {
                B2BUser user = new B2BUser();
                user.customerID = customerID;
                user.name = name;
                user.email = email;
                user.isCustomerUser = isCustomerUser;
                user.testsCompleted = testCompleted;
                return user;
            }
            catch (Exception e)
            {
                throw new Exception("Could not cast to B2B user: " + e.Message);
            }
        }

    } // end class B2B

    public class B2BUser // WIP
    {
        public int customerID { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public bool isCustomerUser { get; set; }
        public int testsCompleted { get; set; }

    } // end of class B2BUser
}// end namespace