using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using VueJsJWT.Data;
using VueJsJWT.Identity;

namespace VueJsJWT.Controllers
{
    public class AccountController : Controller
    {
        private UserManager<User> _userManager;
        private IConfiguration _configuration;
        private AppDbContext _dbContext;
        public AccountController(UserManager<User> userManager, IConfiguration configuration, AppDbContext dbContext)
        {
            _userManager = userManager;
            _configuration = configuration;
            _dbContext = dbContext;
        }

        [Route("/login")]
        [HttpPost]
        public async Task<IActionResult> Login(string login, string password)
        {
            var user = await _userManager.FindByEmailAsync(login);
            if(user != null && await _userManager.CheckPasswordAsync(user, password))
            {
                var claim = new[]
                {
                    new Claim(JwtRegisteredClaimNames.Email, user.Email)
                };

                return await RefreshToken(claim, user);
            }
            return BadRequest();
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using(var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        private string GenerateToken(IEnumerable<Claim> claims)
        {
            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:SigningKey"]));
            var expiresInMinutes = Convert.ToInt32(_configuration["Jwt:ExpiresInMinutes"]);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Site"],
                audience: _configuration["Jwt:Site"],
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddMinutes(expiresInMinutes),
                signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [Route("/check")]
        [HttpGet]
        [Authorize]
        public string Check()
        {
            return JsonConvert.SerializeObject(new {
                headers = HttpContext.Request.Headers,
                auth = HttpContext.User.Identity.IsAuthenticated.ToString()
            });
            //return HttpContext.User.Identity.IsAuthenticated.ToString();
        }

        [Route("/register")]
        [HttpPost]
        public async Task<IActionResult> Register([FromBody]RegisterViewModel model)
        {
            var user = new User
            {
                Email = model.Login,
                UserName = model.Login
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Writer");

                var claim = new[]
                {
                    new Claim(JwtRegisteredClaimNames.Email, user.Email)
                };
                var newRefreshToken = GenerateRefreshToken();

                _dbContext.RefreshTokens.Add(new RefreshToken { Token = newRefreshToken, User = user });
                await _dbContext.SaveChangesAsync();

                return new ObjectResult(new
                {
                    token = GenerateToken(claim),
                    refreshToken = newRefreshToken
                });
            }
            else return BadRequest();
        }

        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidateIssuer = true,
                ValidateIssuerSigningKey = true,
                ValidAudience = _configuration["Jwt:Site"],
                ValidIssuer = _configuration["Jwt:Site"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:SigningKey"])),
                ValidateLifetime = false
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken = null;
            ClaimsPrincipal principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);

            var jwtSecurityToken = securityToken as JwtSecurityToken;
            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256))
                throw new SecurityTokenException("Invalid token");

            return principal;
        }

        private async Task<ObjectResult> RefreshToken(IEnumerable<Claim> claims, User user)
        {
            var newToken = GenerateToken(claims);
            var newRefreshToken = GenerateRefreshToken();

            _dbContext.RefreshTokens.Remove(user.RefreshTokens[0]);
            await _dbContext.RefreshTokens.AddAsync(new RefreshToken { User = user, Token = newRefreshToken });
            await _dbContext.SaveChangesAsync();

            return new ObjectResult(new
            {
                token = newToken,
                refreshToken = newRefreshToken
            });
        }

        [Route("/refresh")]
        [HttpPost]
        public async Task<IActionResult> Refresh([FromBody]JwtTokenModel refreshViewModel)
        {
            var principal = GetPrincipalFromExpiredToken(refreshViewModel.oldToken);
            var user = _dbContext.Users.Include(p => p.RefreshTokens).FirstOrDefault(p => p.Email == principal.Claims.ElementAt(0).Value);
            if (refreshViewModel.oldRefreshToken != user.RefreshTokens[0].Token)
                throw new SecurityTokenException("Invalid refresh token");

            return await RefreshToken(principal.Claims, user);
        }
    }
    public class JwtTokenModel
    {
        public string oldToken { get; set; }
        public string oldRefreshToken { get; set; }
    }
    public class RegisterViewModel
    {
        public string Login { get; set; }
        public string Password { get; set; }
    }
}
