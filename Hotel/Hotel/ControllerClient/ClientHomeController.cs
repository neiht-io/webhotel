using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Hotel.Models;
using Hotel.ViewModel;
using Hotel.Areas.Client.Models;

namespace Hotel.Areas.Client.Controllers
{
    public class ClientHomeController : Controller
    {
        private HotelDB db = new HotelDB();
        // GET: Client/Home
        public ActionResult Index()
        {
            return View();
        }

        // GET: Client/Home/RoomList
        public ActionResult RoomList()
        {
            var roomTypes = db.RoomTypes.ToList();
            var roomViewModels = new List<RoomViewModelCL>();

            foreach (var roomType in roomTypes)
            {
                var rooms = db.Rooms.Where(r => r.RoomTypeId == roomType.RoomTypeId && r.IsActive).ToList();
                var roomVMs = rooms.Select(r => new RoomViewModelCL
                {
                    RoomId = r.RoomId,
                    RoomNumber = r.RoomNumber,
                    RoomImage = r.RoomImage,
                    RoomPrice = r.RoomPrice,
                    RoomTypeName = roomType.RoomTypeName,
                    RoomCapacity = r.RoomCapacity,
                    RoomDescription = r.RoomDescription
                    
                }).ToList();

                roomViewModels.AddRange(roomVMs);
            }

            return View(roomViewModels);
        }

        // GET: Client/Home/SearchRooms
        public ActionResult SearchRooms()
        {
            return View();
        }

        // POST: Client/Home/SearchRooms
        [HttpPost]
        public ActionResult SearchRooms(BookingViewModelCL searchViewModel)
        {
            var bookingFrom = searchViewModel.BookingFrom.Date;
            var bookingTo = searchViewModel.BookingTo.Date.AddDays(1).AddTicks(-1);
            var noOfMember = searchViewModel.NoOfMember;

            var availableRooms = db.Rooms.Where(r =>
                r.IsActive &&
                r.RoomBookings.All(rb =>
                    (bookingFrom >= rb.BookingTo || bookingTo <= rb.BookingFrom) &&
                    (bookingFrom >= rb.BookingTo || bookingTo <= rb.BookingFrom) &&
                    noOfMember <= r.RoomCapacity
                )
            ).ToList();

            var roomViewModels = availableRooms.Select(r => new RoomViewModelCL
            {
                RoomId = r.RoomId,
                RoomNumber = r.RoomNumber,
                RoomImage = r.RoomImage,
                RoomPrice = r.RoomPrice,
                RoomTypeName = r.RoomType.RoomTypeName
            }).ToList();

            return PartialView("_RoomList", roomViewModels);
        }

        // GET: Client/Home/BookRoom/5
        public ActionResult BookRoom(int id)
        {
            var room = db.Rooms.Find(id);

            if (room == null)
            {
                return HttpNotFound();
            }

            var bookingViewModel = new BookingViewModelCL
            {
                RoomId = room.RoomId
            };

            return View(bookingViewModel);
        }

        // POST: Client/Home/BookRoom
        [HttpPost]
        public ActionResult BookRoom(BookingViewModelCL bookingViewModel, User user)
        {
            if (ModelState.IsValid)
            {
                var room = db.Rooms.Find(bookingViewModel.RoomId);

                if (room == null)
                {
                    return HttpNotFound();
                }

                // Kiểm tra xem phòng đã có người đặt trong khoảng thời gian đã chọn hay chưa
                bool isRoomAvailable = IsRoomAvailable(bookingViewModel.RoomId, bookingViewModel.BookingFrom, bookingViewModel.BookingTo);

                if (!isRoomAvailable)
                {
                    ModelState.AddModelError(string.Empty, "Phòng đã có người đặt trong khoảng thời gian này. Vui lòng chọn phòng khác.");
                    return View(bookingViewModel);
                }

                int? userId = Session["UserId"] as int?;// Session điều kiện 
                int numOfDays = Convert.ToInt32((bookingViewModel.BookingTo - bookingViewModel.BookingFrom).TotalDays);
                var booking = new RoomBooking
                {   
                    UserId = (int)Session["UserId"], //Vì đăng nhập là có Id nên là không càn phải so sánh
                    CustomerName = bookingViewModel.CustomerName,
                    CustomerAddress = bookingViewModel.CustomerAddress,
                    CustomerPhone = bookingViewModel.CustomerPhone,
                    BookingFrom = bookingViewModel.BookingFrom,
                    BookingTo = bookingViewModel.BookingTo,
                    RoomId = bookingViewModel.RoomId,
                    NoOfMember = bookingViewModel.NoOfMember,
                    Total = room.RoomPrice * numOfDays,
                    PaymentTypeId = 1 // Change this to the appropriate PaymentTypeId based on your data
                };

                db.RoomBookings.Add(booking);
                db.SaveChanges();

                return RedirectToAction("BookingConfirmation", new { id = booking.BookingId });
            }

            return View(bookingViewModel);
        }

        private bool IsRoomAvailable(int roomId, DateTime bookingFrom, DateTime bookingTo)
        {
            var existingBooking = db.RoomBookings.FirstOrDefault(r =>
                r.RoomId == roomId &&
                ((bookingFrom >= r.BookingFrom && bookingFrom <= r.BookingTo) ||
                 (bookingTo >= r.BookingFrom && bookingTo <= r.BookingTo)));

            return existingBooking == null;
        }


        // GET: Client/Home/BookingConfirmation/5
        public ActionResult BookingConfirmation(int id)
        {
            var booking = db.RoomBookings.Find(id);

            if (booking == null)
            {
                return HttpNotFound();
            }

            return View(booking);
        }

        // GET: Client/Home/BookingHistory
        public ActionResult BookingHistory()
        {
            var bookings = db.RoomBookings.Select(ur => ur.UserId).ToList();
              

            return View(bookings);

            //var roomViewModels = availableRooms.Select(r => new RoomViewModelCL
            //{
            //    RoomId = r.RoomId,
            //    RoomNumber = r.RoomNumber,
            //    RoomImage = r.RoomImage,
            //    RoomPrice = r.RoomPrice,
            //    RoomTypeName = r.RoomType.RoomTypeName
            //}).ToList();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

    }
}