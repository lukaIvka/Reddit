using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using UserService_Data;

namespace RedditService_WebRole.Controllers
{
    public class UserController : Controller
    {
        UserDataRepository repo = new UserDataRepository();
        CommentDataRepository commentRepo = new CommentDataRepository();
        VotesDataRepository vote_repo = new VotesDataRepository();
        ThemeDataRepository theme_repo = new ThemeDataRepository();
        TokenService tokenservice = new TokenService();
        static string clientToken;
        public static string username;
        // GET: User
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult UserPage()
        {
            return View();
        }

        [HttpPost]
        public ActionResult SaveToken(string token)
        {
            clientToken = token;
            if (token != null)
            {
                username = tokenservice.GetUsernameFromToken(clientToken);
                Debug.WriteLine("Token" + token);
                Debug.WriteLine("Username: " + username);
            }

            return View("UserPage");
        }

        public ActionResult ProfileTO()
        {
            return View("UserProfile");
        }

        [HttpPost]
        public ActionResult ProfilePage(string token)
        {
            Debug.WriteLine("Ulazak u akciju Profile Page");
            try
            {
                if (string.IsNullOrEmpty(token))
                {
                    return Json(new { error = "Token not provided" });
                }

                if (!tokenservice.ValidateJwtToken(token))
                {
                    Debug.WriteLine("Token nije validiran");
                    return Json(new { error = "Invalid token" });
                }

                string username = tokenservice.GetUsernameFromToken(token);
                User currentUser = repo.RetrieveAllUsers().FirstOrDefault(s => s.Ime == username);

                Debug.WriteLine(currentUser.Ime + " " + currentUser.Prezime);
                if (currentUser == null)
                {
                    Debug.WriteLine("Korisnik nije pronadjen");
                    return Json(new { error = "User not found" });
                }

                return Json(currentUser);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.StackTrace + e.Message);
                return Json(new { error = e.Message });
            }
        }
        public ActionResult UserProfile()
        {
            return View();
        }


        public ActionResult ProfileLogin(string Email)
        {
            return View("UserProfile");
        }

        [HttpPost]
        public ActionResult Update(User user, HttpPostedFileBase file, string OldPassword)
        {
            User old_user = repo.RetrieveAllUsers().FirstOrDefault(s => s.Password == OldPassword);
            if (file != null && file.ContentLength > 0)
            {

                string uploadsFolderPath = Server.MapPath("~/Uploads");
                string fileName = Path.GetFileName(file.FileName);
                string filePath = Path.Combine(uploadsFolderPath, fileName);


                file.SaveAs(filePath);


                old_user.PhotoUrl = Url.Content("~/Uploads/" + fileName);
            }
            if (user.Password != null)
            {
                old_user.Password = user.Password;
            }
            if (user.Ime != null && user.Prezime != null && user.Email != null && user.Drzava != null && user.Broj_telefona != null && user.Adresa != null && user.Grad != null)
            {
                old_user.Ime = user.Ime;
                old_user.Prezime = user.Prezime;
                old_user.Grad = user.Grad;
                old_user.Drzava = user.Drzava;
                old_user.Adresa = user.Adresa;
                old_user.Email = user.Email;
                old_user.RowKey = user.Email;
                old_user.Broj_telefona = user.Broj_telefona;

            }
            repo.UpdateUser(old_user);
            string newToken = tokenservice.GenerateJwtToken(user);
            return Json(new { token = newToken });

            
        }
        [HttpPost]
        public ActionResult RedirectToEdit(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return Json(new { error = "Token not provided" });
            }
             string username = tokenservice.GetUsernameFromToken(token);
            User user = repo.RetrieveAllUsers().FirstOrDefault(s => s.Ime == username);
            return Json(new { success = true, email = user.Email });
        }


        public ActionResult UpdateUser(string email)
        {
            User user = repo.RetrieveAllUsers().FirstOrDefault(s => s.Email == email);
            return View(user);
        }


        [HttpPost]
        public ActionResult Upvote(string token, string rowKey)
        {
            string username = tokenservice.GetUsernameFromToken(token);
            User user = repo.RetrieveAllUsers().FirstOrDefault(s => s.Ime == username);
            
            if (user!=null)
            {
                Topic topic = theme_repo.RetrieveAllThemes().FirstOrDefault(s => s.RowKey == rowKey);
                string voteId = user.RowKey + topic.RowKey;
                if (vote_repo.Exists(voteId))
                {
                    Votes newVote = vote_repo.RetrieveAllVotes().FirstOrDefault(s => s.RowKey == voteId);
                    if (newVote.VoteType == "upvote")
                    {
                        topic.Upvote -=1;
                        theme_repo.UpdateTopic(topic);
                        vote_repo.DeleteVote(newVote.RowKey);
                        return Json(new { success = true, change = false, newValue = false, user = true, message = "Upvote count and token received successfully." });
                    }
                    else
                    {
                        newVote.VoteType = "upvote";
                        topic.Downvote -= 1;
                        topic.Upvote += 1;
                        theme_repo.UpdateTopic(topic);
                        vote_repo.UpdateVote(newVote);
                        return Json(new { success = true, change = true, newValue = true, user = true, message = "Upvote count and token received successfully." });
                    }
                }
                else
                {
                    Votes vote = new Votes(voteId)
                    {
                        UserId = user.RowKey,
                        ThemeId = topic.RowKey,
                        VoteType = "upvote"
                    };
                    topic.Upvote++;
                    theme_repo.UpdateTopic(topic);
                    vote_repo.AddVote(vote);
                    // Votes vote = new Votes(user.RowKey, topic.RowKey, voteId, "upvote");
                }



                return Json(new { success = true, change = true, newValue = false, user = true, message = "Upvote count and token received successfully." });
            }
            return Json(new { success = true, change = true, newValue = false, user = false, message = "Upvote count and token received successfully." });
        }


        [HttpPost]
        public ActionResult Downvote(string token, string rowKey)
        {
            string username = tokenservice.GetUsernameFromToken(token);
            User user = repo.RetrieveAllUsers().FirstOrDefault(s => s.Ime == username);
            if (user != null)
            {
                Topic topic = theme_repo.RetrieveAllThemes().FirstOrDefault(s => s.RowKey == rowKey);
                string voteId = user.RowKey + topic.RowKey;
                if (vote_repo.Exists(voteId))
                {
                    Votes newVote = vote_repo.RetrieveAllVotes().FirstOrDefault(s => s.RowKey == voteId);
                    if (newVote.VoteType == "downvote")
                    {
                        topic.Downvote -= 1;
                        theme_repo.UpdateTopic(topic);
                        vote_repo.DeleteVote(newVote.RowKey);
                        return Json(new { success = true, change = false, newValue = false, user = true, message = "Upvote count and token received successfully." });

                    }
                    else
                    {
                        newVote.VoteType = "downvote";
                        topic.Downvote += 1;
                        topic.Upvote -= 1;
                        theme_repo.UpdateTopic(topic);
                        vote_repo.UpdateVote(newVote);
                        return Json(new { success = true, change = true, newValue = true, user = true, message = "Upvote count and token received successfully." });
                    }
                }
                else
                {
                    Votes vote = new Votes(voteId)
                    {
                        UserId = user.RowKey,
                        ThemeId = topic.RowKey,
                        VoteType = "downvote"
                    };
                    topic.Downvote++;
                    theme_repo.UpdateTopic(topic);
                    vote_repo.AddVote(vote);
                   
                }



                return Json(new { success = true, change = true, newValue = false, user = true, message = "Upvote count and token received successfully." });
            }
            return Json(new { success = true, change = true, newValue = false, user = false, message = "Upvote count and token received successfully." });
        }
      
        public ActionResult ThemeDetails(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return HttpNotFound("Theme not found");
                }

                var theme = theme_repo.RetrieveAllThemes().FirstOrDefault(t => t.RowKey == id);
                if (theme == null)
                {
                    return HttpNotFound("Theme not found");
                }

                return View("~/Views/Theme/DisplayTheme.cshtml", theme);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.StackTrace + e.Message);
                return new HttpStatusCodeResult(500, "Internal server error");
            }
        }


        [HttpPost]
        public ActionResult GetThemes(string token)
        {
            try
            {
                if (string.IsNullOrEmpty(token))
                {
                    return Json(new { success = false, message = "Token not provided" });
                }

                if (!tokenservice.ValidateJwtToken(token))
                {
                    return Json(new { success = false, message = "Invalid token" });
                }

                string username = tokenservice.GetUsernameFromToken(token);
                string email = repo.RetrieveAllUsers().FirstOrDefault(u => u.Ime == username)?.Email;
                var topics = theme_repo.RetrieveAllThemes().Where(t => t.Publisher == email).ToList();
                foreach (var topic in topics)
                {
                    // Populate comments for each topic
                    topic.Comments = commentRepo.RetrieveAllComments().Where(c => c.ThemeOwner == topic.RowKey).ToList();
                }
                var topicData = topics.Select(t => new
                {
                    t.Title,
                    t.Content,
                    t.Time_published,
                    t.Upvote,
                    t.Downvote,
                    t.PhotoUrl,
                    t.RowKey,
                    Comments = t.Comments.Select(c => new {
                        c.Content,
                        c.Upvote,
                        c.Downvote,
                        c.ThemeOwner,
                        c.RowKey
                    }).ToList()
                }).ToList();

                return Json(new { success = true, topics = topicData });
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.StackTrace + e.Message);
                return Json(new { success = false, message = e.Message });
            }
        }


        [HttpPost]
        public ActionResult GetComments(string token)
        {
            try
            {
                if (string.IsNullOrEmpty(token))
                {
                    return Json(new { success = false, message = "Token not provided" });
                }

                if (!tokenservice.ValidateJwtToken(token))
                {
                    return Json(new { success = false, message = "Invalid token" });
                }

                string username = tokenservice.GetUsernameFromToken(token);
                string email = repo.RetrieveAllUsers().FirstOrDefault(u => u.Ime == username)?.Email;
                var comments = commentRepo.RetrieveAllComments().Where(c => c.Publisher == email).ToList();

                var commentData = comments.Select(c => new
                {
                    c.Content,
                    c.Upvote,
                    c.Downvote,
                    c.ThemeOwner,
                    c.RowKey // Assuming RowKey as the unique identifier for the comment
                }).ToList();

                return Json(new { success = true, comments = commentData });
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.StackTrace + e.Message);
                return Json(new { success = false, message = e.Message });
            }
        }
    }
}