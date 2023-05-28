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
        public ActionResult SearchRooms(BookingViewModelCL bvm)
        {

            var availableRooms = db.Rooms.Where(r =>
                r.IsActive &&
                r.RoomCapacity >= bvm.NoOfMember &&
                r.RoomBookings.All(rb =>
                    (bvm.BookingFrom >= rb.BookingTo || bvm.BookingTo <= rb.BookingFrom) &&
                    (bvm.BookingFrom >= rb.BookingTo || bvm.BookingTo <= rb.BookingFrom)
                )
            ).ToList();

            var roomViewModels = availableRooms.Select(r => new RoomViewModelCL
            {
                RoomId = r.RoomId,
                RoomNumber = r.RoomNumber,
                RoomImage = r.RoomImage,
                RoomPrice = r.RoomPrice,
                RoomTypeName = r.RoomType.RoomTypeName,
                RoomCapacity = r.RoomCapacity,
                RoomDescription = r.RoomDescription
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
                // Kiểm tra người dùng nhập ngày đi > đến 
                if (bookingViewModel.BookingFrom > bookingViewModel.BookingTo)
                {
                    ModelState.AddModelError(string.Empty, "Ngày đặt phòng không hợp lệ. Vui lòng chọn lại.");
                    return View(bookingViewModel);
                }

                int? userId = Session["UserId"] as int?;// Session điều kiện 

                // Checkin 14h và Checkout 12h hôm sau
                bookingViewModel.BookingFrom = bookingViewModel.BookingFrom.Date.AddHours(14);
                bookingViewModel.BookingTo = bookingViewModel.BookingTo.Date.AddHours(12);

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
                    PaymentTypeId = 1
                };

                db.RoomBookings.Add(booking);
                db.SaveChanges();

                return RedirectToAction("BookingConfirmation", new { id = booking.BookingId });
            }

            return View(bookingViewModel);
        }

        private bool IsRoomAvailable(int roomId, DateTime bookingFrom, DateTime bookingTo)
        {
            var checkBooking = db.RoomBookings
                    .Where(r =>
                            r.RoomId == roomId &&
                            ((bookingFrom >= r.BookingFrom && bookingFrom < r.BookingTo) ||
                            (bookingTo > r.BookingFrom && bookingTo <= r.BookingTo) ||
                            (bookingFrom <= r.BookingFrom && bookingTo >= r.BookingTo)))
                            .ToList();

            return checkBooking.Count == 0;
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
            // Lấy UserId của người dùng từ Session
            int getUserId = (int)Session["UserId"];

            var bookings = db.RoomBookings
                .Where(r => r.UserId == getUserId).ToList();

            return View(bookings);
        }

        public ActionResult CancelBooking(int id)
        {

            var booking = db.RoomBookings.Find(id);

            if (booking == null)
            {
                return HttpNotFound();
            }

            db.RoomBookings.Remove(booking);
            db.SaveChanges();

            return RedirectToAction("BookingHistory");
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