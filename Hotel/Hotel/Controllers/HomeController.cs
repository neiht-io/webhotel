using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Hotel.Areas.Client.Models;
using Hotel.Models;


namespace Hotel.Controllers
{
    public class HomeController : Controller

    { HotelDB db = new HotelDB();
        public ActionResult Index()
        {
            
            return View();
        }


        public ActionResult Signup()
        {

            return View();
        }
        [HttpPost]
        public ActionResult Signup(User user)
        {  
            db.Users.Add(user);
             db.SaveChanges();  
            

            return RedirectToAction("Login");
        }

        public ActionResult Login()
        {

            return View();
        }


        [HttpPost]
        public ActionResult Login(User user )
        {
            var UserName = user.UserName;
            var UserPassword = user.Password;
            var usercheck = db.Users.SingleOrDefault(x=>x.UserName.Equals(UserName) && x.Password.Equals(UserPassword));
          


            if (usercheck !=null){

                //Nếu đăng nhập thành công -> Đặt biến Session(Biến này tồn tại đến khi đóng chương trình)

                Session["User"] = usercheck;
                Session["UserId"] = usercheck.UserId;

            
    

                return RedirectToAction("Index", "Home"); 


            }
            else
            {

                ViewBag.LoginFail = "Đăng nhập không thành công, vui lòng nhập lại";
                return View("Login");

            }

        }



        public ActionResult Logout(User user)
        { 

            //Xóa Session đi sau khi đăng xuất

            Session["User"] = null;
            Session.Remove("UserId");


            return RedirectToAction("Index");
        }



        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}