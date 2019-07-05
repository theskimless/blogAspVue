using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace VueJsJWT.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Введите логин или Email")]
        [MinLength(3, ErrorMessage = "Минимальная длина логина 3 символа")]
        public string Login { get; set; }

        [MinLength(3, ErrorMessage = "Минимальная длина пароля 6 символов")]
        [Required(ErrorMessage = "Введите пароль")]
        public string Password { get; set; }
    }
}
