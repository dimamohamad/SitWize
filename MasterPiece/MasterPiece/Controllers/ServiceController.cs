using MasterPiece.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MasterPiece.Controllers
{
    public class ServiceController : Controller
    {
        private MasterPeiceEntities db = new MasterPeiceEntities();
        public ActionResult Index()
        {
            var services = db.Services.ToList();
            return View(services);
        }

        public ActionResult ServiceDetails(int id)
        {
            var service = db.Services.Include("ServiceDetails").FirstOrDefault(s => s.ServiceID == id);

            if (service == null)
            {
                return HttpNotFound();
            }

            return View(service);
        }
        public ActionResult BookService(int id)
        {
            var service = db.Services.FirstOrDefault(s => s.ServiceID == id);

            if (service == null)
            {
                return HttpNotFound();
            }

            switch (id)
            {
                case 1:
                    return View("BookService1", service);
                case 2:
                    return View("BookService2", service);
                case 3:
                    return View("BookService3", service);
                case 4:
                    return View("BookService4", service);
                case 5:
                    return View("BookService5", service);
                default:
                    return HttpNotFound();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult BookService1(int id, FormCollection form)
        {
            var service = db.Services.FirstOrDefault(s => s.ServiceID == id);

            if (service == null)
            {
                return HttpNotFound();
            }


            if (string.IsNullOrEmpty(service.ServiceName))
            {
                ModelState.AddModelError("DetailType", "Service Name cannot be empty.");
                return View("BookService1", service);
            }


            var serviceDetail = new ServiceDetail
            {
                ServiceID = id,
                DetailType = service.ServiceName, 
                StartDate = string.IsNullOrEmpty(form["StartDate"]) ? (DateTime?)null : DateTime.Parse(form["StartDate"]),
                StartDateFlexible = form["StartDateFlexible"] == "on",
                Days = string.IsNullOrEmpty(form["Days"]) ? null : form["Days"],
                StartTime = string.IsNullOrEmpty(form["StartTime"]) ? (TimeSpan?)null : TimeSpan.Parse(form["StartTime"]),
                EndTime = string.IsNullOrEmpty(form["EndTime"]) ? (TimeSpan?)null : TimeSpan.Parse(form["EndTime"]),
                Duration = string.IsNullOrEmpty(form["Duration"]) ? null : form["Duration"],
                Frequency = string.IsNullOrEmpty(form["Frequency"]) ? null : form["Frequency"],
                ScheduleType = string.IsNullOrEmpty(form["ScheduleType"]) ? null : form["ScheduleType"],
                SpecialNeedsTypes = string.IsNullOrEmpty(form["SpecialNeedsTypes"]) ? null : form["SpecialNeedsTypes"],
                AdditionalInfo = string.IsNullOrEmpty(form["AdditionalInfo"]) ? null : form["AdditionalInfo"]
            };

            try
            {

                db.ServiceDetails.Add(serviceDetail);
                db.SaveChanges();

                System.Diagnostics.Debug.WriteLine("ServiceDetail saved successfully with DetailID: " + serviceDetail.DetailID);
            }
            catch (DbEntityValidationException ex)
            {

                foreach (var validationErrors in ex.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        System.Diagnostics.Debug.WriteLine($"Property: {validationError.PropertyName} Error: {validationError.ErrorMessage}");

                        ModelState.AddModelError(validationError.PropertyName, validationError.ErrorMessage);
                    }
                }


                return View("BookService1", service);
            }



            Session["ServiceDetailID"] = serviceDetail.DetailID;
            return RedirectToAction("SelectSitter", "Booking");

        }
        public ActionResult BookService2(int id)
        {
            var service = db.Services.FirstOrDefault(s => s.ServiceID == id);

            if (service == null)
            {
                return HttpNotFound();
            }

            return View(service);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult BookService2(int id, FormCollection form)
        {
            var service = db.Services.FirstOrDefault(s => s.ServiceID == id);

            if (service == null)
            {
                return HttpNotFound();
            }


            TimeSpan? startTime = TimeSpan.TryParse(form["StartTime"], out TimeSpan parsedStartTime) ? (TimeSpan?)parsedStartTime : null;
            TimeSpan? endTime = TimeSpan.TryParse(form["EndTime"], out TimeSpan parsedEndTime) ? (TimeSpan?)parsedEndTime : null;
            string duration = !string.IsNullOrEmpty(form["Duration"]) ? form["Duration"] : null;

            // Check if StartTime and EndTime are valid
            if (!startTime.HasValue || !endTime.HasValue)
            {
                ModelState.AddModelError("StartTime", "Please enter valid start and end times.");
                return View(service); // Return the view with an error if times are invalid
            }


            string frequency = form["frequency"];
            bool weekdays = form["weekdays"] == "on";
            bool weekends = form["weekends"] == "on";
            bool evenings = form["evenings"] == "on";
            //string typicalDuration = form["typicalDuration"];

            DateTime? startDate = null;

            if (DateTime.TryParse(form["StartDate"], out DateTime parsedStartDate))
            {
                startDate = parsedStartDate;
            }

            bool startDateFlexible = form["StartDateFlexible"] == "on";




            //if (string.IsNullOrEmpty(typicalDuration))
            //{
            //    ModelState.AddModelError("typicalDuration", "Please enter the typical duration of care.");
            //    return View(service);
            //}
            // Set days based on selection
            string days = string.Empty;
            if (weekdays)
            {
                days = "Sunday, Monday, Tuesday, Wednesday, Thursday";
            }
            if (weekends)
            {
                days = string.IsNullOrEmpty(days) ? "Friday, Saturday" : days + ", Friday, Saturday";
            }






            string scheduleType = string.Join(", ", new List<string>
            {
                weekdays ? "Weekdays" : null,
                weekends ? "Weekends" : null,
                evenings ? "Evenings" : null
            }.Where(s => !string.IsNullOrEmpty(s)));

            var serviceDetail = new ServiceDetail
            {
                ServiceID = id,
                DetailType = service.ServiceName,
                Frequency = frequency,
                ScheduleType = scheduleType,
                Duration = duration,
                Days = days,
                StartDate = startDate,
                StartDateFlexible = startDateFlexible,
                StartTime = startTime,
                EndTime = endTime
            };

            try
            {
                db.ServiceDetails.Add(serviceDetail);
                db.SaveChanges();

                System.Diagnostics.Debug.WriteLine("ServiceDetail saved successfully with DetailID: " + serviceDetail.DetailID);
            }
            catch (DbEntityValidationException ex)
            {
                foreach (var validationErrors in ex.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        System.Diagnostics.Debug.WriteLine($"Property: {validationError.PropertyName} Error: {validationError.ErrorMessage}");
                        ModelState.AddModelError(validationError.PropertyName, validationError.ErrorMessage);
                    }
                }

                return View(service);
            }
            Session["ServiceDetailID"] = serviceDetail.DetailID;
            return RedirectToAction("SelectSitter", "Booking");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult BookService3(int id, FormCollection form)
        {
            var service = db.Services.FirstOrDefault(s => s.ServiceID == id);

            if (service == null)
            {
                return HttpNotFound();
            }


            string dateNeeded = form["dateNeeded"];
            string startTime = form["startTime"];
            string endTime = form["endTime"];
            string duration = form["duration"];
            string reason = form["reason"];

            var serviceDetail = new ServiceDetail
            {
                ServiceID = id,
                DetailType = service.ServiceName,
                StartDate = string.IsNullOrEmpty(dateNeeded) ? (DateTime?)null : DateTime.Parse(dateNeeded),
                StartTime = string.IsNullOrEmpty(startTime) ? (TimeSpan?)null : TimeSpan.Parse(startTime),
                EndTime = string.IsNullOrEmpty(endTime) ? (TimeSpan?)null : TimeSpan.Parse(endTime),
                Duration = duration,
                AdditionalInfo = reason
            };

            try
            {
                db.ServiceDetails.Add(serviceDetail);
                db.SaveChanges();

                System.Diagnostics.Debug.WriteLine("ServiceDetail saved successfully with DetailID: " + serviceDetail.DetailID);
            }
            catch (DbEntityValidationException ex)
            {
                foreach (var validationErrors in ex.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        System.Diagnostics.Debug.WriteLine($"Property: {validationError.PropertyName} Error: {validationError.ErrorMessage}");
                        ModelState.AddModelError(validationError.PropertyName, validationError.ErrorMessage);
                    }
                }

                return View(service);
            }
            catch (Exception ex)
            {

                System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
                ModelState.AddModelError("", "An error occurred while saving the service details. Please try again.");
                return View(service);
            }
            Session["ServiceDetailID"] = serviceDetail.DetailID;
            return RedirectToAction("SelectSitter", "Booking");
        }






        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult BookService4(int id, FormCollection form)
        {
            var service = db.Services.FirstOrDefault(s => s.ServiceID == id);

            if (service == null)
            {
                return HttpNotFound();
            }

            // Extracting form data
            string scheduleType = form["scheduleType"];
            DateTime? startDate = string.IsNullOrEmpty(form["startDate"]) ? (DateTime?)null : DateTime.Parse(form["startDate"]);
            bool startDateFlexible = form["flexibleStart"] == "on";
             string startTime = form["startTime"];
            string endTime = form["endTime"];
            string duration = form["duration"];
            string daysTime = string.Join(", ", new List<string>
            {
                form["morning"] == "on" ? "Morning" : null,
                form["daytime"] == "on" ? "Daytime" : null,
                form["afternoon"] == "on" ? "Afternoon" : null,
                form["evening"] == "on" ? "Evening" : null,
                form["weekendDaytime"] == "on" ? "Weekend Daytime" : null,
                form["weekendEvening"] == "on" ? "Weekend Evening" : null
            }.Where(s => !string.IsNullOrEmpty(s)));

            //string duration = form["duration"];
            var serviceDetail = new ServiceDetail
            {
                ServiceID = id,
                DetailType = service.ServiceName,
                ScheduleType = scheduleType,
                StartDate = startDate,
                StartDateFlexible = startDateFlexible,
                Days = daysTime,
                StartTime = string.IsNullOrEmpty(startTime) ? (TimeSpan?)null : TimeSpan.Parse(startTime),
                EndTime = string.IsNullOrEmpty(endTime) ? (TimeSpan?)null : TimeSpan.Parse(endTime),
                Duration = duration,
               
                AdditionalInfo = form["AdditionalInfo"] 
            };

            try
            {
               
                db.ServiceDetails.Add(serviceDetail);
                db.SaveChanges();

                System.Diagnostics.Debug.WriteLine("ServiceDetail saved successfully with DetailID: " + serviceDetail.DetailID);
            }
            catch (DbEntityValidationException ex)
            {
                foreach (var validationErrors in ex.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        System.Diagnostics.Debug.WriteLine($"Property: {validationError.PropertyName} Error: {validationError.ErrorMessage}");
                        ModelState.AddModelError(validationError.PropertyName, validationError.ErrorMessage);
                    }
                }

                return View("BookService4", service);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
                ModelState.AddModelError("", "An error occurred while saving the service details. Please try again.");
                return View("BookService4", service);
            }

            
            Session["ServiceDetailID"] = serviceDetail.DetailID;
            return RedirectToAction("SelectSitter", "Booking");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult BookService5(int id, FormCollection form)
        {
            var service = db.Services.FirstOrDefault(s => s.ServiceID == id);

            if (service == null)
            {
                return HttpNotFound();
            }

          
            string specialNeedsTypes = string.Join(", ", new List<string>
            {
                form["autism"] == "on" ? "Autism Spectrum Disorder" : null,
                form["adhd"] == "on" ? "ADHD" : null,
                form["downsyndrome"] == "on" ? "Down Syndrome" : null,
                form["cerebralpalsy"] == "on" ? "Cerebral Palsy" : null,
                form["hearingimpairment"] == "on" ? "Hearing Impairment" : null,
                form["visualimpairment"] == "on" ? "Visual Impairment" : null,
                form["speechlanguage"] == "on" ? "Speech/Language Disorders" : null,
                form["other"] == "on" ? form["otherSpecialNeeds"] : null
            }.Where(s => !string.IsNullOrEmpty(s)));

            string frequency = form["frequency"];
            DateTime? startDate = string.IsNullOrEmpty(form["startDate"]) ? (DateTime?)null : DateTime.Parse(form["startDate"]);
            TimeSpan? startTime = string.IsNullOrEmpty(form["startTime"]) ? (TimeSpan?)null : TimeSpan.Parse(form["startTime"]);
            TimeSpan? endTime = string.IsNullOrEmpty(form["endTime"]) ? (TimeSpan?)null : TimeSpan.Parse(form["endTime"]);
            string duration = form["duration"];
            string additionalInfo = form["additionalInfo"];

            
            var serviceDetail = new ServiceDetail
            {
                ServiceID = id,
                DetailType = service.ServiceName,
                SpecialNeedsTypes = specialNeedsTypes,
                Frequency = frequency,
                StartDate = startDate,
                StartTime = startTime,
                EndTime = endTime,
                Duration = duration,
                AdditionalInfo = additionalInfo
            };

            try
            {
                
                db.ServiceDetails.Add(serviceDetail);
                db.SaveChanges();

                System.Diagnostics.Debug.WriteLine("ServiceDetail saved successfully with DetailID: " + serviceDetail.DetailID);
            }
            catch (DbEntityValidationException ex)
            {
                foreach (var validationErrors in ex.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        System.Diagnostics.Debug.WriteLine($"Property: {validationError.PropertyName} Error: {validationError.ErrorMessage}");
                        ModelState.AddModelError(validationError.PropertyName, validationError.ErrorMessage);
                    }
                }

                return View("BookService5", service);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
                ModelState.AddModelError("", "An error occurred while saving the service details. Please try again.");
                return View("BookService5", service);
            }

            
            Session["ServiceDetailID"] = serviceDetail.DetailID;
            return RedirectToAction("SelectSitter", "Booking");
        }

        
    }
}




//[HttpPost]
//[ValidateAntiForgeryToken]
//public ActionResult BookService4(int id, FormCollection form)
//{
//    var service = db.Services.Where(s => s.ServiceID == id).FirstOrDefault();

//    if (service == null)
//    {
//        return HttpNotFound();

//    }

//    bool fullTime = form["fullTime"] == "on";
//    bool partTime = form["partTime"] == "on";

//    string scheduleType = string.Join(", ", new List<string> {

//    fullTime?"Full-time":null,
//    partTime?"Part-time":null


//    }.Where(s => !string.IsNullOrEmpty(s)));





//    var serviceDetail = new ServiceDetail
//    {

//        ServiceID = id,
//        ScheduleType = scheduleType,
//        StartDate = string.IsNullOrEmpty(form["StartDate"]) ? (DateTime?)null : DateTime.Parse(form["StartDate"]),
//        StartDateFlexible = form["StartDateFlexible"] == "on",

//    }




//return View();
//}