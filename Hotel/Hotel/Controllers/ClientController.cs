using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Hotel.Models;
using Hotel.ViewModel;

namespace Hotel.Controllers
{
    public class ClientController : Controller
    {
        private HotelDB db = new HotelDB();
        // GET: Client
        public ActionResult Home()
        {
            return View();
        }

        public ActionResult SelectRoomType()
        {
            var roomTypes = db.RoomTypes.ToList(); // Lấy danh sách tất cả các RoomType từ cơ sở dữ liệu
            ViewBag.RoomTypes = new SelectList(roomTypes, "RoomTypeId", "RoomTypeName"); // Truyền danh sách RoomType vào ViewBag

            return View();
        }

        [HttpPost]
        public ActionResult SelectRoomsByType(int roomTypeId)
        {
            var rooms = db.Rooms.Where(r => r.RoomTypeId == roomTypeId).ToList(); // Lọc danh sách Room theo RoomTypeId

            return PartialView("SelectRoomsByType", rooms);
        }

    }
}