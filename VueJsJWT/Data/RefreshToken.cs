using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VueJsJWT.Identity;

namespace VueJsJWT.Data
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public string Token { get; set; }

        public string UserId { get; set; }
        public User User { get; set; }
    }
}
