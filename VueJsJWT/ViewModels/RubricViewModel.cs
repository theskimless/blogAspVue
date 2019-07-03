using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace VueJsJWT.ViewModels
{
    public class RubricViewModel
    {
        [Required(ErrorMessage = "Имя рубрики не может быть пустым")]
        public string Name { get; set; }
    }
}
