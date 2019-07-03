using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VueJsJWT.Models;

namespace VueJsJWT.ViewModels
{
    public class IndexViewModel
    {
        public List<Rubric> Rubrics { get; set; }
        public List<Article> Articles { get; set; }
        public List<ArticleRubric> ArticleRubrics { get; set; }

        public IndexViewModel()
        {
            ArticleRubrics = new List<ArticleRubric>();
        }
    }
}
