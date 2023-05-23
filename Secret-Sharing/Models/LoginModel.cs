using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;

namespace Secret_Sharing.Models
{
	public partial class LoginModel : DbContext
	{
		public LoginModel()
			: base("name=LoginModel")
		{
		}

		public virtual DbSet<Users> Users { get; set; }

		protected override void OnModelCreating(DbModelBuilder modelBuilder)
		{
		}
	}
}
