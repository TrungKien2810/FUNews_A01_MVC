using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services;
using TrungKienMVC.ViewModels;

namespace TrungKienMVC.Controllers;

/// <summary>Public news page: anyone (anonymous, Lecturer, ...) can view ACTIVE news articles.</summary>
[AllowAnonymous]
public class NewsController : Controller
{
    private readonly INewsArticleService _newsService;
    private readonly ICategoryService _categoryService;
    private readonly ITagService _tagService;

    public NewsController(INewsArticleService newsService, ICategoryService categoryService, ITagService tagService)
    {
        _newsService = newsService;
        _categoryService = categoryService;
        _tagService = tagService;
    }

    public IActionResult Index(string? keyword, short? categoryId, int? tagId)
    {
        var model = new NewsIndexViewModel
        {
            Keyword = keyword,
            CategoryId = categoryId,
            TagId = tagId,
            Articles = _newsService.GetActiveNewsArticles(keyword, categoryId, tagId),
            Categories = _categoryService.GetActiveCategories(),
            Tags = _tagService.GetTags()
        };
        return View(model);
    }

    public IActionResult Details(string id)
    {
        var article = _newsService.GetNewsArticleById(id);
        if (article == null || article.NewsStatus != true)
        {
            return NotFound();
        }

        return View(new NewsDetailViewModel
        {
            Article = article,
            Related = _newsService.GetRelatedArticles(article, 3)
        });
    }
}
