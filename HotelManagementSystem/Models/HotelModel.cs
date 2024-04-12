using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelManagementSystem.Models
{
    public class HotelModel
    {
        public int? Floor { get; set; }
        public string Room { get; set; }
        public bool? isBooked { get; set; }
        public int? KeyCardNo { get; set; }
        public string BookedBy { get; set; }
        public int? BookedByAge { get; set; }
    }
}
