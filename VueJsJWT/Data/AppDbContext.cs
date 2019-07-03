using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VueJsJWT.Identity;
using VueJsJWT.Models;

namespace VueJsJWT.Data
{
    public class AppDbContext : IdentityDbContext<User>
    {
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        public DbSet<Rubric> Rubrics { get; set; }
        public DbSet<Article> Articles { get; set; }
        public DbSet<ArticleRubric> ArticleRubrics { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<IdentityRole>().HasData(
                new { Id = "1", Name = "Admin", NormalizedName = "ADMIN" },
                new { Id = "2", Name = "Writer", NormalizedName = "WRITER" }
            );
        }
    }
}
