using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace maddweb.Models
{
    public class RegisterUser
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Please enter Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Please enter Name")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Please enter password")]
        [DataType(DataType.Password)]
        public string Pwd { get; set; }

        [Required(ErrorMessage = "Please enter contact")]
        [RegularExpression("[0-9]{8}",ErrorMessage ="Digits only")]
        public string Contact { get; set; }

        [Required(ErrorMessage = "Please enter gender")]
        public string Gender { get; set; }


    }
}
