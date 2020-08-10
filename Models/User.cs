using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace maddweb.Models
{
	public class User
	{

		public int ID { get; set; }

		[Required(ErrorMessage = "Please enter Email")]

		public string email { get; set; }

		[Required(ErrorMessage = "Please enter Name")]
		public string name { get; set; }

		[Required(ErrorMessage = "Please enter password")]
		public string pwd { get; set; }

		[Required(ErrorMessage = "Please enter contact")]
        [RegularExpression("[0-9]{8}", ErrorMessage = "Digits only")]
        public string contact { get; set; }

		[Required(ErrorMessage = "Please enter gender")]
		public string Gender { get; set; }

		public string Role { get; set; }

		public string Image { get; set; }



	}
}
