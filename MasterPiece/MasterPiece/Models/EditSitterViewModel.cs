using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MasterPiece.Models
{
    public class EditSitterViewModel
    {


        public int SitterID { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public string PhoneNumber { get; set; }

        public string Bio { get; set; }

        public int? ExperienceYears { get; set; }
        public decimal? HourlyRate { get; set; }

        public HttpPostedFileBase ImageFile { get; set; }
    }
}