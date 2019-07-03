using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VueJsJWT.Models
{
    public class ArticleRubric
    {
        public int Id { get; set; }
        public int ArticleId { get; set; }
        public int RubricId { get; set; }
    }
}
