using MasterPiece.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace MasterPiece.Controllers
{
    public class AccountController : Controller
    {
        private MasterPeiceEntities db = new MasterPeiceEntities();
        public ActionResult Register()
        {
            return View();
        }

        //[HttpPost]
        //public ActionResult Register(User user, string confirmPassword)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        if (user.PasswordHash != confirmPassword)
        //        {
        //            ModelState.AddModelError("confirmPassword", "Passwords do not match.");
        //            ViewBag.ShowPasswordMismatchAlert = true;
        //            return View(user);


        //        }


        //        user.PasswordHash = HashPassword(user.PasswordHash);
        //        user.CreatedAt = DateTime.Now;
        //        user.UserType = "Parent";
        //        db.Users.Add(user);
        //        db.SaveChanges();
        //        TempData["RegistrationSuccess"] = "You have successfully registered. Please login to continue.";

        //        return RedirectToAction("Login");

        //    }
        //    return View(user);
        //}


        [HttpPost]
        public ActionResult Register(User user, string confirmPassword)
        {
            if (ModelState.IsValid)
            {
                var existingUser = db.Users.FirstOrDefault(u => u.Email == user.Email);
                if (existingUser != null)
                {
                    // If email exists, trigger a SweetAlert error and return the view
                    ViewBag.ShowEmailExistsAlert = true;
                    return View(user);
                }

                if (user.PasswordHash != confirmPassword)
                {
                    ModelState.AddModelError("confirmPassword", "Passwords do not match.");
                    ViewBag.ShowPasswordMismatchAlert = true;
                    return View(user);
                }

                user.PasswordHash = HashPassword(user.PasswordHash);
                user.CreatedAt = DateTime.Now;
                user.UserType = "Parent";
                db.Users.Add(user);
                db.SaveChanges();

                TempData["RegistrationSuccess"] = true; 

                return RedirectToAction("Register"); 
            }

            return View(user);
        }

        public ActionResult Login()
        {
            return View();
        }
        //[HttpPost]
        //public ActionResult Login(string email, string password)
        //{

        //    var user = db.Users.FirstOrDefault(u => u.Email == email);

        //    if (user == null)
        //    {
        //        TempData["ErrorMessage"] = "The email you entered does not exist.";
        //    }
        //    else
        //    {

        //        string hashedPassword = HashPassword(password);

        //        if (user.PasswordHash != hashedPassword)
        //        {
        //            TempData["ErrorMessage"] = "The password you entered is incorrect.";
        //        }
        //        else
        //        {


        //            Session["UserId"] = user.UserID;
        //            Session["username"] = user.FirstName;
        //            return RedirectToAction("Index", "Home");
        //        }
        //    }

        //    return RedirectToAction("Login");


        //    //string hashedPassword = HashPassword(password);

        //    //var user = db.Users.FirstOrDefault(u => u.Email == email && u.PasswordHash == hashedPassword);


        //    //if (user != null)
        //    //{

        //    //    Session["UserId"] = user.UserID;
        //    //    return RedirectToAction("Index", "Home");

        //    //}

        //    //ViewBag.ErrorMessage = "Invalid Email or password.";
        //    //return View();


        //}
        //[HttpPost]
        //public ActionResult Login(string email, string password)
        //{

        //    var user = db.Users.FirstOrDefault(u => u.Email == email);

        //    if (user == null)
        //    {
        //        TempData["ErrorMessage"] = "The email you entered does not exist.";
        //    }
        //    else
        //    {

        //        string hashedPassword = HashPassword(password);

        //        if (user.PasswordHash != hashedPassword)
        //        {
        //            TempData["ErrorMessage"] = "The password you entered is incorrect.";
        //        }
        //        else
        //        {
        //            var userId = user.UserID;
        //            var completedBookings = db.Bookings
        //                .Where(b => b.UserID == userId && b.Status == "Completed"
        //                            && !b.Testimonials.Any(t => t.UserID == userId))
        //                .ToList();

        //            if (completedBookings.Any())
        //            {
        //                TempData["ShowTestimonialPopup"] = true;
        //                TempData["CompletedBookingIds"] = completedBookings.Select(b => b.BookingID).ToList(); 
        //            }







        //            if (Session["ServiceDetailID"] != null)
        //            {
        //                Session["UserId"] = user.UserID;
        //                Session["username"] = user.FirstName;
        //                return RedirectToAction("SelectSitter", "Booking");
        //            }
        //            else
        //            {
        //                Session["UserId"] = user.UserID;
        //                Session["username"] = user.FirstName;
        //                return RedirectToAction("Index", "Home");
        //            }
        //        }
        //    }

        //    return RedirectToAction("Login");


        //    //string hashedPassword = HashPassword(password);

        //    //var user = db.Users.FirstOrDefault(u => u.Email == email && u.PasswordHash == hashedPassword);


        //    //if (user != null)
        //    //{

        //    //    Session["UserId"] = user.UserID;
        //    //    return RedirectToAction("Index", "Home");

        //    //}

        //    //ViewBag.ErrorMessage = "Invalid Email or password.";
        //    //return View();


        //}





        //اخر لوجن زابط من هون

        //        [HttpPost]
        //public ActionResult Login(string email, string password)
        //{
        //    var user = db.Users.FirstOrDefault(u => u.Email == email);

        //    if (user == null)
        //    {
        //        TempData["ErrorMessage"] = "The email you entered does not exist.";
        //        return RedirectToAction("Login");
        //    }


        //    string hashedPassword = HashPassword(password);

        //    if (user.PasswordHash != hashedPassword)
        //    {
        //        TempData["ErrorMessage"] = "The password you entered is incorrect.";
        //        return RedirectToAction("Login");
        //    }
        //    else
        //    {

        //        var userId = user.UserID;
        //        var completedBookings = db.Bookings
        //            .Where(b => b.UserID == userId && b.Status == "Completed"
        //                        && !b.Testimonials.Any(t => t.UserID == userId))
        //            .ToList();

        //        if (completedBookings.Any())
        //        {
        //            TempData["ShowTestimonialPopup"] = true;
        //            TempData["CompletedBookingIds"] = completedBookings.Select(b => b.BookingID).ToList();
        //        }


        //        Session["UserId"] = user.UserID;
        //        Session["username"] = user.FirstName;


        //        if (Session["ServiceDetailID"] != null)
        //        {

        //            return RedirectToAction("SelectSitter", "Booking");
        //        }
        //        else
        //        {

        //            return RedirectToAction("Index", "Home");
        //        }
        //    }
        //}

        // تغيرت مع اخر بوكينغ شيلي الجدادا ورجعي هدول اخر لوجن زابط لعند هون



        [HttpPost]
    public ActionResult Login(string email, string password)
    {

            var user = db.Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                TempData["ErrorMessage"] = "The email you entered does not exist.";
                return RedirectToAction("Login");
            }

            string hashedPassword = HashPassword(password);
            if (user.PasswordHash != hashedPassword)
            {
                TempData["ErrorMessage"] = "The password you entered is incorrect.";
                return RedirectToAction("Login");
            }

            Session["UserId"] = user.UserID;
            Session["username"] = user.FirstName;

            var userId = user.UserID;
            var completedBookings = db.Bookings
                .Where(b => b.UserID == userId && b.Status == "Completed"
                            && !b.Testimonials.Any(t => t.UserID == userId))
                .ToList();

            if (completedBookings.Any())
            {
                TempData["ShowTestimonialPopup"] = true;
                TempData["CompletedBookingIds"] = completedBookings.Select(b => b.BookingID).ToList();
            }

            if (Session["PendingBooking"] != null)
            {
                dynamic pendingBooking = Session["PendingBooking"];

                var booking = new Booking
                {
                    UserID = userId,
                    SitterID = pendingBooking.SitterID,
                    ServiceID = pendingBooking.ServiceID,
                    DetailID = pendingBooking.DetailID,
                    BookingDate = DateTime.Now,
                    Status = "Pending",
                    CreatedAt = DateTime.Now,
                    TotalAmount = pendingBooking.TotalAmount
                };

                db.Bookings.Add(booking);
                db.SaveChanges();

                Session["PendingBooking"] = null;

                return RedirectToAction("Payment", "Booking", new { bookingId = booking.BookingID });
            }

            //if (Session["ServiceDetailID"] != null)
            //{
            //    return RedirectToAction("SelectSitter", "Booking");
            //}

            return RedirectToAction("Index", "Home");
        }
    



            private string HashPassword(string password)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }
        public ActionResult Logout()
        {
            Session["UserId"] = null;
            return RedirectToAction("Index", "Home");
        }






        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Profile(HttpPostedFileBase imageFile)
        //{
        //    var userId = Session["UserId"];
        //    if (userId == null)
        //    {
        //        return RedirectToAction("Login");
        //    }

        //    var user = db.Users.Find(userId);
        //    if (user == null)
        //    {
        //        return HttpNotFound();
        //    }

        //    // Update the profile fields


        //    if (imageFile != null && imageFile.ContentLength > 0)
        //    {
        //        var fileName = Path.GetFileName(imageFile.FileName);
        //        var uniqueFileName = Guid.NewGuid().ToString() + "_" + fileName;
        //        var path = Path.Combine(Server.MapPath("~/Images/"), uniqueFileName);

        //        // Save the new image
        //        imageFile.SaveAs(path);

        //        // Delete the old image file if it exists
        //        if (!string.IsNullOrEmpty(user.UserImage))
        //        {
        //            var oldImagePath = Path.Combine(Server.MapPath("~/Images"), user.UserImage);
        //            if (System.IO.File.Exists(oldImagePath))
        //            {
        //                System.IO.File.Delete(oldImagePath);
        //            }
        //        }

        //        // Update the user's profile with the new image
        //        user.UserImage = uniqueFileName;
        //        db.Entry(user).State = EntityState.Modified;
        //        db.SaveChanges();
        //    }


        //    ViewBag.SuccessMessage = "Profile updated successfully.";


        //    return RedirectToAction("Profile"); ;
        //}


        public ActionResult Profile()
        {
            var userId = Session["UserId"];
            if (userId == null)
            {
                return RedirectToAction("Login");
            }

            var user = db.Users.Find(userId);
            if (user == null)
            {
                return HttpNotFound();
            }

            return View(user);
        }





        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Profile(User model, HttpPostedFileBase imageFile)
        {
            if (ModelState.IsValid)
            {
               
                var user = db.Users.Find(model.UserID);

                if (user != null)
                {
                    
                    user.PhoneNumber = model.PhoneNumber;
                    user.Address = model.Address;

                    
                    if (imageFile != null && imageFile.ContentLength > 0)
                    {
                        string fileName = Path.GetFileName(imageFile.FileName);
                        string path = Path.Combine(Server.MapPath("~/Images/"), fileName);
                        imageFile.SaveAs(path);
                        user.UserImage = fileName;
                    }

                    
                    db.SaveChanges();
                    return RedirectToAction("Profile");
                }
            }

            return View(model); 
        }

        public ActionResult YourBookings()
        {
            int userId = (int)Session["UserId"]; 
           
            var bookings = db.Bookings
                             .Include(b => b.Sitter)
                             .Include(b => b.ServiceDetail)
                             .Where(b => b.UserID == userId)
                             .ToList();

            return View(bookings);
        }


        //public ActionResult Edit(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    User userinfo = db.Users.Find(id);
        //    if (userinfo == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(userinfo);
        //}

        //// POST: Account/Edit/5
        //// To protect from overposting attacks, enable the specific properties you want to bind to, for 
        //// more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Edit( User userinfo)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        db.Entry(userinfo).State = EntityState.Modified;
        //        db.SaveChanges();
        //        return RedirectToAction("Index");
        //    }
        //    return View(userinfo);
        //}

        public ActionResult Edit(int? id)
        {
            var userId = (int)Session["UserId"];
            if (userId == null || id == null || userId != (int)id) 
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var user = db.Users.Find((int)id);
            if (user == null)
            {
                return HttpNotFound();
            }

            var editViewModel = new EditViewModel
            {
                UserID = user.UserID,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address
            };

            return View(editViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, EditViewModel model)
        {
            var userId = (int)Session["UserId"];
            if (userId != id) 
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            if (ModelState.IsValid)
            {

                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.Email = model.Email;
                user.PhoneNumber = model.PhoneNumber;
                user.Address = model.Address;

               
                if (model.ImageFile != null && model.ImageFile.ContentLength > 0)
                {
                    string fileName = Path.GetFileName(model.ImageFile.FileName);
                    string path = Path.Combine(Server.MapPath("~/Images/"), fileName);

                    
                    if (!Directory.Exists(Path.Combine(Server.MapPath("~/Images/"))))
                    {
                        Directory.CreateDirectory(Path.Combine(Server.MapPath("~/Images/")));
                    }

                    model.ImageFile.SaveAs(path);
                    user.UserImage = fileName;
                }

                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
                TempData["SuccessMessage"] = "Profile updated successfully.";
                return RedirectToAction("Profile");
            }

           
            return View(model);
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult ResetPassword(string oldPassword, string newPassword, string confirmPassword)
        //{
        //    var userId = Session["UserId"];
        //    if (userId == null)
        //    {
        //        return RedirectToAction("Login");
        //    }

        //    var user = db.Users.Find(userId);
        //    if (user == null)
        //    {
        //        return HttpNotFound();
        //    }

        //    // Hash the old password to compare with the stored hash
        //    string hashedOldPassword = HashPassword(oldPassword);
        //    if (user.PasswordHash != hashedOldPassword)
        //    {
        //        ModelState.AddModelError("", "Old password is incorrect.");
        //        return View();
        //    }

        //    if (newPassword != confirmPassword)
        //    {
        //        ModelState.AddModelError("", "New password and confirmation password do not match.");
        //        return View();
        //    }

        //    // Hash the new password before saving
        //    user.PasswordHash = HashPassword(newPassword);
        //    db.Entry(user).State = System.Data.Entity.EntityState.Modified;
        //    db.SaveChanges();

        //    ViewBag.SuccessMessage = "Password has been reset successfully.";
        //    return View();
        //}







        public ActionResult ResetPassword()
        {
            var userId = Session["userId"];
            if (userId == null)
            {
                return RedirectToAction("Login");
            }
            return View();
        }





        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(string oldPassword, string newPassword, string confirmPassword)
        {
            var userId = Session["UserId"];
            if (userId == null)
            {
                return RedirectToAction("Login");
            }

            var user = db.Users.Find(userId);
            if (user == null)
            {
                return HttpNotFound();
            }

            string hashedOldPassword = HashPassword(oldPassword);
            if (user.PasswordHash != hashedOldPassword)
            {
                ModelState.AddModelError("", "Old password is incorrect.");
                return View(); 
            }

            if (newPassword != confirmPassword)
            {
                ModelState.AddModelError("", "New password and confirmation password do not match.");
                return View();
            }


            
            user.PasswordHash = HashPassword(newPassword);
            db.Entry(user).State = EntityState.Modified;
            db.SaveChanges();

            ViewBag.SuccessMessage = "Password has been reset successfully.";
            return View(); 
        }

    }
}