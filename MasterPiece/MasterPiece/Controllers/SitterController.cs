using MasterPiece.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace MasterPiece.Controllers
{
    public class SitterController : Controller
    {
        private MasterPeiceEntities db = new MasterPeiceEntities();
         
       
        // GET: Sitter
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(string email, string password)
        {
          
            var sitter = db.Sitters.FirstOrDefault(s => s.Email == email);

            if (sitter == null)
            {
                TempData["ErrorMessage"] = "Email not found.";
                return RedirectToAction("Login", "Sitter");
            }

           
            string hashedPassword = HashPassword(password);

           
            if (sitter.PasswordHash != hashedPassword)
            {
                TempData["ErrorMessage"] = "Invalid password.";
                return RedirectToAction("Login", "Sitter");
            }

           
            Session["SitterID"] = sitter.SitterID;
            Session["SitterName"] = sitter.FirstName;
            return RedirectToAction("Profile","Sitter");
        }

        public ActionResult Logout()
        {
            Session["SitterID"] = null;
            Session["SitterName"] = null;
            return RedirectToAction("Login","Sitter");
        }


        public ActionResult Profile()
        {
            
            var sitterId = Session["SitterID"];
            if (sitterId == null)
            {
                
                return RedirectToAction("Login", "Sitter");
            }

           
            var sitter = db.Sitters.Find(sitterId);
            if (sitter == null)
            {
                
                return HttpNotFound();
            }

           
            return View(sitter);
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Profile(Sitter model, HttpPostedFileBase imageFile)
        {
           
            if (ModelState.IsValid)
            {
                
                var sitter = db.Sitters.Find(model.SitterID);
                if (sitter != null)
                {
                   
                    sitter.PhoneNumber = model.PhoneNumber;
                    sitter.Bio = model.Bio;
                    sitter.ExperienceYears = model.ExperienceYears;
                    sitter.HourlyRate = model.HourlyRate;

                   
                    if (imageFile != null && imageFile.ContentLength > 0)
                    {
                        string fileName = Path.GetFileName(imageFile.FileName);
                        string path = Path.Combine(Server.MapPath("~/Images/"), fileName);
                        imageFile.SaveAs(path);
                        sitter.sitterImage = fileName;
                    }

                   
                    db.SaveChanges();

                    
                    return RedirectToAction("Profile","Sitter");
                }
            }

          
            return View(model);
        }


        public ActionResult Bookings()
        {
            var sitterId = Session["SitterID"];
            if (sitterId == null)
            {
                return RedirectToAction("Login", "Sitter");
            }

            var bookings = db.Bookings
                .Where(b => b.SitterID == (int)sitterId)
                .Include(b => b.User)
                .Include(b => b.ServiceDetail)
                .ToList();

            return View(bookings);
        }








        public ActionResult Edit(int? id)
        {
            
            var sitterId = (int)Session["SitterID"];
            if (sitterId == null || id == null || sitterId != (int)id)
            {
                
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            
            var sitter = db.Sitters.Find((int)id);
            if (sitter == null)
            {
                
                return HttpNotFound();
            }

            
            var editViewModel = new EditSitterViewModel
            {
                SitterID = sitter.SitterID,
                FirstName = sitter.FirstName,
                LastName = sitter.LastName,
                Email = sitter.Email,
                PhoneNumber = sitter.PhoneNumber,
                Bio = sitter.Bio,
                ExperienceYears = sitter.ExperienceYears,
                HourlyRate=sitter.HourlyRate,

            };

            return View(editViewModel);
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, EditSitterViewModel model)
        {
            
            var sitterId = (int)Session["SitterID"];
            if (sitterId != id)
            {
                
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            
            var sitter = db.Sitters.Find(id);
            if (sitter == null)
            {
                
                return HttpNotFound();
            }

            
            if (ModelState.IsValid)
            {
                
                sitter.FirstName = model.FirstName;
                sitter.LastName = model.LastName;
                sitter.Email = model.Email;
                sitter.PhoneNumber = model.PhoneNumber;
                sitter.Bio = model.Bio;
                sitter.ExperienceYears = model.ExperienceYears;
                sitter.HourlyRate=model.HourlyRate;


                if (model.ImageFile != null && model.ImageFile.ContentLength > 0)
                {
                    string fileName = Path.GetFileName(model.ImageFile.FileName);
                    string path = Path.Combine(Server.MapPath("~/Images/"), fileName);

                    
                    if (!Directory.Exists(Path.Combine(Server.MapPath("~/Images/"))))
                    {
                        Directory.CreateDirectory(Path.Combine(Server.MapPath("~/Images/")));
                    }

                   
                    model.ImageFile.SaveAs(path);
                    sitter.sitterImage = fileName;
                }

               
                db.Entry(sitter).State = EntityState.Modified;
                db.SaveChanges();

                TempData["SuccessMessage"] = "Profile updated successfully.";
                return RedirectToAction("Profile");
            }

            return View(model);
        }


        public ActionResult ResetPassword()
        {
            var sitterId = Session["SitterID"];
            if (sitterId == null)
            {
                return RedirectToAction("Login","Sitter");
            }

            return View();
        }

      
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(string oldPassword, string newPassword, string confirmPassword)
        {
            var sitterId = Session["SitterID"];
            if (sitterId == null)
            {
                return RedirectToAction("Login","Sitter");
            }

            var sitter = db.Sitters.Find(sitterId);
            if (sitter == null)
            {
                return HttpNotFound();
            }

            string hashedOldPassword = HashPassword(oldPassword);
            if (sitter.PasswordHash != hashedOldPassword)
            {
                ModelState.AddModelError("", "Old password is incorrect.");
                return View();
            }

            
            if (newPassword != confirmPassword)
            {
                ModelState.AddModelError("", "New password and confirmation password do not match.");
                return View();
            }

          
            sitter.PasswordHash = HashPassword(newPassword);
            db.Entry(sitter).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();

            ViewBag.SuccessMessage = "Password has been reset successfully.";
            return View();
        }

      
        // Helper Method for Hashing Password
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }
    }
}