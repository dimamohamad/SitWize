using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Web.Helpers;
using System.Web.Mvc;
using MasterPiece.Models;

namespace MasterPiece.Controllers
{
    public class AdminController : Controller
    {
        private MasterPeiceEntities db = new MasterPeiceEntities();

        public ActionResult Index()
        {
            if (Session["AdminID"] == null)
            {
               
                return RedirectToAction("LoginAdmin", "Admin");
            }

            
            ViewBag.AdminName = Session["AdminName"];



            var bookings = db.Bookings
                  .Where(b => b.Status == "Confirmed" || b.Status == "Completed")
                  .ToList();

            // Calculate total bookings
            var totalBookings = bookings.Count;

            // Calculate total revenue (assuming TotalAmount contains the amount and we're taking 25%)
            var totalRevenue = bookings.Where(b => b.TotalAmount.HasValue)
                                       .Sum(b => b.TotalAmount.Value * 0.25M);

            // Group by month for booking counts
            var monthlyBookings = bookings
                .GroupBy(b => b.BookingDate.HasValue ? b.BookingDate.Value.Month : 0)
                .Select(g => new { Month = g.Key, Count = g.Count() })
                .OrderBy(m => m.Month)
                .ToList()
                .Cast<dynamic>()
                .ToList();

            // Group by month for revenue calculation
            var monthlyRevenue = bookings
                .Where(b => b.TotalAmount.HasValue)
                .GroupBy(b => b.BookingDate.HasValue ? b.BookingDate.Value.Month : 0)
                .Select(g => new { Month = g.Key, Total = g.Sum(b => b.TotalAmount.Value * 0.25M) })
                .OrderBy(m => m.Month)
                .ToList()
                .Cast<dynamic>()
                .ToList();

            // Prepare the ViewModel
            var viewModel = new DashboardViewModel
            {
                TotalBookings = totalBookings,
                TotalRevenue = totalRevenue,
                MonthlyBookings = monthlyBookings,
                MonthlyRevenue = monthlyRevenue
            };

            return View(viewModel);
        



        //var bookings = db.Bookings.ToList();

        //var totalBookings = bookings.Count;
        //var totalRevenue = bookings.Where(b => b.TotalAmount.HasValue)
        //                           .Sum(b => b.TotalAmount.Value * 0.25M);

        //var monthlyBookings = bookings
        //    .GroupBy(b => b.BookingDate.HasValue ? b.BookingDate.Value.Month : 0)
        //    .Select(g => new { Month = g.Key, Count = g.Count() })
        //    .OrderBy(m => m.Month)
        //    .ToList()
        //    .Cast<dynamic>()
        //    .ToList();

        //var monthlyRevenue = bookings
        //    .Where(b => b.TotalAmount.HasValue)
        //    .GroupBy(b => b.BookingDate.HasValue ? b.BookingDate.Value.Month : 0)
        //    .Select(g => new { Month = g.Key, Total = g.Sum(b => b.TotalAmount.Value * 0.25M) })
        //    .OrderBy(m => m.Month)
        //    .ToList()
        //    .Cast<dynamic>()
        //    .ToList();

        //var viewModel = new DashboardViewModel
        //{
        //    TotalBookings = totalBookings,
        //    TotalRevenue = totalRevenue,
        //    MonthlyBookings = monthlyBookings,
        //    MonthlyRevenue = monthlyRevenue
        //};

        //return View(viewModel);



    }




        [HttpGet]
        public ActionResult LoginAdmin()
        {
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        public ActionResult LoginAdmin(string email, string password)
        {
            if (ModelState.IsValid)
            {
                

               
                var admin = db.Admins.FirstOrDefault(a => a.Email == email && a.PasswordHash == password);

                if (admin != null)
                {
                   
                    Session["AdminID"] = admin.AdminID;
                    Session["AdminName"] = admin.AdminName;
                    Session["Role"] = admin.Role;

                    return RedirectToAction("Index", "Admin");
                }

               
                ModelState.AddModelError("", "Invalid email or password.");
            }
            return View();
        }

       
        [HttpGet]
        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("LoginAdmin", "Admin");
        }
















        public ActionResult GetAllUsers(string searchTerm = null)
        {
            var users = db.Users.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                users = users.Where(u => u.FirstName.Contains(searchTerm)
                                          || u.LastName.Contains(searchTerm)
                                          || u.Email.Contains(searchTerm)
                                          || u.PhoneNumber.Contains(searchTerm));
            }

            return View(users.ToList());
        }











        //public ActionResult GetAllUsers()
        //{
        //    var users = db.Users.ToList();


        //    return View(users);

        //}




        public ActionResult GetServices()
        {
            var services = db.Services.ToList();

            return View(services);
        }


        public ActionResult GetContacted()
        {
            var contacts = db.ContactUs.ToList();

            return View(contacts);
        }

        public ActionResult ContactedDetails(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ContactU contactU = db.ContactUs.Find(id);

            if (contactU == null)
            {
                return HttpNotFound();
            }

            return View(contactU);
        }
        public ActionResult DeleteContacted(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ContactU contactU = db.ContactUs.Find(id);

            if (contactU == null)
            {
                return HttpNotFound();
            }

            db.ContactUs.Remove(contactU);
            db.SaveChanges();

            return RedirectToAction("GetContacted");
        }

        public ActionResult ReplyContact(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ContactU contactU = db.ContactUs.Find(id);

            if (contactU == null)
            {
                return HttpNotFound();
            }

            ViewBag.Email = contactU.Email;

            return View();
        }
        [HttpPost]
        public ActionResult SendReply(string email, string subject, string message)
        {
            try
            {

                var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential("dimaahamd1998@gmail.com", "ktqhdrixunkpmlgi"),
                    EnableSsl = true,
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress("dimaahamd1998@gmail.com", "SitWize"),
                    Subject = subject,
                    Body = message,
                    IsBodyHtml = false,
                };


                mailMessage.To.Add(email);


                smtpClient.Send(mailMessage);

                return Json(new { success = true, message = "Email sent successfully!" });
            }
            catch (Exception ex)
            {
                // Return error JSON response
                return Json(new { success = false, message = "Error sending email: " + ex.Message });
            }
        }




        //public ActionResult GetAllSitters()
        //{
        //    var sitters = db.Sitters.ToList();
        //    return View(sitters);


        //}
        public ActionResult GetAllSitters(string searchTerm = null)
        {
            var sitters = db.Sitters.AsQueryable();

            // Check if the search term is not null or empty
            if (!string.IsNullOrEmpty(searchTerm))
            {
                sitters = sitters.Where(s => s.FirstName.Contains(searchTerm)
                                          || s.LastName.Contains(searchTerm)
                                          || s.Email.Contains(searchTerm)
                                          || s.PhoneNumber.Contains(searchTerm));
            }

            return View(sitters.ToList());
        }

        //public ActionResult DeleteSitter(int id)
        //{
        //    var sitter = db.Sitters.Find(id);
        //    if (sitter == null)
        //    {
        //        return HttpNotFound();
        //    }

        //    db.Sitters.Remove(sitter);
        //    db.SaveChanges();

        //    return RedirectToAction("GetAllSitters");
        //}


        //آخر تعديل قبل فوق 


        public ActionResult DeleteSitter(int id)
        {
            var sitter = db.Sitters.Find(id);
            if (sitter == null)
            {
                return HttpNotFound();
            }

            // Check if the sitter has any active bookings
            var hasBookings = db.Bookings.Any(b => b.SitterID == id);
            if (hasBookings)
            {
                // Set an error message in TempData to show in the view
                TempData["ErrorMessage"] = "This sitter cannot be deleted because they have active bookings.";
                return RedirectToAction("GetAllSitters");
            }

            // If no active bookings, proceed with deletion
            db.Sitters.Remove(sitter);
            db.SaveChanges();

            return RedirectToAction("GetAllSitters");
        }

        public ActionResult DownloadSitterLicense(int id)
        {
            var sitter = db.Sitters.Find(id);  
            if (sitter == null || string.IsNullOrEmpty(sitter.LicensePath))
            {
                TempData["ErrorMessage"] = "License file not found.";
                return RedirectToAction("GetAllSitters","Admin");  
            }

            var filePath = Server.MapPath(sitter.LicensePath);
            if (!System.IO.File.Exists(filePath))
            {
                TempData["ErrorMessage"] = "License file does not exist.";
                return RedirectToAction("GetAllSitters","Admin");  
            }

            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            var fileName = Path.GetFileName(filePath);

            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }




        public ActionResult SitterDetails(int id)
        {
            var sitter = db.Sitters.Find(id);

            if (sitter == null)
            {
                return HttpNotFound();
            }

            return View(sitter);
        }





        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult SitterEdit(Sitter sitter)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        db.Entry(sitter).State = EntityState.Modified;
        //        db.SaveChanges();
        //        return RedirectToAction("Details", new { id = sitter.SitterID });
        //    }

        //    return View(sitter);
        //}

        //@@@@@@@@@@@@@@@@@@before last edit @@@@@@@@@@@@@@@@@@@@

        //public ActionResult SitterEdit(int id)
        //{
        //    var sitter = db.Sitters.Find(id);

        //    if (sitter == null)
        //    {
        //        return HttpNotFound();
        //    }

        //    return View(sitter);
        //}
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult SitterEdit(Sitter sitter)
        //{
        //    try
        //    {
        //        if (ModelState.IsValid)
        //        {
        //            var existingSitter = db.Sitters.Find(sitter.SitterID);

        //            if (existingSitter == null)
        //            {
        //                return HttpNotFound();
        //            }

        //            // Update properties (excluding concurrency control)
        //            existingSitter.FirstName = sitter.FirstName;
        //            existingSitter.LastName = sitter.LastName;
        //            existingSitter.Email = sitter.Email;
        //            existingSitter.PhoneNumber = sitter.PhoneNumber;
        //            existingSitter.Bio = sitter.Bio;
        //            existingSitter.ExperienceYears = sitter.ExperienceYears;
        //            existingSitter.sitterImage = sitter.sitterImage;
        //            existingSitter.IsAvailable = sitter.IsAvailable;
        //            existingSitter.IsApproved = sitter.IsApproved;
        //            existingSitter.CreatedAt = sitter.CreatedAt;
        //            existingSitter.HourlyRate = sitter.HourlyRate;
        //            existingSitter.LicensePath = sitter.LicensePath;

        //            db.Entry(existingSitter).State = EntityState.Modified;
        //            db.SaveChanges();  // This will throw an exception if the record was modified
        //            return RedirectToAction("SitterDetails", new { id = sitter.SitterID });
        //        }
        //    }
        //    catch (DbUpdateConcurrencyException ex)
        //    {
        //        // Handle concurrency issue
        //        var entry = ex.Entries.Single();
        //        var databaseValues = (Sitter)entry.GetDatabaseValues().ToObject();

        //        ModelState.AddModelError(string.Empty, "The record you attempted to edit was modified by another user after you got the original value. The edit operation was canceled, and the current values in the database have been displayed. If you still want to edit this record, click Save again.");

        //        // Set current database values to be displayed in the form
        //        sitter.FirstName = databaseValues.FirstName;
        //        sitter.LastName = databaseValues.LastName;
        //        sitter.Email = databaseValues.Email;
        //        sitter.PhoneNumber = databaseValues.PhoneNumber;
        //        sitter.Bio = databaseValues.Bio;
        //        sitter.ExperienceYears = databaseValues.ExperienceYears;
        //        sitter.sitterImage = databaseValues.sitterImage;
        //        sitter.IsAvailable = databaseValues.IsAvailable;
        //        sitter.IsApproved = databaseValues.IsApproved;
        //        sitter.CreatedAt = databaseValues.CreatedAt;
        //        sitter.HourlyRate = databaseValues.HourlyRate;
        //        sitter.LicensePath = databaseValues.LicensePath;

        //        // You can also manually check for other properties if needed
        //    }

        //    return View(sitter);
        //}



        //public ActionResult SitterEdit(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    Sitter sitter = db.Sitters.Find(id);
        //    if (sitter == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(sitter);
        //}

        //// POST: Admin/SitterEdit/5
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult SitterEdit([Bind(Include = "SitterID,FirstName,LastName,Email,PhoneNumber,Bio,ExperienceYears,IsAvailable,IsApproved,HourlyRate,LicensePath")] Sitter sitter)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        db.Entry(sitter).State = EntityState.Modified;
        //        db.SaveChanges();
        //        return RedirectToAction("GetAllSitters");
        //    }
        //    return View(sitter);
        //}

        public ActionResult SitterEdit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Sitter sitter = db.Sitters.Find(id);
            if (sitter == null)
            {
                return HttpNotFound();
            }
            return View(sitter);
        }

        // POST: Admin/SitterEdit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SitterEdit([Bind(Include = "SitterID,FirstName,LastName,Email,PhoneNumber,Bio,ExperienceYears,IsAvailable,IsApproved,HourlyRate,LicensePath")] Sitter sitter)
        {
            if (ModelState.IsValid)
            {
                if (!sitter.ExperienceYears.HasValue)
                {
                    ModelState.AddModelError("ExperienceYears", "Experience years is required.");
                    TempData["swalMessage"] = "error|Experience years is required.";
                    return View(sitter);
                }

                if (!sitter.HourlyRate.HasValue)
                {
                    ModelState.AddModelError("HourlyRate", "Hourly rate is required.");
                    TempData["swalMessage"] = "error|Hourly rate is required.";
                    return View(sitter);
                }

                if (!IsHourlyRateValid(sitter.ExperienceYears.Value, sitter.HourlyRate.Value, out string errorMessage))
                {
                    ModelState.AddModelError("HourlyRate", errorMessage);
                    TempData["swalMessage"] = $"error|{errorMessage}";
                    return View(sitter);
                }

                try
                {
                    db.Entry(sitter).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["swalMessage"] = $"success|Sitter {sitter.FirstName} {sitter.LastName} has been updated successfully!";
                    return RedirectToAction("GetAllSitters");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"An error occurred while saving changes: {ex.Message}");
                    TempData["swalMessage"] = $"error|An error occurred while saving changes: {ex.Message}";
                    return View(sitter);
                }
            }

            // In case of validation errors
            TempData["swalMessage"] = "error|Please correct the validation errors.";
            return View(sitter);
        }

        private bool IsHourlyRateValid(int experienceYears, decimal hourlyRate, out string errorMessage)
        {
            decimal minRate, maxRate;
            if (experienceYears >= 0 && experienceYears <= 2)
            {
                minRate = 5; maxRate = 7;
            }
            else if (experienceYears >= 3 && experienceYears <= 5)
            {
                minRate = 7; maxRate = 10;
            }
            else if (experienceYears > 5 && experienceYears <= 10)
            {
                minRate = 10; maxRate = 15;
            }
            else if (experienceYears > 10)
            {
                minRate = 15; maxRate = 20;
            }
            else
            {
                errorMessage = "Invalid experience years.";
                return false;
            }

            if (hourlyRate < minRate || hourlyRate > maxRate)
            {
                errorMessage = $"Hourly rate should be between {minRate:C} and {maxRate:C} based on your experience.";
                return false;
            }

            errorMessage = string.Empty;
            return true;
        }





        //    [HttpPost]
        //    [ValidateAntiForgeryToken]
        //    public ActionResult AddSitter(AddSitterViewModel model)
        //    {
        //        if (ModelState.IsValid)
        //        {
        //            if (db.Sitters.Any(s => s.Email == model.Email))
        //            {
        //                ModelState.AddModelError("Email", "This email is already registered.");
        //                return View(model);
        //            }

        //            // Hourly rate validation
        //            decimal minRate, maxRate;
        //            if (model.ExperienceYears >= 0 && model.ExperienceYears <= 2)
        //            {
        //                minRate = 5;
        //                maxRate = 7;
        //            }
        //            else if (model.ExperienceYears >= 3 && model.ExperienceYears <= 5)
        //            {
        //                minRate = 7;
        //                maxRate = 10;
        //            }
        //            else if (model.ExperienceYears > 5 && model.ExperienceYears <= 10)
        //            {
        //                minRate = 10;
        //                maxRate = 15;
        //            }
        //            else if (model.ExperienceYears > 10)
        //            {
        //                minRate = 15;
        //                maxRate = 20;
        //            }
        //            else
        //            {
        //                ModelState.AddModelError("ExperienceYears", "Invalid experience years.");
        //                return View(model);
        //            }

        //            if (model.HourlyRate < minRate || model.HourlyRate > maxRate)
        //            {
        //                ModelState.AddModelError("HourlyRate", $"Hourly rate should be between {minRate:C} and {maxRate:C} based on your experience.");
        //                return View(model);
        //            }

        //            if (model.LicenseUpload != null && model.LicenseUpload.ContentLength > 0)
        //            {
        //                var allowedExtensions = new[] { ".pdf", ".doc", ".docx" };
        //                var fileExtension = Path.GetExtension(model.LicenseUpload.FileName).ToLower();
        //                if (!allowedExtensions.Contains(fileExtension))
        //                {
        //                    ModelState.AddModelError("LicenseUpload", "Only PDF, DOC, and DOCX files are allowed.");
        //                    return View(model);
        //                }

        //                var fileName = Path.GetFileName(model.LicenseUpload.FileName);
        //                var path = Path.Combine(Server.MapPath("~/Uploads/Licenses"), fileName);
        //                model.LicenseUpload.SaveAs(path);

        //                model.LicensePath = "/Uploads/Licenses/" + fileName;
        //            }

        //            string password = GenerateRandomPassword();
        //            string hashedPassword = HashPassword(password);

        //            var sitter = new Sitter
        //            {
        //                FirstName = model.FirstName,
        //                LastName = model.LastName,
        //                Email = model.Email,
        //                PhoneNumber = model.PhoneNumber,
        //                Bio = model.Bio,
        //                ExperienceYears = model.ExperienceYears,
        //                HourlyRate = model.HourlyRate,
        //                LicensePath = model.LicensePath,
        //                PasswordHash = hashedPassword,
        //                CreatedAt = DateTime.Now,
        //                IsApproved = true,
        //                IsAvailable = true
        //            };

        //            db.Sitters.Add(sitter);
        //            db.SaveChanges();

        //            SendCredentialsEmail(sitter.Email, sitter.FirstName, password);

        //            return RedirectToAction("GetAllSitters");
        //        }

        //        return View(model);

        //}

        [HttpGet]
        public ActionResult AddSitter()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddSitter(AddSitterViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (db.Sitters.Any(s => s.Email == model.Email))
                {
                    TempData["ErrorMessage"] = "This email is already registered.";
                    return View(model);
                }


                decimal minRate, maxRate;
                if (model.ExperienceYears >= 0 && model.ExperienceYears <= 2)
                {
                    minRate = 5;
                    maxRate = 7;
                }
                else if (model.ExperienceYears >= 3 && model.ExperienceYears <= 5)
                {
                    minRate = 7;
                    maxRate = 10;
                }
                else if (model.ExperienceYears > 5 && model.ExperienceYears <= 10)
                {
                    minRate = 10;
                    maxRate = 15;
                }
                else if (model.ExperienceYears > 10)
                {
                    minRate = 15;
                    maxRate = 20;
                }
                else
                {
                    TempData["ErrorMessage"] = "Invalid experience years.";
                    return View(model);
                }

                if (model.HourlyRate < minRate || model.HourlyRate > maxRate)
                {
                    TempData["ErrorMessage"] = $"Hourly rate should be between {minRate:C} and {maxRate:C} based on your experience.";
                    return View(model);
                }
                if (model.LicenseUpload != null && model.LicenseUpload.ContentLength > 0)
                {
                    var allowedExtensions = new[] { ".pdf", ".doc", ".docx" };
                    var fileExtension = Path.GetExtension(model.LicenseUpload.FileName).ToLower();
                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        ModelState.AddModelError("LicenseUpload", "Only PDF, DOC, and DOCX files are allowed.");
                        return View(model);
                    }

                    var fileName = Path.GetFileName(model.LicenseUpload.FileName);
                    var path = Path.Combine(Server.MapPath("~/Uploads/Licenses"), fileName);
                    model.LicenseUpload.SaveAs(path);

                    model.LicensePath = "/Uploads/Licenses/" + fileName;
                }

                string password = GenerateRandomPassword();
                string hashedPassword = HashPassword(password);



                var sitter = new Sitter
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber,
                    Bio = model.Bio,
                    ExperienceYears = model.ExperienceYears,
                    HourlyRate = model.HourlyRate,
                    LicensePath = model.LicensePath,
                    PasswordHash = hashedPassword,
                    CreatedAt = DateTime.Now,
                    IsApproved = true,
                    IsAvailable = true
                };

                db.Sitters.Add(sitter);
                db.SaveChanges();

                string errorMessage;
                bool isEmailSent = SendCredentialsEmail(sitter.Email, sitter.FirstName, password, out errorMessage);

                if (!isEmailSent)
                {
                    ViewBag.ErrorMessage = errorMessage;
                }
                else
                {

                    TempData["SuccessMessage"] = "Sitter has been successfully added and credentials were sent.";
                }

                return RedirectToAction("AddSitter");
            }


            return View(model);
        }


        private bool SendCredentialsEmail(string email, string firstName, string password, out string errorMessage)
        {
            errorMessage = string.Empty;

            try
            {
                var fromAddress = new MailAddress("dimaahamd1998@gmail.com", "SitWize");
                var toAddress = new MailAddress(email, firstName);
                const string subject = "Your Sitter Account Credentials";
                string body = $"Dear {firstName},\n\nYour account has been created. Here are your login credentials:\n\nEmail: {email}\nPassword: {password}\n\nPlease change your password after your first login.\n\nBest regards,\nThe Admin Team";

                using (var smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new System.Net.NetworkCredential("dimaahamd1998@gmail.com", "ktqhdrixunkpmlgi")
                })
                {
                    using (var message = new MailMessage(fromAddress, toAddress)
                    {
                        Subject = subject,
                        Body = body
                    })
                    {
                        smtp.Send(message);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = "There was an error sending the email: " + ex.Message;
                return false;
            }
        }

















        public ActionResult JoinUsApplications()
        {
            var applications = db.JoinUs.Where(j => j.ApplicationStatus == "Pending").ToList();
            return View(applications);
        }

        public ActionResult ReviewApplication(int id)
        {
            var application = db.JoinUs.Find(id);
            if (application == null)
            {
                return HttpNotFound();
            }
            return View(application);
        }

        public ActionResult AcceptApplication(int id)
        {
            var application = db.JoinUs.Find(id);
            if (application == null)
            {
                return HttpNotFound();
            }


            application.ApplicationStatus = "Accepted";


            string password = GenerateRandomPassword();
            var hashedPassword = HashPassword(password);


            var sitter = new Sitter
            {
                FirstName = application.FirstName,
                LastName = application.LastName,
                Email = application.Email,
                PhoneNumber = application.PhoneNumber,
                ExperienceYears = application.ExperienceYears.HasValue ? application.ExperienceYears.Value : (int?)null,
                Bio = application.CoverLetter,
                CreatedAt = DateTime.Now,
                IsApproved = true,
                IsAvailable = true,
                PasswordHash = hashedPassword,
                sitterImage = "~/img/istockphoto-1300845620-612x612.jpg",
                HourlyRate = application.HourlyRate,
                LicensePath = application.LicensePath
            };

            try
            {

                db.Sitters.Add(sitter);
                db.SaveChanges();


                SendEmail(sitter.Email, "Congratulations! Your application has been approved",
                    $"Dear {sitter.FirstName},\n\nYour application has been approved. You can now log in using the following credentials:\n\nEmail: {sitter.Email}\nPassword: {password}\n\nThank you!");

            }
            catch (DbEntityValidationException ex)
            {

                foreach (var validationErrors in ex.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        System.Diagnostics.Debug.WriteLine("Property: " + validationError.PropertyName + " Error: " + validationError.ErrorMessage);
                    }
                }

                throw;
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RejectApplication(int id)
        {
            var application = db.JoinUs.Find(id);
            if (application == null)
            {
                return HttpNotFound();
            }


            application.ApplicationStatus = "Rejected";
            db.SaveChanges();

            SendEmail(application.Email, "Application Rejected",
                $"Dear {application.FirstName},\n\nWe regret to inform you that your application has been rejected. Thank you for your interest in joining us.\n\nBest regards,\nThe Team");

            return RedirectToAction("Index", "Home");
        }



        private void SendEmail(string toEmail, string subject, string body)
        {
            try
            {
                var fromAddress = new MailAddress("dimaahamd1998@gmail.com", "SitWize");
                var toAddress = new MailAddress(toEmail);
                const string fromPassword = "ktqhdrixunkpmlgi";

                var smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
                };

                using (var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject,
                    Body = body
                })
                {
                    smtp.Send(message);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error sending email: " + ex.Message);
            }
        }




        private string GenerateRandomPassword()
        {
            return Guid.NewGuid().ToString().Substring(0, 8);
        }


        private string HashPassword(string password)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }








        public ActionResult DownloadLicense(int id)
        {

            var application = db.JoinUs.Find(id);
            if (application == null || string.IsNullOrEmpty(application.LicensePath))
            {
                TempData["ErrorMessage"] = "License file not found.";
                return RedirectToAction("ReviewApplication", new { id });
            }

            var filePath = Server.MapPath(application.LicensePath);
            if (!System.IO.File.Exists(filePath))
            {
                TempData["ErrorMessage"] = "License file does not exist.";
                return RedirectToAction("ReviewApplication", new { id });
            }

            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            var fileName = Path.GetFileName(filePath);

            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }




        public ActionResult PendingPayment()
        {
            var pendingPayments = db.Payments
                .Where(p => p.Status == "Pending")
                .Include(p => p.Booking)
                .ToList();

            return View(pendingPayments);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdatePaymentStatus(int PaymentID, string Status)
        {
            var payment = db.Payments.FirstOrDefault(p => p.PaymentID == PaymentID);

            if (payment == null)
            {
                return HttpNotFound();
            }

            payment.Status = Status;
            db.SaveChanges();

            TempData["SuccessMessage"] = $"Payment {PaymentID} status updated to {Status}.";

            return RedirectToAction("PendingPayment");
        }






        [HttpGet]
        public ActionResult GetAllPayments()
        {
            var payments = db.Payments.ToList();
            return View(payments);
        }

        public ActionResult PaymentDetails(int id)
        {
            var payment = db.Payments.Include(p => p.Booking.User)
                                     .Include(p => p.Booking.Sitter)
                                     .Include(p => p.Booking.Service)
                                     .FirstOrDefault(p => p.PaymentID == id);

            if (payment == null)
            {
                return HttpNotFound();
            }

            return View(payment);
        }


        public ActionResult DeletePayments(int id)
        {
            var payment = db.Payments.Find(id);

            if (payment == null)
            {
                return HttpNotFound();
            }

            db.Payments.Remove(payment);
            db.SaveChanges();

            return RedirectToAction("GetAllPayments");
        }


        //public ActionResult EditPayments(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    Payment payment = db.Payments.Find(id);
        //    if (payment == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(payment);
        //}

        //// POST: Admin/EditPaymentStatus/5
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult EditPayments(int id, string status)
        //{
        //    var payment = db.Payments.Find(id);
        //    if (payment == null)
        //    {
        //        return HttpNotFound();
        //    }

        //    if (ModelState.IsValid)
        //    {
        //        payment.Status = status;
        //        try
        //        {
        //            db.SaveChanges();
        //            return RedirectToAction("GetAllPayments");
        //        }
        //        catch (DbEntityValidationException ex)
        //        {
        //            foreach (var entityValidationErrors in ex.EntityValidationErrors)
        //            {
        //                foreach (var validationError in entityValidationErrors.ValidationErrors)
        //                {
        //                    ModelState.AddModelError("", validationError.ErrorMessage);
        //                }
        //            }
        //        }
        //    }

        //    return View(payment);
        //}

        public ActionResult EditPayments(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Payment payment = db.Payments.Find(id);
            if (payment == null)
            {
                return HttpNotFound();
            }
            return View(payment);
        }

        // POST: Admin/EditPaymentStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditPayments([Bind(Include = "PaymentID,Status")] Payment paymentUpdate)
        {
            if (ModelState.IsValid)
            {
                var payment = db.Payments.Find(paymentUpdate.PaymentID);
                if (payment == null)
                {
                    return HttpNotFound();
                }

                payment.Status = paymentUpdate.Status;
                db.Entry(payment).State = EntityState.Modified;

                try
                {
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Payment status updated successfully.";
                    return RedirectToAction("GetAllPayments", new { id = payment.PaymentID });
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
                }
            }


            return View(db.Payments.Find(paymentUpdate.PaymentID));
        }



        //public ActionResult GetBookings()
        //{


        //    var bookings = db.Bookings.Include("User").Include("Sitter").Include("Service").ToList();
        //    return View(bookings);

        //}
        public ActionResult GetBookings()
        {
            var bookings = db.Bookings.Include(b => b.User).Include(b => b.Sitter).Include(b => b.Service).Include(b => b.ServiceDetail).ToList();
            return View(bookings);
        }

        public ActionResult DeleteBookings(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Booking booking = db.Bookings.Find(id);
            if (booking == null)
                return HttpNotFound();

            return View(booking);
        }

        // POST: Admin/DeleteBookings/5
        [HttpPost, ActionName("DeleteBookings")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteBookingsConfirmed(int id)
        {
            try
            {
                Booking booking = db.Bookings.Find(id);
                if (booking != null)
                {
                    db.Bookings.Remove(booking);
                    db.SaveChanges();
                    return Json(new { success = true, message = "Booking deleted successfully." });
                }
                return Json(new { success = false, message = "Booking not found." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred: " + ex.Message });
            }
        }





        public ActionResult EditBooking(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Booking booking = db.Bookings
                .Include(b => b.User)
                .Include(b => b.Sitter)
                .Include(b => b.Service)
                .Include(b => b.ServiceDetail)
                .FirstOrDefault(b => b.BookingID == id);
            if (booking == null)
            {
                return HttpNotFound();
            }
            ViewBag.Statuses = new SelectList(new[] { "Pending", "Confirmed", "Cancelled", "Completed" }, booking.Status);
            return View(booking);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditBooking([Bind(Include = "BookingID,Status")] Booking bookingUpdate)
        {
            if (ModelState.IsValid)
            {
                var bookingToUpdate = db.Bookings.Find(bookingUpdate.BookingID);
                if (bookingToUpdate != null)
                {
                    bookingToUpdate.Status = bookingUpdate.Status;
                    db.SaveChanges();
                    return RedirectToAction("GetBookings", "Admin");
                }
            }

            var booking = db.Bookings
                .Include(b => b.User)
                .Include(b => b.Sitter)
                .Include(b => b.Service)
                .Include(b => b.ServiceDetail)
                .FirstOrDefault(b => b.BookingID == bookingUpdate.BookingID);
            ViewBag.Statuses = new SelectList(new[] { "Pending", "Confirmed", "Cancelled", "Completed" }, bookingUpdate.Status);
            return View(booking);
        }



        public ActionResult BookingDetails(int id)
        {
            var booking = db.Bookings.Include(b => b.User)
                                     .Include(b => b.Sitter)
                                     .Include(b => b.Service)
                                     .Include(b => b.ServiceDetail)
                                     .FirstOrDefault(b => b.BookingID == id);

            if (booking == null)
            {
                return HttpNotFound();
            }

            return View(booking);
        }












        public ActionResult CreateBookings()

        {
            var booking = new Booking
            {
                Status = "Pending"
            };

            ViewBag.ServiceID = new SelectList(db.Services, "ServiceID", "ServiceName");
            ViewBag.SitterID = new SelectList(db.Sitters, "SitterID", "FirstName");
            ViewBag.UserID = new SelectList(db.Users, "UserID", "FirstName");
            return View();
        }


        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult CreateBookings([Bind(Include = "BookingID,UserID,SitterID,ServiceID,DetailID,BookingDate,StartDate,StartTime,EndTime,Duration,Status,CreatedAt,TotalAmount")] Booking booking)
        //{


        //    if (ModelState.IsValid)
        //    {
        //        string startTimeStr = Request.Form["StartTime"];
        //        string endTimeStr = Request.Form["EndTime"];
        //        string durationStr = Request.Form["Duration"];


        //        TimeSpan startTimeParsed, endTimeParsed;
        //        bool isStartTimeValid = TimeSpan.TryParse(startTimeStr, out startTimeParsed);
        //        bool isEndTimeValid = TimeSpan.TryParse(endTimeStr, out endTimeParsed);

        //        if (!isStartTimeValid || !isEndTimeValid)
        //        {
        //            ModelState.AddModelError("", "Invalid time format. Please ensure Start Time and End Time are valid.");
        //            return View(booking);
        //        }
        //        DateTime? startDate = null;
        //        if (DateTime.TryParse(Request.Form["StartDate"], out DateTime parsedStartDate))
        //        {
        //            startDate = parsedStartDate;
        //        }
        //        else
        //        {
        //            ModelState.AddModelError("", "Invalid Start Date format.");
        //            return View(booking);
        //        }




        //        var serviceDetail = new ServiceDetail
        //        {
        //            ServiceID = booking.ServiceID,
        //            DetailType = "Regular Sitter", 
        //            StartTime = startTimeParsed,        
        //            EndTime = endTimeParsed,            
        //            Duration = durationStr,             
        //            StartDate = startDate,             

        //        };

        //        db.ServiceDetails.Add(serviceDetail);
        //        db.SaveChanges();

        //        booking.DetailID = serviceDetail.DetailID;
        //        booking.Status = "Pending";
        //        booking.CreatedAt = DateTime.Now;






        //        db.Bookings.Add(booking);
        //        db.SaveChanges();

        //        return RedirectToAction("GetBookings","Admin");
        //    }


        //    ViewBag.ServiceID = new SelectList(db.Services, "ServiceID", "ServiceName", booking.ServiceID);
        //    ViewBag.SitterID = new SelectList(db.Sitters, "SitterID", "FirstName", booking.SitterID);
        //    ViewBag.UserID = new SelectList(db.Users, "UserID", "FirstName", booking.UserID);

        //    return View(booking);

        //}



        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateBookings([Bind(Include = "BookingID,UserID,SitterID,ServiceID,DetailID,BookingDate,StartDate,StartTime,EndTime,Duration,Status")] Booking booking)
        {
            // Note: Removed TotalAmount from the Bind attribute since we'll calculate it server-side

            if (ModelState.IsValid)
            {
                string startTimeStr = Request.Form["StartTime"];
                string endTimeStr = Request.Form["EndTime"];
                string durationStr = Request.Form["Duration"];

                TimeSpan startTimeParsed, endTimeParsed;
                bool isStartTimeValid = TimeSpan.TryParse(startTimeStr, out startTimeParsed);
                bool isEndTimeValid = TimeSpan.TryParse(endTimeStr, out endTimeParsed);

                if (!isStartTimeValid || !isEndTimeValid)
                {
                    ModelState.AddModelError("", "Invalid time format. Please ensure Start Time and End Time are valid.");
                    return View(booking);
                }

                DateTime? startDate = null;
                if (DateTime.TryParse(Request.Form["StartDate"], out DateTime parsedStartDate))
                {
                    startDate = parsedStartDate;
                }
                else
                {
                    ModelState.AddModelError("", "Invalid Start Date format.");
                    return View(booking);
                }

                // Get sitter's hourly rate
                var sitter = db.Sitters.FirstOrDefault(s => s.SitterID == booking.SitterID);
                if (sitter == null || !sitter.HourlyRate.HasValue)
                {
                    ModelState.AddModelError("", "Selected sitter's hourly rate is not set.");
                    return View(booking);
                }

                // Calculate duration
                TimeSpan duration = endTimeParsed - startTimeParsed;
                if (duration.TotalHours < 0)
                {
                    duration = duration.Add(TimeSpan.FromHours(24));
                }

                // Calculate total amount
                decimal totalHours = (decimal)duration.TotalHours;
                decimal totalAmount = sitter.HourlyRate.Value * totalHours;

                // Create ServiceDetail
                var serviceDetail = new ServiceDetail
                {
                    ServiceID = booking.ServiceID,
                    DetailType = "Regular Sitter",
                    StartTime = startTimeParsed,
                    EndTime = endTimeParsed,
                    Duration = durationStr,
                    StartDate = startDate
                };

                db.ServiceDetails.Add(serviceDetail);
                db.SaveChanges();

                // Set up the booking
                booking.DetailID = serviceDetail.DetailID;
                booking.Status = "Pending";
                booking.CreatedAt = DateTime.Now;
                booking.TotalAmount = totalAmount; // Set the calculated total amount

                db.Bookings.Add(booking);
                db.SaveChanges();

                return RedirectToAction("GetBookings", "Admin");
            }

            // If we get here, something failed, redisplay form
            ViewBag.ServiceID = new SelectList(db.Services, "ServiceID", "ServiceName", booking.ServiceID);
            ViewBag.SitterID = new SelectList(db.Sitters, "SitterID", "FirstName", booking.SitterID);
            ViewBag.UserID = new SelectList(db.Users, "UserID", "FirstName", booking.UserID);

            return View(booking);
        }

        [HttpGet]
        public JsonResult GetSitterRate(int sitterId)
        {
            var sitter = db.Sitters.FirstOrDefault(s => s.SitterID == sitterId);
            if (sitter != null && sitter.HourlyRate.HasValue)
            {
                return Json(new { hourlyRate = sitter.HourlyRate.Value }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { hourlyRate = 0 }, JsonRequestBehavior.AllowGet);
        }


        public ActionResult GetTest()
        {

            var Test = db.Testimonials.ToList();
            return View(Test);
        }

        [HttpPost]
        public JsonResult UpdateStatus(int id, string status)
        {
            var testimonial = db.Testimonials.Find(id);
            if (testimonial != null)
            {
                testimonial.statues = status;
                db.SaveChanges(); 
                return Json(new { success = true, message = "Status updated successfully!" });
            }
            return Json(new { success = false, message = "Testimonial not found." });
        }

        [HttpPost]
        public JsonResult DeleteTest(int id)
        {
            
            var testimonial = db.Testimonials.Find(id);
            if (testimonial != null)
            {
                db.Testimonials.Remove(testimonial); 

                try
                {
                    db.SaveChanges();
                    return Json(new { success = true, message = "Testimonial deleted successfully!" });
                }
                catch (Exception ex)
                {
                    
                    Console.WriteLine(ex.Message);
                    return Json(new { success = false, message = "An error occurred while deleting the testimonial." });
                }
            }
            return Json(new { success = false, message = "Testimonial not found." });
        }
























    }



}







