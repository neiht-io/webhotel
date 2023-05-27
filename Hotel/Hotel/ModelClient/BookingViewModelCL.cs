using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Hotel.Areas.Client.Models
{
    public class BookingViewModelCL


    {
        public int UserId { get; set; }

        public int RoomId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerAddress { get; set; }
        public string CustomerPhone { get; set; }
        [DataType(DataType.Date)]
        [Display(Name = "Booking From")]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime BookingFrom { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Booking To")]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime BookingTo { get; set; }
        public int NoOfMember { get; set; }

        public List<RoomViewModelCL> RoomList { get; set; } = new List<RoomViewModelCL>();
    }
}