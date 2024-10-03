using MasterPiece.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;

namespace MasterPiece.Controllers
{
    public class HomeController : Controller
    {

        private MasterPeiceEntities db = new MasterPeiceEntities();

        public ActionResult Index()
        {
            {
                bool testimonialExists = false;

                if (Session["UserID"] != null)
                {
                    var userId = (int)Session["UserID"];

                    // Check if the user has submitted a testimonial
                    testimonialExists = db.Testimonials.Any(t => t.UserID == userId && t.TestimonialSubmitted == true);

                    // Show the toast if the user has not submitted a testimonial and has past bookings
                    var completedBookings = db.Bookings
                                           .Where(b => b.UserID == userId && b.Status == "Completed").Any();
                                                       

                   
                    if (completedBookings && !testimonialExists)
                    {
                        ViewBag.ShowToastNotification = true; 
                    }
                }

                ViewBag.TestimonialExists = testimonialExists;
                var approvedTestimonials = db.Testimonials
                                     .Where(t => t.statues == "Approved")
                                     .Take(4) 
                                     .ToList();
                ViewBag.ApprovedTestimonials = approvedTestimonials;





                return View();
            }
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";
            var topSitters = db.Sitters
       .OrderByDescending(s => s.Bookings.Count()) // Order by booking count
       .Take(3) // Take the top 3 sitters
       .ToList(); // Return the sitters directly without a ViewModel

            return View(topSitters);

           
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitContactForm(ContactModel model)
        {
            if (ModelState.IsValid)
            {
               
                var contact = new ContactU
                {
                    Name = model.Name,
                    Email = model.Email,
                    Subject = model.Subject,
                    Message = model.Message,
                    SubmittedAt = DateTime.Now
                };

                db.ContactUs.Add(contact);
                db.SaveChanges();

            
                try
                {
                    SendMessage(model);
                    TempData["SuccessMessage"] = "Your message has been sent successfully!";
                }
                catch (Exception ex)
                {
           
                    TempData["ErrorMessage"] = "There was an error sending your message. Please try again later.";
             
                }

                return RedirectToAction("Contact");
            }

           
            return View("Contact", model);
        }

        private void SendMessage(ContactModel model)
        {
            try
            {
                string toEmail = ConfigurationManager.AppSettings["ToEmail"];
                string fromEmail = ConfigurationManager.AppSettings["FromEmail"];
                string smtpUsername = ConfigurationManager.AppSettings["SmtpUsername"];
                string smtpPassword = ConfigurationManager.AppSettings["SmtpPassword"];
                string smtpServer = ConfigurationManager.AppSettings["SmtpServer"];
                int smtpPort = int.Parse(ConfigurationManager.AppSettings["SmtpPort"]);

                using (MailMessage mailMessage = new MailMessage())
                {
                    string fromName = "SitWize"; 
                    mailMessage.From = new MailAddress(fromEmail, fromName);
                    mailMessage.ReplyToList.Add(new MailAddress(model.Email, model.Name)); 
                    mailMessage.To.Add(toEmail);
                    mailMessage.Subject = $"New Contact Message: {model.Subject}";
                    mailMessage.Body = $"Name: {model.Name}\nEmail: {model.Email}\nSubject: {model.Subject}\n\nMessage:\n{model.Message}";
                    mailMessage.IsBodyHtml = false;

                    using (SmtpClient smtpClient = new SmtpClient(smtpServer, smtpPort))
                    {
                        smtpClient.UseDefaultCredentials = false;
                        smtpClient.Credentials = new System.Net.NetworkCredential(smtpUsername, smtpPassword);
                        smtpClient.EnableSsl = true;

                        smtpClient.Send(mailMessage);
                    }
                }
            }
            catch (Exception ex)
            {
               
                throw; 
            }
        }




        //[HttpGet]
        //public ActionResult JoinUs()
        //{
        //    return View();
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult JoinUs(JoinU model) // Use JoinU directly
        //{


        //    var existingApplication = db.JoinUs.FirstOrDefault(j => j.Email == model.Email);

        //    if (existingApplication != null)
        //    {
        //        // If email exists, add an error message and return the form view
        //        TempData["ErrorMessage"] = "The email address is already registered. Please enter a different email.";
        //        return View(model);
        //    }



        //    if (ModelState.IsValid)
        //    {
        //        model.ApplicationStatus = "Pending";
        //        model.SubmittedAt = DateTime.Now;

        //        db.JoinUs.Add(model); // Assuming db.JoinUs is DbSet<JoinU>
        //        db.SaveChanges();

        //        TempData["SuccessMessage"] = "Your application has been submitted successfully!";
        //        return RedirectToAction("JoinUs");
        //    }
        //    TempData["ErrorMessage"] = "There were errors in your form submission. Please correct them and try again.";
        //    // If we got this far, something failed; redisplay the form
        //    return View(model);
        //}

        [HttpGet]
        public ActionResult JoinUs()
        {
            var model = new JoinU
            {
                ExperienceTypes = "" 
            };
            return View(model);
        }




        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult JoinUs(JoinU model, string[] ExperienceTypes)
        //{

        //    var existingApplication = db.JoinUs.FirstOrDefault(j => j.Email == model.Email);

        //    if (existingApplication != null)
        //    {
        //        TempData["ErrorMessage"] = "The email address is already registered. Please enter a different email.";
        //        return View(model);
        //    }

        //    if (ModelState.IsValid)
        //    {
        //        decimal minRate = 5;
        //        decimal maxRate = 7;

        //        if (model.ExperienceYears >= 0 && model.ExperienceYears <= 2)
        //        {
        //            minRate = 5;
        //            maxRate = 7;
        //        }
        //        else if (model.ExperienceYears >= 3 && model.ExperienceYears <= 5)
        //        {
        //            minRate = 7;
        //            maxRate = 10;
        //        }
        //        else if (model.ExperienceYears >= 5 && model.ExperienceYears <= 10)
        //        {
        //            minRate = 10;
        //            maxRate = 15;
        //        }
        //        else if (model.ExperienceYears > 10)
        //        {
        //            minRate = 15;
        //            maxRate = 20;
        //        }

        //        if (model.HourlyRate < minRate || model.HourlyRate > maxRate)
        //        {
        //            ModelState.AddModelError("HourlyRate", $"Hourly rate should be between {minRate:C} and {maxRate:C} based on your experience.");
        //            return View(model);
        //        }





        //        if (ExperienceTypes != null && ExperienceTypes.Any())
        //        {
        //            model.ExperienceTypes = string.Join(",", ExperienceTypes);
        //        }
        //        else
        //        {
        //            model.ExperienceTypes = string.Empty; 
        //        }


        //        model.ApplicationStatus = "Pending";
        //        model.SubmittedAt = DateTime.Now;

        //        // Save to the database
        //        db.JoinUs.Add(model);
        //        db.SaveChanges();

        //        TempData["SuccessMessage"] = "Your application has been submitted successfully!";
        //        return RedirectToAction("JoinUs");
        //    }

        //    TempData["ErrorMessage"] = "There were errors in your form submission. Please correct them and try again.";
        //    return View(model);
        //}





        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult JoinUs(JoinU model, string[] ExperienceTypes, HttpPostedFileBase LicenseUpload)
        {

            var existingApplication = db.JoinUs.FirstOrDefault(j => j.Email == model.Email);

            if (existingApplication != null)
            {
                TempData["ErrorMessage"] = "The email address is already registered. Please enter a different email.";
                return View(model);
            }

            if (ModelState.IsValid)
            {
                decimal minRate = 5;
                decimal maxRate = 7;

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
                else if (model.ExperienceYears >= 5 && model.ExperienceYears <= 10)
                {
                    minRate = 10;
                    maxRate = 15;
                }
                else if (model.ExperienceYears > 10)
                {
                    minRate = 15;
                    maxRate = 20;
                }

                if (model.HourlyRate < minRate || model.HourlyRate > maxRate)
                {
                    ModelState.AddModelError("HourlyRate", $"Hourly rate should be between {minRate:C} and {maxRate:C} based on your experience.");
                    return View(model);
                }





                if (ExperienceTypes != null && ExperienceTypes.Any())
                {
                    model.ExperienceTypes = string.Join(",", ExperienceTypes);
                }
                else
                {
                    model.ExperienceTypes = string.Empty;
                }



                
                if (ExperienceTypes.Contains("Special needs") || ExperienceTypes.Contains("Learning disabilities") || ExperienceTypes.Contains("Behavioral difficulties"))
                {
                    if (LicenseUpload == null || LicenseUpload.ContentLength == 0)
                    {
                        ModelState.AddModelError("LicenseUpload", "You must upload a license if you select special needs, learning disabilities, or behavioral difficulties.");
                        return View(model);
                    }
                    else
                    {
                        
                        var allowedExtensions = new[] { ".pdf", ".doc", ".docx" };
                        var fileExtension = Path.GetExtension(LicenseUpload.FileName).ToLower();

                        if (!allowedExtensions.Contains(fileExtension))
                        {
                            ModelState.AddModelError("LicenseUpload", "Only PDF, DOC, and DOCX files are allowed.");
                            return View(model);
                        }

                        
                        var fileName = Path.GetFileName(LicenseUpload.FileName);
                        var path = Path.Combine(Server.MapPath("~/Uploads/Licenses"), fileName);
                        LicenseUpload.SaveAs(path);

                       
                        model.LicensePath = "/Uploads/Licenses/" + fileName;
                    }
                }


                model.ApplicationStatus = "Pending";
                model.SubmittedAt = DateTime.Now;

                // Save to the database
                db.JoinUs.Add(model);
                db.SaveChanges();

                TempData["SuccessMessage"] = "Your application has been submitted successfully!";
                return RedirectToAction("JoinUs");
            }

            TempData["ErrorMessage"] = "There were errors in your form submission. Please correct them and try again.";
            return View(model);
        }



      

            public ActionResult PopularSitters()
            {
                var topSitters = db.Sitters
                    .OrderByDescending(s => s.Bookings.Count)
                    .Take(3)
                    .Select(s => new
                    {
                        s.SitterID,
                        s.FirstName,
                        s.LastName,
                        s.ExperienceYears,
                        s.sitterImage,
                        BookingCount = s.Bookings.Count
                    })
                    .ToList();

                return View(topSitters);
            }


        public ActionResult privacy()
        {
            return View();
        }

    }




}
