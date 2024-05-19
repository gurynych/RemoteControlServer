using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace RemoteControlServer.ViewModels
{
    public class RegistrationViewModel
    {
        [Required(ErrorMessage = "Поле \"логин\" не должно быть пустым")]
        [StringLength(20, MinimumLength = 3, ErrorMessage = "Длина строки должна быть от 3 до 20 символов")]
        [Display(Name="Логин")]
        public string Login { get; set; }

        [Required(ErrorMessage = "Поле \"почта\" не должно быть пустым")]
        [Display(Name = "Почта")]
        [EmailAddress(ErrorMessage ="Email имеет недопустимый формат")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Поле \"пароль\" не должно быть пустым")]
        [StringLength(20, MinimumLength = 3, ErrorMessage = "Длина строки должна быть от 3 до 20 символов")]
        [Display(Name = "Пароль")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Необходимо подтвердить пароль")]
        [StringLength(20, MinimumLength = 3, ErrorMessage = "Длина строки должна быть от 3 до 20 символов")]
        [Display(Name = "Подтвердить пароль")]
        [Compare("Password",ErrorMessage ="Пароли не соответствуют")]
        public string ConfirmPassword { get; set; }

		public string ReturnUrl { get; set; }
	}
}
