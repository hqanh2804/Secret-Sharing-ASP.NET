namespace Secret_Sharing.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Users
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "Username cannot be blank!")]
        [StringLength(30)]
        public string Username { get; set; }

        [Required(ErrorMessage = "Email cannot be blank!")]
        [EmailAddress(ErrorMessage = "Invalid Email!")]
        [StringLength(100)]
        public string Email { get; set; }

		[Required(ErrorMessage = "Password cannot be blank!")]
		[StringLength(20)]
		public string Password { get; set; }
    }
}
