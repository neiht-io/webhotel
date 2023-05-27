using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;
using Hotel.Models;
using Hotel.ViewModel;

namespace Hotel.Controllers
{
    public class Room_BookingController : Controller
    {
        private HotelDB db = new HotelDB();

        // GET: Room_Booking
        public ActionResult Index()
        {
            List<RoomBookingViewModel> listBookingViewModel =
                (from roomBooking in db.RoomBookings
                 join room in db.Rooms on roomBooking.RoomId equals room.RoomId
                 join paymenttype in db.PaymentTypes on roomBooking.PaymentTypeId equals paymenttype.PaymentTypeId
                 select new RoomBookingViewModel()
                 {
                     CustomerName = roomBooking.CustomerName,
                     CustomerAddress = roomBooking.CustomerAddress,
                     CustomerPhone = roomBooking.CustomerPhone,
                     BookingFrom = roomBooking.BookingFrom,
                     BookingTo = roomBooking.BookingTo,
                     NoOfMember = roomBooking.NoOfMember,
                     RoomNumber = room.RoomNumber,
                     RoomPrice = room.RoomPrice,
                     BookingId = roomBooking.BookingId,
                     PaymentType = paymenttype.PaymentType1,
                     NumberOfDays = System.Data.Entity.DbFunctions.DiffDays(roomBooking.BookingFrom, roomBooking.BookingTo).Value,
                     Total = roomBooking.Total
                 }).ToList();
            return View(listBookingViewModel);
        }

        // GET: Room_Booking/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RoomBooking roomBooking = db.RoomBookings.Find(id);
            if (roomBooking == null)
            {
                return HttpNotFound();
            }
            return View(roomBooking);
        }

        public ActionResult SearchRoom(BookingViewModel bvm)
        {

            if (bvm.BookingFrom == null || bvm.BookingTo == null)
            {
                return View();
            }

            var checkRoomBooked = from b in db.RoomBookings
                                  where
                                    ((bvm.BookingFrom >= b.BookingFrom) && (bvm.BookingFrom <= b.BookingTo)) ||
                                    ((bvm.BookingTo >= b.BookingFrom) && (bvm.BookingTo <= b.BookingTo)) ||
                                    ((bvm.BookingFrom <= b.BookingFrom) && (bvm.BookingTo >= b.BookingFrom)) && (bvm.BookingTo <= b.BookingTo) ||
                                    ((bvm.BookingFrom >= b.BookingFrom) && (bvm.BookingFrom <= b.BookingTo)) && (bvm.BookingTo >= b.BookingTo) ||
                                    ((bvm.BookingFrom <= b.BookingFrom) && (bvm.BookingTo >= b.BookingTo))
                                  select b;

            var availableRoom = db.Rooms.Where(r => !checkRoomBooked.Any(b => b.RoomId == r.RoomId))
                .Include(x => x.RoomType).ToList();


            foreach (var item in availableRoom)
            {
                if (item.RoomCapacity >= bvm.NoOfMember)
                {
                    bvm.listRoom.Add(item);
                }
            }

            bvm.ListOfRoom = (from room in db.Rooms
                             where (room.BookingStatusId == 2) 
                             select new SelectListItem()
                              {
                                  Text = room.RoomNumber,
                                  Value = room.RoomId.ToString()
                              }).ToList();


            bvm.ListOfPaymentType = (from paymentType in db.PaymentTypes
                                     select new SelectListItem()
                                     {
                                         Text = paymentType.PaymentType1,
                                         Value = paymentType.PaymentTypeId.ToString()
                                     }).ToList();

            bvm.BookingFrom = DateTime.Now;
            bvm.BookingTo = DateTime.Now.AddDays(1);
            return View(bvm);
        }


        // POST: Room_Booking/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(BookingViewModel bvm)
        {
            int numberOfDays = Convert.ToInt32((bvm.BookingTo - bvm.BookingFrom).TotalDays);
            Room room = db.Rooms.Single(model => model.RoomId == bvm.RoomId);
            decimal RoomPrice = room.RoomPrice;
            decimal Total = RoomPrice * numberOfDays;

            // Kiểm tra xem phòng đã được đặt hay chưa
            bool isRoomAvailable = !db.RoomBookings.Any(rb => rb.RoomId == bvm.RoomId &&
                ((rb.BookingFrom <= bvm.BookingFrom && rb.BookingTo >= bvm.BookingFrom) ||
                (rb.BookingFrom <= bvm.BookingTo && rb.BookingTo >= bvm.BookingTo)));

            if (ModelState.IsValid && isRoomAvailable)
            {
                RoomBooking RoomBooking = new RoomBooking()
                {
                    CustomerName = bvm.CustomerName,
                    CustomerAddress = bvm.CustomerAddress,
                    CustomerPhone = bvm.CustomerPhone,
                    BookingFrom = bvm.BookingFrom,
                    BookingTo = bvm.BookingTo,
                    RoomId = bvm.RoomId,
                    NoOfMember = bvm.NoOfMember,
                    PaymentTypeId = bvm.PaymentTypeId,
                    Total = Total,
                };

                db.RoomBookings.Add(RoomBooking);
                db.SaveChanges();

                return RedirectToAction("Index");
            }
            else
            {
                // Phòng đã được đặt trong khoảng thời gian này
                ViewBag.ErrorMessage = "Phòng này đã có người đặt trong khoảng thời gian này. Vui lòng chọn ngày khác.";
                return View("SearchRoom");
            }
        }
        //public ActionResult Create(BookingViewModel bvm)
        //{
        //            int numberOfDays = Convert.ToInt32((bvm.BookingTo - bvm.BookingFrom).TotalDays);
        //            Room room = db.Rooms.Single(model => model.RoomId == bvm.RoomId);
        //            decimal RoomPrice = room.RoomPrice;
        //            decimal Total = RoomPrice * numberOfDays;
        //            RoomBooking RoomBooking = new RoomBooking()
        //            {
        //                CustomerName = bvm.CustomerName,
        //                CustomerAddress = bvm.CustomerAddress,
        //                CustomerPhone = bvm.CustomerPhone,
        //                BookingFrom = bvm.BookingFrom,
        //                BookingTo = bvm.BookingTo,
        //                RoomId = bvm.RoomId,
        //                NoOfMember = bvm.NoOfMember,
        //                PaymentTypeId = bvm.PaymentTypeId,
        //                Total = Total,
        //            };
        //            if (ModelState.IsValid)
        //            {
        //                db.RoomBookings.Add(RoomBooking);
        //                db.SaveChanges();
        //            }
        //    return RedirectToAction("Index");
        //}

        // GET: Room_Booking/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RoomBooking roomBooking = db.RoomBookings.Find(id);
            if (roomBooking == null)
            {
                return HttpNotFound();
            }
            ViewBag.PaymentTypeId = new SelectList(db.PaymentTypes, "PaymentTypeId", "PaymentType1", roomBooking.PaymentTypeId);
            ViewBag.RoomId = new SelectList(db.Rooms, "RoomId", "RoomNumber", roomBooking.RoomId);
            return View(roomBooking);
        }

        // POST: Room_Booking/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "BookingId,CustomerName,CustomerAddress,CustomerPhone,BookingFrom,BookingTo,RoomId,NoOfMember,Total,PaymentTypeId")] RoomBooking roomBooking)
        {
            if (ModelState.IsValid)
            {
                db.Entry(roomBooking).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.PaymentTypeId = new SelectList(db.PaymentTypes, "PaymentTypeId", "PaymentType1", roomBooking.PaymentTypeId);
            ViewBag.RoomId = new SelectList(db.Rooms, "RoomId", "RoomNumber", roomBooking.RoomId);
            return View(roomBooking);
        }

        // GET: Room_Booking/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RoomBooking roomBooking = db.RoomBookings.Find(id);
            if (roomBooking == null)
            {
                return HttpNotFound();
            }
            return View(roomBooking);
        }

        // POST: Room_Booking/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            RoomBooking roomBooking = db.RoomBookings.Find(id);
            db.RoomBookings.Remove(roomBooking);
            db.SaveChanges();
            return RedirectToAction("Index");
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
