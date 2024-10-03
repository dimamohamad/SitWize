using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MasterPiece.Models
{
    public class AddSitterViewModel
    {
        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        [Display(Name = "Bio")]
        public string Bio { get; set; }

        [Required]
        [Display(Name = "Experience (Years)")]
        public int ExperienceYears { get; set; }

        [Required]
        [Display(Name = "Hourly Rate")]
        public decimal HourlyRate { get; set; }

        [Display(Name = "License")]
        public HttpPostedFileBase LicenseUpload { get; set; }

        // This property will store the path of the uploaded file
        public string LicensePath { get; set; }
    }
}