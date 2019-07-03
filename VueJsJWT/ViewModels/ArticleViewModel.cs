using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace VueJsJWT.ViewModels
{
    public class ArticleViewModel
    {
        [Required(ErrorMessage = "Имя статьи не может быть пустым")]
        [MaxLength(300, ErrorMessage = "Максимальная длина названия 120 символов")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Текст статьи не может быть пустым")]
        [MinLength(300, ErrorMessage = "Минимальная длина текста 300 символов")]
        public string Data { get; set; }

        public string Rubrics { get; set; }
    }
}
