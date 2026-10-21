using BusinessObjects.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Services;
using TrungKienMVC.Infrastructure;
using TrungKienMVC.ViewModels;

namespace TrungKienMVC.Controllers;

/// <summary>Staff: manage news articles including tags (CRUD + search) and view own news history.</summary>
[Authorize(Roles = AccountRoles.StaffName)]
public class NewsArticlesController : Controller
{
    private readonly INewsArticleService _newsService;
    private readonly ICategoryService _categoryService;
    private readonly ITagService _tagService;

    public NewsArticlesController(INewsArticleService newsService, ICategoryService categoryService, ITagService tagService)
    {
        _newsService = newsService;
        _categoryService = categoryService;
        _tagService = tagService;
    }

    public IActionResult Index(string? keyword, short? categoryId, bool? status, int? tagId)
    {
        var model = new NewsIndexViewModel
        {
            Keyword = keyword,
            CategoryId = categoryId,
            Status = status,
            TagId = tagId,
            Articles = _newsService.SearchNewsArticles(keyword, categoryId, status, null, tagId),
            Categories = _categoryService.GetCategories(),
            Tags = _tagService.GetTags()
        };
        return View(model);
    }

    /// <summary>News history: articles created by the signed in staff.</summary>
    public IActionResult History(string? keyword)
    {
        var model = new NewsIndexViewModel
        {
            Keyword = keyword,
            Articles = _newsService.GetNewsHistory(User.GetAccountId(), keyword)
        };
        return View(model);
    }

    [HttpGet]
    public IActionResult Details(string id)
    {
        var article = _newsService.GetNewsArticleById(id);
        if (article == null) return NotFound();
        return PartialView("_NewsDetails", article);
    }

    [HttpGet]
    public IActionResult Create()
    {
        LoadFormData(null);
        return PartialView("_NewsForm", new NewsFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(NewsFormViewModel model)
    {
        ValidateCategory(model, null);
        if (!ModelState.IsValid)
        {
            // Re-render the category the user actually chose, so a validation error does not
            // silently reset the dropdown on them.
            LoadFormData(model.CategoryId);
            return PartialView("_NewsForm", model);
        }

        var now = DateTime.Now;
        var article = new NewsArticle
        {
            NewsTitle = model.NewsTitle.Trim(),
            Headline = model.Headline.Trim(),
            NewsContent = model.NewsContent.Trim(),
            NewsSource = model.NewsSource?.Trim(),
            CategoryId = model.CategoryId,
            NewsStatus = model.NewsStatus,
            CreatedById = User.GetAccountId(),
            UpdatedById = User.GetAccountId(),
            CreatedDate = now,
            ModifiedDate = now
        };
        _newsService.AddNewsArticle(article, model.SelectedTagIds);
        TempData["Success"] = $"News \"{model.NewsTitle}\" was created.";
        return Json(new { success = true });
    }

    [HttpGet]
    public IActionResult Edit(string id)
    {
        var article = _newsService.GetNewsArticleById(id);
        if (article == null) return NotFound();
        if (!IsOwner(article)) return Forbid();
        LoadFormData(article.CategoryId);
        return PartialView("_NewsForm", new NewsFormViewModel
        {
            NewsArticleId = article.NewsArticleId,
            NewsTitle = article.NewsTitle ?? string.Empty,
            Headline = article.Headline,
            NewsContent = article.NewsContent ?? string.Empty,
            NewsSource = article.NewsSource,
            CategoryId = article.CategoryId,
            NewsStatus = article.NewsStatus == true,
            SelectedTagIds = article.Tags.Select(t => t.TagId).ToList()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(NewsFormViewModel model)
    {
        var article = string.IsNullOrEmpty(model.NewsArticleId) ? null : _newsService.GetNewsArticleById(model.NewsArticleId);
        if (article == null) return NotFound();
        if (!IsOwner(article)) return Forbid();

        ValidateCategory(model, article.CategoryId);
        if (!ModelState.IsValid)
        {
            // Same as Create: keep the submitted category instead of the stored one.
            LoadFormData(model.CategoryId);
            return PartialView("_NewsForm", model);
        }

        article.NewsTitle = model.NewsTitle.Trim();
        article.Headline = model.Headline.Trim();
        article.NewsContent = model.NewsContent.Trim();
        article.NewsSource = model.NewsSource?.Trim();
        article.CategoryId = model.CategoryId;
        article.NewsStatus = model.NewsStatus;
        article.UpdatedById = User.GetAccountId();
        article.ModifiedDate = DateTime.Now;
        _newsService.UpdateNewsArticle(article, model.SelectedTagIds);
        TempData["Success"] = $"News \"{model.NewsTitle}\" was updated.";
        return Json(new { success = true });
    }

    [HttpGet]
    public IActionResult Delete(string id)
    {
        var article = _newsService.GetNewsArticleById(id);
        if (article == null) return NotFound();
        if (!IsOwner(article)) return Forbid();
        return PartialView("_DeleteNews", article);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(string id)
    {
        var article = _newsService.GetNewsArticleById(id);
        if (article == null) return NotFound();
        if (!IsOwner(article)) return Forbid();
        _newsService.DeleteNewsArticle(id);
        TempData["Success"] = $"News \"{article.NewsTitle}\" was deleted.";
        return Json(new { success = true });
    }

    /// <summary>New news must use an active category; an existing article may keep its current one.</summary>
    private void ValidateCategory(NewsFormViewModel model, short? currentCategoryId)
    {
        if (!model.CategoryId.HasValue) return;
        var category = _categoryService.GetCategoryById(model.CategoryId.Value);
        if (category == null)
        {
            ModelState.AddModelError(nameof(model.CategoryId), "Category does not exist.");
        }
        else if (category.IsActive != true && category.CategoryId != currentCategoryId)
        {
            ModelState.AddModelError(nameof(model.CategoryId), "Category is inactive.");
        }
    }

    /// <summary>
    /// A staff account manages the articles it wrote. "View news history created by him/her"
    /// only makes sense if authorship is the unit of ownership, and a newsroom where any
    /// staff member can silently rewrite another's story is a data-integrity bug, not a
    /// permission shortcut. Enforced here, not only by hiding the buttons in the ledger.
    /// </summary>
    private bool IsOwner(NewsArticle article) => article.CreatedById == User.GetAccountId();

    private void LoadFormData(short? currentCategoryId)
    {
        var categories = _categoryService.GetCategories()
            .Where(c => c.IsActive == true || c.CategoryId == currentCategoryId);
        ViewBag.Categories = new SelectList(categories, nameof(Category.CategoryId), nameof(Category.CategoryName));
        ViewBag.Tags = _tagService.GetTags();
    }
}
