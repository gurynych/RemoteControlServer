﻿using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace RemoteControlServer.Models
{
    public class AuthorizationViewModel
    {

        [Required(ErrorMessage = "Поле \"почта\" не может быть пустым")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Поле \"пароль\" не может быть пустым")]
        [Display(Name = "Password")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

    }
}
