﻿using Secret_Sharing.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Security.Cryptography;
using System.Text;
using System.IO;
using System.Data.SqlClient;
namespace Secret_Sharing.Controllers
{
	public class HomeController : Controller
	{
		// connection string to local database
		string str = @"Data Source=QUOCANH;Initial Catalog=SecretSharing;Integrated Security=True";

		LoginModel db = new LoginModel();
		UploadModel upload = new UploadModel();
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
			// Check if username registered or not
			var isRegisteredUser = (from u in db.Users
									where u.Username == user.Username
									select u).SingleOrDefault();
			if (isRegisteredUser != null)
			{
				ViewBag.Error = "This username already registered!";
				return View();
			}

			// Hash user's password by SHA1 algorithm
			HashAlgorithm algorithm = SHA1.Create();
			user.Password = Encoding.UTF8.GetString(algorithm.ComputeHash(Encoding.UTF8.GetBytes(user.Password)));

			// Add user information to database
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
			// Check if username registered or not
			var validUser = (from u in db.Users
							 where u.Username == user.Username
							 select u).SingleOrDefault();
			Session["ID"] = "";
			if (validUser == null)
			{
				ViewBag.Error = "Username or password is invalid!";
				return View();
			}

			// Hash input password
			HashAlgorithm algorithm = SHA1.Create();
			string pass = Encoding.UTF8.GetString(algorithm.ComputeHash(Encoding.UTF8.GetBytes(user.Password)));
			
			// Compare user's input password with their registered password before
			if (pass.CompareTo(validUser.Password) == 0)
			{
				// Create a session with User ID
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
			// Get exactly ticks when uploading the file
			string fileId = DateTime.Now.Ticks.ToString("x");

			// Create a new URL of the file
			string protectedUrl = Url.Action("Download", "Files", new { id = fileId }, Request.Url.Scheme);

			return protectedUrl;
		}

		[HttpPost]
		public ActionResult Upload(HttpPostedFileBase file)
		{
			ManageFile f = new ManageFile();

			if (file != null && file.ContentLength > 0)
			{
				// Get file name and file extension to create a new name for it
				var fileExtension = Path.GetExtension(file.FileName);
				string fileName = Path.GetFileNameWithoutExtension(file.FileName);
				string newFileName = DateTime.Now.ToString("yyyyMMdd") + "-" + fileName.Trim() + fileExtension;

				// Upload the file to the server (save to folder ~/Uploads/)
				string UploadPath = Path.Combine(Server.MapPath("~/Uploads"), newFileName);
				file.SaveAs(UploadPath);

				f.ID = (int)Session["ID"];
				
				f.Filename = newFileName;

				f.Url = GenerateProtectedUrl(UploadPath);

				f.CreatedDate = DateTime.Now;

				// Save file's information to database.
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
					conn.Close();
				}
				ViewBag.Error = "File uploaded successfully!";
			}
			
			return RedirectToAction("Index");
		}

		// This function is to get file's information from database by user's input fileURL
		private ManageFile getFileFromDatabase(string fileURL)
		{
			string query = "SELECT * FROM ManageFiles WHERE Url = @url";

			using (SqlConnection conn = new SqlConnection(str))
			{
				SqlCommand command = new SqlCommand(query, conn);
				command.Parameters.AddWithValue("@url", fileURL);
				conn.Open();
				SqlDataReader reader = command.ExecuteReader();

				// Check if file exists in database and save its information to a ManageFile model
				if (reader.Read())
				{
					ManageFile file = new ManageFile();
					file.ID = (int)reader["ID"];
					file.Filename = (string)reader["Filename"];
					file.Url = (string)reader["Url"];
					file.CreatedDate = (DateTime)reader["CreatedDate"];

					return file;
				}

				conn.Close();
				return null;
			}
		}

		[HttpGet]
		public ActionResult MyFiles()
		{
			// Get current session's user id
			int userID = (int)Session["ID"];
			List<ManageFile> files = new List<ManageFile>();

			// Select all files which current user uploaded
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
				conn.Close();
			}
			return View(files);
		}

		public ActionResult Delete()
		{
			return View();
		}

		[HttpPost]
		public ActionResult Delete(string fileURL)
		{
			ManageFile file = new ManageFile();

			file = getFileFromDatabase(fileURL);

			if (file != null)
			{
				// Find file in folder ~/Uploads/ and delete it
				string filePath = Server.MapPath("~/Uploads/" + file.Filename);
				System.IO.File.Delete(filePath);

				// Also delete file on database
				using (SqlConnection conn = new SqlConnection(str))
				{
					conn.Open();
					using (SqlCommand cmd = conn.CreateCommand())
					{
						cmd.CommandText = "delete from ManageFiles where Url = @url;";
						cmd.Parameters.AddWithValue("@url", fileURL);
						cmd.ExecuteNonQuery();
					}
				}
			}
			return RedirectToAction("MyFiles");
		}

		public ActionResult DownloadFile()
		{
			return View();
		}

		[HttpPost]
		public ActionResult DownloadFile(string fileURL)
		{
			ManageFile file = new ManageFile();

			if (!string.IsNullOrEmpty(fileURL))
			{
				file = getFileFromDatabase(fileURL);

				if (file != null)
				{
					// Find the file in folder ~/Uploads/
					string filePath = Server.MapPath("~/Uploads/" + file.Filename);

					// Check if it exists then download
					if (System.IO.File.Exists(filePath))
					{
						return File(filePath, MimeMapping.GetMimeMapping(file.Filename), file.Filename);
					}
				}
			}
			return RedirectToAction("Index");
		}

		public ActionResult UploadString()
		{
			return View();
		}

		[HttpPost]
		public ActionResult UploadString (string fileName, string content)
		{
			if (string.IsNullOrEmpty(fileName) || string.IsNullOrEmpty(content))
			{
				ViewBag.Error = "File name or Content cannot be blank!";

				return RedirectToAction("Index");
			}

			// This method is similar to Upload() method
			try
			{
				ManageFile f = new ManageFile();

				string uploadFolder = Server.MapPath("~/Uploads");
				string filePath = Path.Combine(uploadFolder, fileName + ".txt");

				// This line is to write content to a .txt file
				System.IO.File.WriteAllText(filePath, content);

				string newFileName = DateTime.Now.ToString("yyyyMMdd") + "-" + fileName.Trim() + ".txt";

				f.ID = (int)Session["ID"];

				f.Filename = newFileName;

				f.Url = GenerateProtectedUrl(filePath);

				f.CreatedDate = DateTime.Now;

				// Save file information to database
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
					conn.Close();
				}
				ViewBag.Error = "Upload successfully! Your content has been written to .txt!";
			}

			catch (Exception ex)
			{
				ViewBag.Error = "Error: " + ex.Message;
			}

			return RedirectToAction("Index");
		}
	}
}