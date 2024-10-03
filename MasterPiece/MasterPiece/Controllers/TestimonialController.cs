using MasterPiece.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MasterPiece.Controllers
{
    public class TestimonialController : Controller
    {
        private MasterPeiceEntities db = new MasterPeiceEntities();

        public ActionResult Index()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitTestimonial(string TestimonialText)
        {
            var userId = Session["UserID"] as int?;
            if (userId == null)
            {
                return Json(new { success = false, message = "You need to log in to submit a testimonial." });
            }

            var testimonial = new Testimonial
            {
                UserID = userId.Value,
                Text = TestimonialText,
                CreatedAt = DateTime.Now,
                TestimonialSubmitted = true
            };


            db.Testimonials.Add(testimonial);
            db.SaveChanges();

            // Reset the skip status
            //var user = db.Users.Find(userId);
            //if (user != null)
            //{
            //    user.HasSkippedTestimonial = false;
            //    db.SaveChanges();
            //}

            return Json(new { success = true, message = "Thank you for your testimonial!" }); // Return JSON response
        }
    }
}