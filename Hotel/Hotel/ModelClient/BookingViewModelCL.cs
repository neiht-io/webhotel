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
        [Required(ErrorMessage = "Vui lòng nhập tên.")]
        public string CustomerName { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập địa chỉ.")]
        public string CustomerAddress { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]
        public string CustomerPhone { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Booking From")]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        [Required(ErrorMessage = "Vui lòng chọn ngày đi.")]
        public DateTime BookingFrom { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Booking To")]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        [Required(ErrorMessage = "Vui lòng chọn ngày đến.")]
        public DateTime BookingTo { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số người ở.")]
        public int NoOfMember { get; set; }

        public List<RoomViewModelCL> RoomList { get; set; } = new List<RoomViewModelCL>();
    }
}