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
using System.Data.SqlClient;
using System.Runtime.Remoting.Messaging;

namespace Secret_Sharing.Controllers
{
	public class HomeController : Controller
	{
		string str = @"Data Source=QUOCANH;Initial Catalog=SecretSharing;Integrated Security=True";

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
				Session["ID"] = validUser.ID;
				return View("Index");
			}
			else
			{
				ViewBag.Error = "Username or password is invalid!";
				return View();
			}
		}

		private string GenerateProtectedUrl(string filePath)
		{
			string fileId = DateTime.Now.Ticks.ToString("x");

			string protectedUrl = Url.Action("Download", "Files", new { id = fileId }, Request.Url.Scheme);

			return protectedUrl;
		}

		[HttpPost]
		public ActionResult Upload(HttpPostedFileBase file)
		{
			ManageFile f = new ManageFile();

			if (file != null && file.ContentLength > 0)
			{
				var fileExtension = Path.GetExtension(file.FileName);
				string fileName = Path.GetFileNameWithoutExtension(file.FileName);
				string newFileName = DateTime.Now.ToString("yyyyMMdd") + "-" + fileName.Trim() + fileExtension;

				string UploadPath = Path.Combine(Server.MapPath("~/Uploads"), fileName);
				file.SaveAs(UploadPath);

				f.ID = (int)Session["ID"];
				
				f.Filename = newFileName;

				f.Url = GenerateProtectedUrl(UploadPath);

				f.CreatedDate = DateTime.Now;

				using (SqlConnection conn = new SqlConnection(str))
				{
					conn.Open();
					using (SqlCommand cmd = conn.CreateCommand())
					{
						cmd.CommandText = "insert into ManageFiles values (@id, @filename, @url, @date)";
						cmd.Parameters.AddWithValue("@id", f.ID);
						cmd.Parameters.AddWithValue("@filename", f.Filename);
						cmd.Parameters.AddWithValue("@url", f.Url);
						cmd.Parameters.AddWithValue("date", f.CreatedDate);
						cmd.ExecuteNonQuery();
					}
				}
				ViewBag.Error = "File uploaded successfully!";
			}
			
			return RedirectToAction("Index");
		}

		[HttpGet]
		public ActionResult MyFiles()
		{
			int userID = (int)Session["ID"];
			List<ManageFile> files = new List<ManageFile>();

			using (SqlConnection conn = new SqlConnection(str))
			{
				conn.Open();
				using (SqlCommand cmd = conn.CreateCommand())
				{
					cmd.CommandText = "SELECT * FROM ManageFiles WHERE ID = @userId";
					cmd.Parameters.AddWithValue("@userId", userID);

					using (SqlDataReader reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							ManageFile file = new ManageFile();
							file.ID = (int)reader["ID"];
							file.Filename = (string)reader["Filename"];
							file.Url = (string)reader["Url"];
							file.CreatedDate = (DateTime)reader["CreatedDate"];

							files.Add(file);
						}
					}
				}
			}
			return View(files);
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