using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using VueJsJWT.Data;
using VueJsJWT.Models;
using VueJsJWT.ViewModels;

namespace VueJsJWT.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class BlogController : ControllerBase
    {
        private const int articlesPerPage = 10;
        private const int previewArticleSymbols = 150;

        private AppDbContext _dbContext;
        public BlogController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // POST CREATE RUBRIC
        [HttpGet("getData")]
        public JsonResult GetData()
        {
            var indexView = new IndexViewModel { Rubrics = _dbContext.Rubrics.ToList() };
            indexView.Articles = _dbContext.Articles.Skip(Math.Max(0, _dbContext.Articles.Count() - articlesPerPage)).ToList();
            indexView.Articles.ForEach(p => {
                if (p.Data.Length > previewArticleSymbols) p.Data = p.Data.Substring(0, previewArticleSymbols);
            });

            foreach(var article in indexView.Articles)
            {
                indexView.ArticleRubrics.AddRange(_dbContext.ArticleRubrics.Where(p => p.ArticleId == article.Id));
            }
            
            return new JsonResult(indexView);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("getArticle")]
        public string GetArticle(int articleId)
        {
            var articleData = _dbContext.Articles.FirstOrDefault(p => p.Id == articleId).Data;
            if (articleData.Length > previewArticleSymbols) return articleData.Substring(previewArticleSymbols);
            return "";
        }

        // POST CREATE ARTICLE
        [Authorize(Roles = "Admin")]
        [HttpPost("createArticle")]
        public async Task<ActionResult> CreateArticle([FromBody]ArticleViewModel model)
        {
            if (!string.IsNullOrEmpty(model.Title) && !string.IsNullOrEmpty(model.Rubrics) && !string.IsNullOrEmpty(model.Data))
            {
                var article = new Article { Title = model.Title, Data = model.Data, Date = DateTime.Now };
                _dbContext.Articles.Add(article);
                _dbContext.SaveChanges();

                foreach (var rubric in model.Rubrics.Split(","))
                {
                    _dbContext.ArticleRubrics.Add(new ArticleRubric { ArticleId = article.Id, RubricId = int.Parse(rubric) });
                }
                _dbContext.SaveChanges();
            }

            return Ok();
        }

        // POST CREATE RUBRIC
        [Authorize(Roles = "Admin")]
        [HttpPost("createRubric")]
        public async Task<ActionResult> CreateRubric(RubricViewModel model)
        {
            await _dbContext.Rubrics.AddAsync(new Rubric { Name = model.Name });
            await _dbContext.SaveChangesAsync();
            return Ok();
        }

        public class IdModel
        {
            [Required]
            public int Id { get; set; }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("deleteArticle")]
        public async Task DeleteArticle([FromBody]IdModel model)
        {
            _dbContext.Articles.Remove(_dbContext.Articles.FirstOrDefault(p => p.Id == model.Id));
            _dbContext.ArticleRubrics.RemoveRange(_dbContext.ArticleRubrics.Where(p => p.ArticleId == model.Id));
            await _dbContext.SaveChangesAsync();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("deleteRubric")]
        public async Task<ActionResult> DeleteRubric([FromBody]IdModel model)
        {
            var articles = _dbContext.ArticleRubrics.Where(p => p.RubricId == model.Id);

            //WE CAN'T DELETE RUBRIC IF THERE IS AN ARTICLE WHICH HAS ONLY THIS RUBRIC
            var hasArticleWithOneRubric = false;
            foreach (var article in articles)
            {
                if (_dbContext.ArticleRubrics.Where(p => p.ArticleId == article.ArticleId).Count() == 1)
                {
                    hasArticleWithOneRubric = true;
                    break;
                }
            }
            if (hasArticleWithOneRubric) return BadRequest("Невозможно удалить. Существуют статьи состоящие только из данной рубрики.");
            else
            {
                _dbContext.Rubrics.Remove(_dbContext.Rubrics.FirstOrDefault(p => p.Id == model.Id));
                await _dbContext.SaveChangesAsync();
            }

            return Ok();
        }

        //// GET api/values
        //[HttpGet]
        //public ActionResult<IEnumerable<string>> Get()
        //{
        //    return new string[] { "value1", "value2" };
        //}

        //// GET api/values/5
        //[HttpGet("{id}")]
        //public ActionResult<string> Get(int id)
        //{
        //    return "value";
        //}


        //// PUT api/values/5
        //[HttpPut("{id}")]
        //public void Put(int id, [FromBody] string value)
        //{
        //}

        //// DELETE api/values/5
        //[HttpDelete("{id}")]
        //public void Delete(int id)
        //{
        //}
    }
}
