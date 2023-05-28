namespace Secret_Sharing.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;
	using System.Web;

	public partial class ManageFile
    {
        public int ID { get; set; }

        [Required]
        [StringLength(200)]
        public string Filename { get; set; }

        [Required]
        public string Url { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        public HttpPostedFileBase File { get; set; }
    }
}
