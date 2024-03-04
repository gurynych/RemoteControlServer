using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace RemoteControlServer.ViewModels
{
    public class AuthorizationViewModel
    {

        [EmailAddress(ErrorMessage = "Неверный email адрес")]
        [Required(ErrorMessage = "Поле \"почта\" не может быть пустым")]        
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Поле \"пароль\" не может быть пустым")]
        [Display(Name = "Password")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public string ReturnUrl { get; set; }
    }
}
