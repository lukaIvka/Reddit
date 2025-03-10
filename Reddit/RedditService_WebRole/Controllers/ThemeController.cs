using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
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
    public class ThemeController : Controller
    {
        TokenService tokenService = new TokenService();
        ThemeDataRepository repo = new ThemeDataRepository();
        CommentDataRepository commentRepo = new CommentDataRepository();
        UserDataRepository userRepo = new UserDataRepository();
        SubscriptionRepository subRepo = new SubscriptionRepository();
        VotesDataRepository vote_repo = new VotesDataRepository();
        public int counter = 0;

        // GET: Theme
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult NewTheme()
        {
            return View("NewTheme");
        }

        [HttpPost]
        public ActionResult SaveTheme(string title, string content, HttpPostedFileBase image, string token)
        {
            if (!string.IsNullOrEmpty(title) && !string.IsNullOrEmpty(content) && image != null)
            {
                try
                {
                    Topic topic = new Topic();
                    string uniqueBlobName = string.Format("image_{0}", topic.RowKey);
                    var storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("DataConnectionString"));
                    CloudBlobClient blobStorage = storageAccount.CreateCloudBlobClient();
                    CloudBlobContainer container = blobStorage.GetContainerReference("vezba");

                    container.CreateIfNotExists();

                    CloudBlockBlob blob = container.GetBlockBlobReference(uniqueBlobName);
                    blob.Properties.ContentType = image.ContentType;

                    image.InputStream.Seek(0, SeekOrigin.Begin);

                    blob.UploadFromStream(image.InputStream);

                    topic.Title = title;
                    topic.Content = content;
                    topic.PhotoUrl = blob.Uri.ToString();
                    topic.Publisher = tokenService.GetEmailFromToken(token);
                    topic.Time_published = DateTime.Now;
                    topic.Upvote = 0;
                    topic.Downvote = 0;
                    topic.Comments = new List<Comment>();

                    repo.AddTheme(topic);

                    CloudQueue queue = QueueHelper.GetQueueReference("vezba");
                    queue.AddMessage(new CloudQueueMessage(topic.RowKey), null, TimeSpan.FromMilliseconds(30));
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message + e.StackTrace);
                }

                return Json(new { success = true, message = "Topic is successfully saved" });
            }
            else
            {
                return Json(new { success = false, message = "You have not sent all the required information." });
            }
        }

        public ActionResult LoadThemes(string sortOrder)
        {
            try
            {
                var themes = repo.RetrieveAllThemes().ToList();

                if (sortOrder == null || sortOrder.ToLower() == "asc")
                {

                    themes = themes.OrderBy(t => t.Title).ToList();
                }
                else
                {
                    themes = themes.OrderByDescending(t => t.Title).ToList();
                }

                return Json(new { success = true, themes }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message + e.StackTrace);
                return Json(new { success = false, message = "Failed to load themes" }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult DeleteTheme(string themeId, string token)
        {
            try
            {
                if (!tokenService.ValidateJwtToken(token))
                    return Json(new { success = false, message = "Token validation failed" }, JsonRequestBehavior.AllowGet);
                Debug.WriteLine("rowkey provera: " + themeId);

                var themes = repo.RetrieveAllThemes().ToList();
                foreach (var t in themes)
                {
                    if (t.RowKey.Equals(themeId))
                    {
                        repo.DeleteTheme(t);
                        return Json(new { success = true }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message + "\n" + e.StackTrace);
                return Json(new { success = false, message = "Post deleting failed" }, JsonRequestBehavior.AllowGet);
            }



            return Json(new { success = false, message = "Post deleting failed" }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetComments(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return Json(new { success = false, message = "Theme not found" }, JsonRequestBehavior.AllowGet);
                }

                var comments = commentRepo.RetrieveAllComments().Where(c => c.ThemeOwner == id).ToList();
                var commentData = comments.Select(c => new
                {
                    c.Content,
                    c.Upvote,
                    c.Downvote,
                    c.Publisher,
                    c.RowKey
                }).ToList();

                return Json(new { success = true, comments = commentData }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.StackTrace + e.Message);
                return new HttpStatusCodeResult(500, "Internal server error");
            }
        }


        [HttpPost]
        public ActionResult SubmitComment(string token, string themeId, string content)
        {
            try
            {
                if (!tokenService.ValidateJwtToken(token))
                {
                    return Json(new { success = false, message = "Invalid token" });
                }

                string username = tokenService.GetUsernameFromToken(token);
                string email = userRepo.RetrieveAllUsers().FirstOrDefault(u => u.Ime == username)?.Email;
                Topic topic = repo.RetrieveAllThemes().FirstOrDefault(t => t.RowKey == themeId);
                string id = themeId + email;
                Comment newComment = new Comment()
                {
                    Publisher = email,
                    Content = content,
                    ThemeOwner = themeId,
                    Upvote = 0,
                    Downvote = 0,
                };

                if (commentRepo.Exists(id))
                {
                    counter++;
                    newComment.RowKey += counter.ToString();
                }
                commentRepo.AddComment(newComment);
                if (topic.Comments == null)
                {
                    topic.Comments = new List<Comment>();
                }
                topic.Comments.Add(newComment);
                repo.UpdateTopic(topic);
                return Json(new { success = true });
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.StackTrace + e.Message);
                return Json(new { success = false, message = e.Message });
            }
        }


        [HttpPost]
        public ActionResult Details(string token, string id)
        {
            try
            {
                string username = tokenService.GetUsernameFromToken(token);
                var theme = repo.RetrieveAllThemes().FirstOrDefault(t => t.RowKey == id);
                string email = userRepo.RetrieveAllUsers().FirstOrDefault(u => u.Ime == username)?.Email;
                var comments = commentRepo.RetrieveAllComments().Where(c => c.Publisher == email).ToList();
                List<Comment> newListComments = new List<Comment>();
                foreach (Comment c in comments)
                {
                    if (c.ThemeOwner == id)
                    {
                        newListComments.Add(c);
                    }
                }
                if (theme == null)
                {
                    return HttpNotFound();
                }

                var themeData = new
                {
                    theme.RowKey,
                    theme.Time_published,
                    theme.Publisher,
                    theme.Title,
                    theme.Content,
                    theme.Upvote,
                    theme.Downvote,
                    theme.PhotoUrl,
                    Comments = newListComments
                };

                return Json(new { success = true, theme = themeData }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { success = false, message = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult GetThemeDetails(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return Json(new { success = false, message = "Theme not found" }, JsonRequestBehavior.AllowGet);
                }

                var theme = repo.RetrieveAllThemes().FirstOrDefault(t => t.RowKey == id);
                if (theme == null)
                {
                    return Json(new { success = false, message = "Theme not found" }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { success = true, theme = theme }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.StackTrace + e.Message);
                return new HttpStatusCodeResult(500, "Internal server error");
            }
        }

        [HttpPost]
        public ActionResult AddSubscriber(string themeId, string token)
        {
            try
            {
                if (!tokenService.ValidateJwtToken(token))
                    return Json(new { success = false, message = "Token validation failed" }, JsonRequestBehavior.AllowGet);

                var email = tokenService.GetEmailFromToken(token);

                Subscription sub = new Subscription();

                sub.ThemeId = themeId;
                sub.UserId = email;

                Debug.WriteLine("Provera: " + sub.UserId + sub.ThemeId + sub.RowKey);

                var subs = subRepo.RetrieveAllSubscriptions().ToList();
                if (!subRepo.SubscriptionExists(themeId, email))
                    subRepo.AddSubscription(sub);
                else
                    return Json(new { success = false, message = "You are already subscribed to this post." }, JsonRequestBehavior.AllowGet);
                return Json(new { success = true, message = "Subscription added." }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message + "\n" + e.StackTrace);
                return Json(new { success = false, message = "Post subscription failed" }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult DeleteSubscriber(string themeId, string token)
        {
            try
            {
                if (!tokenService.ValidateJwtToken(token))
                    return Json(new { success = false, message = "Token validation failed" }, JsonRequestBehavior.AllowGet);

                var email = tokenService.GetEmailFromToken(token);

                var subs = subRepo.RetrieveAllSubscriptions().ToList();

                Debug.WriteLine("Check email and themeId: " + email + themeId);
                foreach (var s in subs)
                {
                    if (s.ThemeId.Equals(themeId) && s.UserId.Equals(email))
                    {
                        subRepo.DeleteSubscription(s);
                        return Json(new { success = true, message = "Subscription deleted." }, JsonRequestBehavior.AllowGet);
                    }
                }

                return Json(new { success = false, message = "You are not subscribed to this post." }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message + "\n" + e.StackTrace);
                return Json(new { success = false, message = "Post unsubscription failed" }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetAllSubscriptions()
        {
            try
            {
                var subs = subRepo.RetrieveAllSubscriptions().ToList();
                Debug.WriteLine("broj elemenata u sub:" + subs.Count.ToString());
                return Json(new { success = true, subs }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { success = false, message = e.StackTrace + e.Message }, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpPost]
        public ActionResult DeleteComment(string token, string commentId)
        {
            string username = tokenService.GetUsernameFromToken(token);
            User user = userRepo.RetrieveAllUsers().FirstOrDefault(u => u.Ime == username);
            Comment comment = commentRepo.RetrieveAllComments().FirstOrDefault(c => c.RowKey == commentId);
            string topicId = comment.ThemeOwner;
            Topic topic = repo.RetrieveAllThemes().FirstOrDefault(t => t.RowKey == topicId);
            User owner = userRepo.RetrieveAllUsers().FirstOrDefault(u => u.RowKey == topic.Publisher);

            if (user == null || comment == null || topic == null || owner == null) // Check if any required objects are null
            {
                return Json(new { success = false, message = "You are not logged in or the comment does not exist" });
            }

            if (user.RowKey == comment.Publisher || user.RowKey == owner.RowKey)
            {
                commentRepo.DeleteComment(comment);
                if (topic.Comments != null)
                {
                    topic.Comments.Remove(comment);
                }
            }
            return Json(new { success = true });
        }

        [HttpPost]
        public ActionResult UpvoteComment(string token, string rowKey)
        {
            string username = tokenService.GetUsernameFromToken(token);
            User user = userRepo.RetrieveAllUsers().FirstOrDefault(s => s.Ime == username);

            if (user != null)
            {
                Comment comment = commentRepo.RetrieveAllComments().FirstOrDefault(s => s.RowKey == rowKey);
                string voteId = user.RowKey + comment.RowKey;
                if (vote_repo.Exists(voteId))
                {
                    Votes newVote = vote_repo.RetrieveAllVotes().FirstOrDefault(s => s.RowKey == voteId);
                    if (newVote.VoteType == "upvote")
                    {
                        comment.Upvote -= 1;
                        commentRepo.UpdateComment(comment);
                        vote_repo.DeleteVote(newVote.RowKey);
                        return Json(new { success = true, change = false, newValue = false, user = true, message = "Upvote count and token received successfully." });
                    }
                    else
                    {
                        newVote.VoteType = "upvote";
                        comment.Downvote -= 1;
                        comment.Upvote += 1;
                        commentRepo.UpdateComment(comment);
                        vote_repo.UpdateVote(newVote);
                        return Json(new { success = true, change = true, newValue = true, user = true, message = "Upvote count and token received successfully." });
                    }
                }
                else
                {
                    Votes vote = new Votes(voteId)
                    {
                        UserId = user.RowKey,
                        ThemeId = comment.RowKey, // mjesto za commentId u tabeli ali tamo je samo za teme
                        VoteType = "upvote"
                    };
                    comment.Upvote++;
                    commentRepo.UpdateComment(comment);
                    vote_repo.AddVote(vote);
                    // Votes vote = new Votes(user.RowKey, topic.RowKey, voteId, "upvote");
                }



                return Json(new { success = true, change = true, newValue = false, user = true, message = "Upvote count and token received successfully." });
            }
            return Json(new { success = true, change = true, newValue = false, user = false, message = "Upvote count and token received successfully." });
        }

        [HttpPost]
        public ActionResult DownvoteComment(string token, string rowKey)
        {
            string username = tokenService.GetUsernameFromToken(token);
            User user = userRepo.RetrieveAllUsers().FirstOrDefault(s => s.Ime == username);
            if (user != null)
            {
                Comment comment = commentRepo.RetrieveAllComments().FirstOrDefault(s => s.RowKey == rowKey);
                string voteId = user.RowKey + comment.RowKey;
                if (vote_repo.Exists(voteId))
                {
                    Votes newVote = vote_repo.RetrieveAllVotes().FirstOrDefault(s => s.RowKey == voteId);
                    if (newVote.VoteType == "downvote")
                    {
                        comment.Downvote -= 1;
                        commentRepo.UpdateComment(comment);
                        vote_repo.DeleteVote(newVote.RowKey);
                        return Json(new { success = true, change = false, newValue = false, user = true, message = "Upvote count and token received successfully." });

                    }
                    else
                    {
                        newVote.VoteType = "downvote";
                        comment.Downvote += 1;
                        comment.Upvote -= 1;
                        commentRepo.UpdateComment(comment);
                        vote_repo.UpdateVote(newVote);
                        return Json(new { success = true, change = true, newValue = true, user = true, message = "Upvote count and token received successfully." });
                    }
                }
                else
                {
                    Votes vote = new Votes(voteId)
                    {
                        UserId = user.RowKey,
                        ThemeId = comment.RowKey,
                        VoteType = "downvote"
                    };
                    comment.Downvote++;
                    commentRepo.UpdateComment(comment);
                    vote_repo.AddVote(vote);

                }



                return Json(new { success = true, change = true, newValue = false, user = true, message = "Upvote count and token received successfully." });
            }
            return Json(new { success = true, change = true, newValue = false, user = false, message = "Upvote count and token received successfully." });
        }



    }
}
