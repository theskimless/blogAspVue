using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VueJsJWT.Identity;

namespace VueJsJWT.Models
{
    public class Article
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Data { get; set; }
        public DateTime Date { get; set; }
        public User User { get; set; }
        public string UserId { get; set; }
    }
}
