using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using MasterPiece.Models;
using Microsoft.Ajax.Utilities;
using PayPal.Api;
using Stripe.Billing;

namespace MasterPiece.Controllers
{
    public class BookingController : Controller
    {
        private MasterPeiceEntities db = new MasterPeiceEntities();

        // GET: Booking/SelectSitter
        //public ActionResult SelectSitter(string ExperienceRange)
        //{
        //    var sitters = db.Sitters.Where(s => s.IsAvailable == true && s.IsApproved == true).ToList();


        //    if (!string.IsNullOrEmpty(ExperienceRange))
        //    {

        //        switch (ExperienceRange)
        //        {
        //            case "0-2":
        //                sitters = sitters.Where(s => (s.ExperienceYears ?? 0) >= 0 && (s.ExperienceYears ?? 0) <= 2).ToList();
        //                break;
        //            case "2-5":
        //                sitters = sitters.Where(s => (s.ExperienceYears ?? 0) > 2 && (s.ExperienceYears ?? 0) <= 5).ToList();
        //                break;
        //            case "5-10":
        //                sitters = sitters.Where(s => (s.ExperienceYears ?? 0) > 5 && (s.ExperienceYears ?? 0) <= 10).ToList();
        //                break;
        //            case "10+":
        //                sitters = sitters.Where(s => (s.ExperienceYears ?? 0) > 10).ToList();
        //                break;
        //        }
        //    }
        //    return View(sitters);
        //}



        //public ActionResult SelectSitter()
        //{
            
        //    var sitters = db.Sitters.Where(s => s.IsAvailable == true && s.IsApproved == true).ToList();
        //    return View(sitters);
        //}


          public ActionResult SelectSitter()
        {
            int? serviceDetailId = Session["ServiceDetailID"] as int?;
            if (serviceDetailId == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest, "Service detail not found.");
            }

            var serviceDetail = db.ServiceDetails.Find(serviceDetailId);
            if (serviceDetail == null)
            {
                return HttpNotFound("Service detail not found in the database.");
            }

            var availableSitters = GetAvailableSitters(serviceDetail);
            return View(availableSitters);
        }

        private List<Sitter> GetAvailableSitters(ServiceDetail serviceDetail)
        {
            var allSitters = db.Sitters.Where(s => s.IsAvailable == true && s.IsApproved == true).ToList();
            var availableSitters = new List<Sitter>();

            foreach (var sitter in allSitters)
            {
                if (IsSitterAvailable(sitter, serviceDetail))
                {
                    availableSitters.Add(sitter);
                }
            }

            return availableSitters;
        }

        private bool IsSitterAvailable(Sitter sitter, ServiceDetail serviceDetail)
        {
            var conflictingBookings = db.Bookings
                .Where(b => b.SitterID == sitter.SitterID &&
                            b.Status == "Confirmed" &&
                            b.ServiceDetail.StartDate == serviceDetail.StartDate &&
                            ((b.ServiceDetail.StartTime <= serviceDetail.StartTime && b.ServiceDetail.EndTime > serviceDetail.StartTime) ||
                             (b.ServiceDetail.StartTime < serviceDetail.EndTime && b.ServiceDetail.EndTime >= serviceDetail.EndTime) ||
                             (b.ServiceDetail.StartTime >= serviceDetail.StartTime && b.ServiceDetail.EndTime <= serviceDetail.EndTime)))
                .Any();

            return !conflictingBookings;
        }


        public JsonResult FilterSitters(string ExperienceRange, string HourlyRateRange, bool? HasSpecialNeedsLicense)
        {
            int? serviceDetailId = Session["ServiceDetailID"] as int?;
            if (serviceDetailId == null)
            {
                return Json(new { error = "Service detail not found." }, JsonRequestBehavior.AllowGet);
            }

            var serviceDetail = db.ServiceDetails.Find(serviceDetailId);
            if (serviceDetail == null)
            {
                return Json(new { error = "Service detail not found in the database." }, JsonRequestBehavior.AllowGet);
            }

            // Get available sitters first
            var availableSitters = GetAvailableSitters(serviceDetail);

            // Apply filters to available sitters
            var filteredSitters = availableSitters.AsQueryable();

            // Filter by Experience Range
            if (!string.IsNullOrEmpty(ExperienceRange))
            {
                switch (ExperienceRange)
                {
                    case "0-2":
                        filteredSitters = filteredSitters.Where(s => (s.ExperienceYears ?? 0) >= 0 && (s.ExperienceYears ?? 0) <= 2);
                        break;
                    case "2-5":
                        filteredSitters = filteredSitters.Where(s => (s.ExperienceYears ?? 0) > 2 && (s.ExperienceYears ?? 0) <= 5);
                        break;
                    case "5-10":
                        filteredSitters = filteredSitters.Where(s => (s.ExperienceYears ?? 0) > 5 && (s.ExperienceYears ?? 0) <= 10);
                        break;
                    case "10+":
                        filteredSitters = filteredSitters.Where(s => (s.ExperienceYears ?? 0) > 10);
                        break;
                }
            }

            // Filter by Hourly Rate Range
            if (!string.IsNullOrEmpty(HourlyRateRange))
            {
                switch (HourlyRateRange)
                {
                    case "3-5":
                        filteredSitters = filteredSitters.Where(s => (s.HourlyRate ?? 0) >= 3 && (s.HourlyRate ?? 0) <= 5);
                        break;
                    case "5-7":
                        filteredSitters = filteredSitters.Where(s => (s.HourlyRate ?? 0) > 5 && (s.HourlyRate ?? 0) <= 7);
                        break;
                    case "7-10":
                        filteredSitters = filteredSitters.Where(s => (s.HourlyRate ?? 0) > 7 && (s.HourlyRate ?? 0) <= 10);
                        break;
                    case "10-15":
                        filteredSitters = filteredSitters.Where(s => (s.HourlyRate ?? 0) > 10 && (s.HourlyRate ?? 0) <= 15);
                        break;
                    case "15+":
                        filteredSitters = filteredSitters.Where(s => (s.HourlyRate ?? 0) > 15);
                        break;
                }
            }

            // Filter by Special Needs License
            if (HasSpecialNeedsLicense.HasValue)
            {
                filteredSitters = filteredSitters.Where(s => !string.IsNullOrEmpty(s.LicensePath) == HasSpecialNeedsLicense.Value);
            }

            // After applying filters, check if the sitters are still available using the IsSitterAvailable method
            var finalSitters = filteredSitters
                .Where(sitter => IsSitterAvailable(sitter, serviceDetail))
                .ToList();

            // Convert to list and select the required properties
            var result = finalSitters.Select(s => new
            {
                s.SitterID,
                s.FirstName,
                s.LastName,
                s.ExperienceYears,
                s.HourlyRate,
                s.Bio,
                s.sitterImage,
                s.LicensePath,
                HasSpecialNeedsLicense = !string.IsNullOrEmpty(s.LicensePath)
            });

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        //بداية قبل تعديل الفلتر النسخة الاخيرة 
        //public JsonResult FilterSitters(string ExperienceRange, string HourlyRateRange, bool? HasSpecialNeedsLicense)
        //{
        //    int? serviceDetailId = Session["ServiceDetailID"] as int?;
        //    if (serviceDetailId == null)
        //    {
        //        return Json(new { error = "Service detail not found." }, JsonRequestBehavior.AllowGet);
        //    }

        //    var serviceDetail = db.ServiceDetails.Find(serviceDetailId);
        //    if (serviceDetail == null)
        //    {
        //        return Json(new { error = "Service detail not found in the database." }, JsonRequestBehavior.AllowGet);
        //    }

        //    // Get available sitters first
        //    var availableSitters = GetAvailableSitters(serviceDetail);

        //    // Apply filters to available sitters
        //    var filteredSitters = availableSitters.AsQueryable();

        //    // Filter by Experience Range
        //    if (!string.IsNullOrEmpty(ExperienceRange))
        //    {
        //        switch (ExperienceRange)
        //        {
        //            case "0-2":
        //                filteredSitters = filteredSitters.Where(s => (s.ExperienceYears ?? 0) >= 0 && (s.ExperienceYears ?? 0) <= 2);
        //                break;
        //            case "2-5":
        //                filteredSitters = filteredSitters.Where(s => (s.ExperienceYears ?? 0) > 2 && (s.ExperienceYears ?? 0) <= 5);
        //                break;
        //            case "5-10":
        //                filteredSitters = filteredSitters.Where(s => (s.ExperienceYears ?? 0) > 5 && (s.ExperienceYears ?? 0) <= 10);
        //                break;
        //            case "10+":
        //                filteredSitters = filteredSitters.Where(s => (s.ExperienceYears ?? 0) > 10);
        //                break;
        //        }
        //    }

        //    // Filter by Hourly Rate Range
        //    if (!string.IsNullOrEmpty(HourlyRateRange))
        //    {
        //        switch (HourlyRateRange)
        //        {
        //            case "3-5":
        //                filteredSitters = filteredSitters.Where(s => (s.HourlyRate ?? 0) >= 3 && (s.HourlyRate ?? 0) <= 5);
        //                break;
        //            case "5-7":
        //                filteredSitters = filteredSitters.Where(s => (s.HourlyRate ?? 0) > 5 && (s.HourlyRate ?? 0) <= 7);
        //                break;
        //            case "7-10":
        //                filteredSitters = filteredSitters.Where(s => (s.HourlyRate ?? 0) > 7 && (s.HourlyRate ?? 0) <= 10);
        //                break;
        //            case "10-15":
        //                filteredSitters = filteredSitters.Where(s => (s.HourlyRate ?? 0) > 10 && (s.HourlyRate ?? 0) <= 15);
        //                break;
        //            case "15+":
        //                filteredSitters = filteredSitters.Where(s => (s.HourlyRate ?? 0) > 15);
        //                break;
        //        }
        //    }

        //    // Filter by Special Needs License
        //    if (HasSpecialNeedsLicense.HasValue)
        //    {
        //        filteredSitters = filteredSitters.Where(s => !string.IsNullOrEmpty(s.LicensePath) == HasSpecialNeedsLicense.Value);
        //    }

        //    // Convert to list and select the required properties
        //    var result = filteredSitters.ToList().Select(s => new
        //    {
        //        s.SitterID,
        //        s.FirstName,
        //        s.LastName,
        //        s.ExperienceYears,
        //        s.HourlyRate,
        //        s.Bio,
        //        s.sitterImage,
        //        s.LicensePath,
        //        HasSpecialNeedsLicense = !string.IsNullOrEmpty(s.LicensePath)
        //    });

        //    return Json(result, JsonRequestBehavior.AllowGet);
        //}



        //قبل تعديل الفلتر النسخة الاخيرة نهاية 

        //public JsonResult FilterSitters(string ExperienceRange, string HourlyRateRange,bool? HasSpecialNeedsLicense)
        //{
        //    var sitters = db.Sitters.Where(s => s.IsAvailable == true && s.IsApproved == true).ToList();

        //    // Filter by Experience Range
        //    if (!string.IsNullOrEmpty(ExperienceRange))
        //    {
        //        switch (ExperienceRange)
        //        {
        //            case "0-2":
        //                sitters = sitters.Where(s => (s.ExperienceYears ?? 0) >= 0 && (s.ExperienceYears ?? 0) <= 2).ToList();
        //                break;
        //            case "2-5":
        //                sitters = sitters.Where(s => (s.ExperienceYears ?? 0) > 2 && (s.ExperienceYears ?? 0) <= 5).ToList();
        //                break;
        //            case "5-10":
        //                sitters = sitters.Where(s => (s.ExperienceYears ?? 0) > 5 && (s.ExperienceYears ?? 0) <= 10).ToList();
        //                break;
        //            case "10+":
        //                sitters = sitters.Where(s => (s.ExperienceYears ?? 0) > 10).ToList();
        //                break;
        //        }
        //    }

        //    // Filter by Hourly Rate Range
        //    if (!string.IsNullOrEmpty(HourlyRateRange))
        //    {
        //        switch (HourlyRateRange)
        //        {
        //            case "3-5":
        //                sitters = sitters.Where(s => (s.HourlyRate ?? 0) >= 3 && (s.HourlyRate ?? 0) <= 5).ToList();
        //                break;
        //            case "5-7":
        //                sitters = sitters.Where(s => (s.HourlyRate ?? 0) > 5 && (s.HourlyRate ?? 0) <= 7).ToList();
        //                break;
        //            case "7-10":
        //                sitters = sitters.Where(s => (s.HourlyRate ?? 0) > 7 && (s.HourlyRate ?? 0) <= 10).ToList();
        //                break;
        //            case "10-15":
        //                sitters = sitters.Where(s => (s.HourlyRate ?? 0) > 10 && (s.HourlyRate ?? 0) <= 15).ToList();
        //                break;
        //            case "15+":
        //                sitters = sitters.Where(s => (s.HourlyRate ?? 0) > 15).ToList();
        //                break;
        //        }
        //    }
        //    // Filter by Special Needs License
        //    if (HasSpecialNeedsLicense.HasValue)
        //    {
        //        if (HasSpecialNeedsLicense.Value)
        //        {
        //            sitters = sitters.Where(s => !string.IsNullOrEmpty(s.LicensePath)).ToList();
        //        }
        //        else
        //        {
        //            sitters = sitters.Where(s => string.IsNullOrEmpty(s.LicensePath)).ToList();
        //        }
        //    }

        //    // Return the filtered sitters as JSON
        //    return Json(sitters.Select(s => new
        //    {
        //        s.SitterID,
        //        s.FirstName,
        //        s.LastName,
        //        s.ExperienceYears,
        //        s.HourlyRate,
        //        s.Bio,
        //        s.sitterImage,
        //        s.LicensePath
        //    }), JsonRequestBehavior.AllowGet);
        //}
        public ActionResult DownloadLicense(string path)
        {
            var filePath = Server.MapPath(path);
            if (!System.IO.File.Exists(filePath))
            {
                return HttpNotFound();
            }
            return File(filePath, "application/octet-stream", Path.GetFileName(filePath));
        }




        //the last booking اخر اشي كان زابط


        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult BookSitter(int sitterId)
        //{
        //    int? serviceDetailId = Session["ServiceDetailID"] as int?;
        //    if (Session["UserId"]==null) {
        //        return RedirectToAction("Login", "Account");


        //    }

        //    int userId =(int) Session["UserId"]; 

        //    if (serviceDetailId == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Service detail not found.");
        //    }

        //    var serviceDetail = db.ServiceDetails.Find(serviceDetailId);
        //    if (serviceDetail == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.NotFound, "Service detail not found in the database.");
        //    }

        //    var sitter = db.Sitters.Find(sitterId);
        //    if (sitter == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.NotFound, "Sitter not found.");
        //    }


        //    string durationString = serviceDetail.Duration;


        //    string numericPart = System.Text.RegularExpressions.Regex.Match(durationString, @"\d+").Value;


        //    decimal durationInHours;
        //    if (!decimal.TryParse(numericPart, out durationInHours))
        //    {
        //        durationInHours = 0; 
        //    }

        //    decimal hourlyRate = sitter.HourlyRate ?? 0;
        //    decimal totalCost = durationInHours * hourlyRate;


        //    var booking = new Booking
        //    {
        //        UserID = (int)Session["UserId"],
        //        SitterID = sitterId,
        //        ServiceID = serviceDetail.ServiceID,
        //        DetailID = serviceDetailId,
        //        BookingDate = DateTime.Now,
        //        Status = "Pending",
        //        CreatedAt = DateTime.Now,
        //        TotalAmount = totalCost
        //    };

        //    db.Bookings.Add(booking);
        //    db.SaveChanges();

        //    return RedirectToAction("Payment", new { bookingId = booking.BookingID });
        //}
        //لهون شيلي الكومنت





        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult BookSitter(int sitterId)
        {
            int? serviceDetailId = Session["ServiceDetailID"] as int?;
            if (serviceDetailId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Service detail not found.");
            }

            var serviceDetail = db.ServiceDetails.Find(serviceDetailId);
            if (serviceDetail == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound, "Service detail not found in the database.");
            }

            var sitter = db.Sitters.Find(sitterId);
            if (sitter == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound, "Sitter not found.");
            }

            string durationString = serviceDetail.Duration;
            string numericPart = System.Text.RegularExpressions.Regex.Match(durationString, @"\d+").Value;
            decimal durationInHours = !decimal.TryParse(numericPart, out durationInHours) ? 0 : durationInHours;
            decimal hourlyRate = sitter.HourlyRate ?? 0;
            decimal totalCost = durationInHours * hourlyRate;

            Session["PendingBooking"] = new
            {
                SitterID = sitterId,
                ServiceID = serviceDetail.ServiceID,
                DetailID = serviceDetailId,
                TotalAmount = totalCost
            };

            if (Session["UserId"] == null)
            {
                //return RedirectToAction("Login", "Account");
                  TempData["ShowLoginAlert"] = true;
        return RedirectToAction("SelectSitter", "Booking");
                
            }

            return CreateBookingAndRedirectToPayment();
        }

        private ActionResult CreateBookingAndRedirectToPayment()
        {
            dynamic pendingBooking = Session["PendingBooking"];
            int userId = (int)Session["UserId"];

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

            return RedirectToAction("Payment", new { bookingId = booking.BookingID });
        }




        public ActionResult Payment(int bookingId)
        {
            //var booking = db.Bookings
            //    .Include(b => b.Sitter)
            //    .Include(b => b.Service)
            //    .FirstOrDefault(b => b.BookingID == bookingId);

            var booking = db.Bookings
                .Include(b => b.Sitter)
                .Include(b => b.Service)
                .Include(b => b.ServiceDetail)  // Include the ServiceDetail to get the StartDate
                .FirstOrDefault(b => b.BookingID == bookingId);

            if (booking == null)
            {
                return HttpNotFound();
            }

            return View(booking); // Pass booking data to the view
        }

        // Step 3: PayPal Success action
        //[HttpPost]
        //public ActionResult Success(int bookingId, string orderID)
        //{
        //    var booking = db.Bookings.Find(bookingId);
        //    if (booking == null)
        //    {
        //        return HttpNotFound();
        //    }

        //    // Update booking status to Confirmed
        //    booking.Status = "Confirmed";
        //    db.SaveChanges();

        //    // Save payment record
        //    var payment = new MasterPiece.Models.Payment
        //    {
        //        BookingID = bookingId,
        //        Amount = booking.TotalAmount,
        //        PaymentMethod = "PayPal",
        //        TransactionID = orderID,
        //        PaymentDate = DateTime.Now,
        //        Status = "Pending"
        //    };
        //    db.Payments.Add(payment);
        //    db.SaveChanges();

        //    TempData["SuccessMessage"] = "Your booking has been successfully completed!";
        //    return Json(new { success = true });
        //}
        [HttpPost]
        public ActionResult Success(int bookingId, string orderID, string paymentMethod)
        {
            var booking = db.Bookings.Find(bookingId);
            if (booking == null)
            {
                return HttpNotFound();
            }

            
            booking.Status = "Confirmed";
            db.SaveChanges();

           
            var payment = new MasterPiece.Models.Payment
            {
                BookingID = bookingId,
                Amount = booking.TotalAmount,
                PaymentMethod = paymentMethod, 
                TransactionID = orderID,
                PaymentDate = DateTime.Now,
                Status = "Pending"
            };
            db.Payments.Add(payment);
            db.SaveChanges();

            TempData["SuccessMessage"] = "Your booking has been successfully completed!";
            return Json(new { success = true });
        }
        public ActionResult Cancel()
        {
            TempData["ErrorMessage"] = "Payment was canceled.";
            return RedirectToAction("Index", "Home");
        }

        public static class PayPalConfiguration
        {
            public static APIContext GetAPIContext()
            {
                var clientId = System.Configuration.ConfigurationManager.AppSettings["clientId"];
                var clientSecret = System.Configuration.ConfigurationManager.AppSettings["clientSecret"];

                if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
                {
                    throw new PayPal.MissingCredentialException("PayPal clientId or clientSecret is missing from Web.config");
                }

                var accessToken = new OAuthTokenCredential(clientId, clientSecret).GetAccessToken();
                return new APIContext(accessToken);
            }
        }
    }
}











        //    return RedirectToAction("ConfirmBooking", new { bookingId = booking.BookingID });
        //}

       
        //public ActionResult ConfirmBooking(int bookingId)
        //{
        //    var booking = db.Bookings.Include(b => b.Sitter).Include(b => b.Service).FirstOrDefault(b => b.BookingID == bookingId);

        //    if (booking == null)
        //    {
        //        return HttpNotFound();
        //    }

        //    return View(booking);
        //}

