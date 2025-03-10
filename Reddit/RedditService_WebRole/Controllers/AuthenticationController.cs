using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using UserService_Data;

namespace RedditService_WebRole.Controllers
{
    public class AuthenticationController : Controller
    {
        TokenService tokenService = new TokenService();
        UserDataRepository repo = new UserDataRepository();

        public ActionResult Register()
        {
            return View("Register");
        }

        public ActionResult Login()
        {

            return View("Login");
        }

        [HttpPost]
        public ActionResult Create(string Ime, string Prezime, string Adresa, string Grad, string Drzava, string Broj_telefona, string Email, string Password, HttpPostedFileBase file)
        {
            try
            {
                string RowKey = Email;
                if (repo.Exists(RowKey))
                {
                    return View("Error");
                }

                string uniqueBlobName = string.Format("image_{0}", RowKey);
                var storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("DataConnectionString"));
                CloudBlobClient blobStorage = storageAccount.CreateCloudBlobClient();
                CloudBlobContainer container = blobStorage.GetContainerReference("vezba");

                container.CreateIfNotExists();

                CloudBlockBlob blob = container.GetBlockBlobReference(uniqueBlobName);
                blob.Properties.ContentType = file.ContentType;

                file.InputStream.Seek(0, SeekOrigin.Begin);

                blob.UploadFromStream(file.InputStream);

                User entry = new User(RowKey)
                {
                    Ime = Ime,
                    Prezime = Prezime,
                    Adresa = Adresa,
                    Grad = Grad,
                    Drzava = Drzava,
                    Broj_telefona = Broj_telefona,
                    Email = Email,
                    Password = Password,
                    PhotoUrl = blob.Uri.ToString()
                };
                repo.AddUser(entry);

                CloudQueue queue = QueueHelper.GetQueueReference("vezba");
                queue.AddMessage(new CloudQueueMessage(RowKey), null, TimeSpan.FromMilliseconds(30));

                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                // Log the exception details for debugging purposes
                System.Diagnostics.Debug.WriteLine("An error occurred: " + ex.Message);
                System.Diagnostics.Debug.WriteLine("Stack Trace: " + ex.StackTrace);
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine("Inner Exception: " + ex.InnerException.Message);
                    System.Diagnostics.Debug.WriteLine("Inner Stack Trace: " + ex.InnerException.StackTrace);
                }

                return View("Register");
            }
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult Authenticate(string Email, string Password)
        {
            try
            {
                // Provera da li korisnik postoji u bazi
                 User user = repo.RetrieveAllUsers().Where(s => s.RowKey == Email).FirstOrDefault();
                if (user == null)
                {
                    return Json(new { error = "User not found" });
                }

                // Provera da li je uneta lozinka ispravna
                if (user.Password != Password)
                {
                    return Json(new { error = "Invalid password" });
                }

                // Generisanje JWT tokena
                string token = tokenService.GenerateJwtToken(user);

                Response.Headers.Add("Authorization", "Bearer " + token);

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }



    }
}