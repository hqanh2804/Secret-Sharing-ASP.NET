using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;

namespace Secret_Sharing.Models
{
	public partial class UploadModel : DbContext
	{
		public UploadModel()
			: base("name=UploadModel")
		{
		}

		public virtual DbSet<ManageFile> ManageFiles { get; set; }

		protected override void OnModelCreating(DbModelBuilder modelBuilder)
		{
		}
	}
}
