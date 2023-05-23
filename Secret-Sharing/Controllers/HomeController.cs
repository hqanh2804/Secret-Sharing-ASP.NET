using Secret_Sharing.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Secret_Sharing.Controllers
{
	public class HomeController : Controller
	{
		LoginModel db = new LoginModel();
		public ActionResult Index()
		{
			return View();
		}

		//HTTP get /Home/Register
		public ActionResult Register()
		{
			return View();
		}

		//HTTP Post /Home/Register
		[HttpPost]
		public ActionResult Register(Users user)
		{
			db.Users.Add(user);
			db.SaveChanges();
			return RedirectToAction("Login");
		}

		//HTTP get /Home/Login
		public ActionResult Login()
		{
			return View();
		}

		//HTTP Post /Home/Login
		[HttpPost]
		public ActionResult Login(Users user)
		{
			var userForm = user.Username;
			var passForm = user.Password;
			return View();
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