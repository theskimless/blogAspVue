using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace VueJsJWT.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Введите логин")]
        [MinLength(3, ErrorMessage = "Минимальная длина логина 3 символа")]
        public string Login { get; set; }

        [Required(ErrorMessage = "Введите Email")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "Введите пароль")]
        [MinLength(3, ErrorMessage = "Минимальная длина пароля 6 символов")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Подтвердите пароль")]
        [Compare("Password", ErrorMessage = "Пароли не совпадают")]
        public string ConfirmPassword { get; set; }
    }
}
