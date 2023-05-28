using Secret_Sharing.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Security.Cryptography;
using System.Text;
using System.IO;
using System.Configuration;
using System.ComponentModel.DataAnnotations.Schema;

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
			var isRegisteredUser = (from u in db.Users
									where u.Username == user.Username
									select u).SingleOrDefault();
			if (isRegisteredUser != null)
			{
				ViewBag.Error = "This username already registered!";
				return View();
			}

			HashAlgorithm algorithm = SHA1.Create();
			user.Password = Encoding.UTF8.GetString(algorithm.ComputeHash(Encoding.UTF8.GetBytes(user.Password)));
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
			var validUser = (from u in db.Users
							 where u.Username == user.Username
							 select u).SingleOrDefault();
			Session["ID"] = "";
			if (validUser == null)
			{
				ViewBag.Error = "Username or password is invalid!";
				return View();
			}

			HashAlgorithm algorithm = SHA1.Create();
			string pass = Encoding.UTF8.GetString(algorithm.ComputeHash(Encoding.UTF8.GetBytes(user.Password)));
			
			if (pass.CompareTo(validUser.Password) == 0)
			{
				Session["ID"] = user.ID;
				return View("Index");
			}
			else
			{
				ViewBag.Error = "Username or password is invalid!";
				return View();
			}
		}

		public ActionResult Upload()
		{
			return View();
		}

		[HttpPost]
		public ActionResult Upload(ManageFile file)
		{

			if(ModelState.IsValid)
			{
				try
				{
					if(file != null)
					{
						string FileName = Path.GetFileNameWithoutExtension(file.File.FileName);
						string FileExtension = Path.GetExtension(file.File.FileName);

						file.ID = (int)Session["ID"];

						FileName = DateTime.Now.ToString("yyyyMMdd") + "-" + FileName.Trim() + FileExtension;
						file.Filename = FileName;

						string UploadPath = Path.Combine(Server.MapPath("~/Files/"), FileName);
						file.File.SaveAs(UploadPath);
						
						file.Url = UploadPath + FileName;

						file.CreatedDate = DateTime.Now;

						UploadModel upload = new UploadModel();

						upload.ManageFiles.Add(file);
						upload.SaveChanges();
					}

					ViewBag.FileStatus = "File uploaded successfully!";
				}

				catch (Exception)
				{
					ViewBag.FileStatus = "Error while file uploading!";
				}
			}

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