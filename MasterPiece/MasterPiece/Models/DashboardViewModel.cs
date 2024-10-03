using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MasterPiece.Models
{
    public class DashboardViewModel
    {

        public int TotalBookings { get; set; }
        public decimal TotalRevenue { get; set; }
        public List<object> MonthlyBookings { get; set; }
        public List<object> MonthlyRevenue { get; set; }
    }
}