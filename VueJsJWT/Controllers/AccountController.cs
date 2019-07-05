using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using VueJsJWT.Data;
using VueJsJWT.Identity;
using VueJsJWT.ViewModels;

namespace VueJsJWT.Controllers
{
    [ApiController]
    public class AccountController : ControllerBase
    {
        private UserManager<User> _userManager;
        private IConfiguration _configuration;
        private AppDbContext _dbContext;
        private RoleManager<IdentityRole> _roleManager;
        public AccountController(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration, AppDbContext dbContext)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _dbContext = dbContext;
        }

        [HttpPost("/login")]
        public async Task<ActionResult> Login(LoginViewModel model)
        {
            var user = _dbContext.Users.FirstOrDefault(p => p.UserName == model.Login || p.Email == model.Login);
            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                var role = await _userManager.GetRolesAsync(user);
                var claims = new[]
                {
                    new Claim(ClaimsIdentity.DefaultNameClaimType, user.UserName),
                    new Claim(ClaimsIdentity.DefaultRoleClaimType, role[0])
                };
                return await RefreshToken(claims, user);
            }
            return BadRequest();
        }

        [HttpGet("/check")]
        [Authorize(Roles = "Admin")]
        public async Task<string> Check()
        {
            var user = await _userManager.GetUserAsync(User);
            return JsonConvert.SerializeObject(new {
                headers = HttpContext.Request.Headers,
                auth = HttpContext.User.Identity.IsAuthenticated.ToString()
            });
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
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

        [HttpPost("/register")]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            var user = new User
            {
                UserName = model.Login,
                Email = model.Email
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Writer");

                var claims = new[]
                {
                    new Claim(ClaimsIdentity.DefaultNameClaimType, user.UserName),
                    new Claim(ClaimsIdentity.DefaultRoleClaimType, "Writer")
                };
                var newRefreshToken = GenerateRefreshToken();

                _dbContext.RefreshTokens.Add(new RefreshToken { Token = newRefreshToken, User = user });
                await _dbContext.SaveChangesAsync();

                return new ObjectResult(new
                {
                    token = GenerateToken(claims),
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

            _dbContext.RefreshTokens.FirstOrDefault(p => p.UserId == user.Id).Token = newRefreshToken;

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
            var user = _dbContext.Users.Include(p => p.RefreshTokens).FirstOrDefault(p => p.UserName == principal.Claims.ElementAt(0).Value);
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
}
