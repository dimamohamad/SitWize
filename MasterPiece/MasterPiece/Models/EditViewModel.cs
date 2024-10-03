using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MasterPiece.Models
{
    public class EditViewModel
    {


        public int UserID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public HttpPostedFileBase ImageFile { get; set; }
    }
}