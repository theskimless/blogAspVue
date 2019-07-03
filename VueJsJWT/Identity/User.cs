using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VueJsJWT.Data;
using VueJsJWT.Models;

namespace VueJsJWT.Identity
{
    public class User : IdentityUser
    {
        public List<RefreshToken> RefreshTokens { get; set; }
        public List<Article> Articles { get; set; }
    }
}
