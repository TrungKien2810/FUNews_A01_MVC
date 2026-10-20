using BusinessObjects.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Services;
using TrungKienMVC.ViewModels;

namespace TrungKienMVC.Controllers;

/// <summary>Staff: manage categories (CRUD + search). A category used by news articles cannot be deleted.</summary>
[Authorize(Roles = AccountRoles.StaffName)]
public class CategoriesController : Controller
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    public IActionResult Index(string? keyword, bool? isActive)
    {
        var model = new CategoryIndexViewModel
        {
            Keyword = keyword,
            IsActive = isActive,
            Categories = _categoryService.SearchCategories(keyword, isActive)
        };
        return View(model);
    }

    [HttpGet]
    public IActionResult Create()
    {
        LoadParentCategories(null);
        return PartialView("_CategoryForm", new CategoryFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(CategoryFormViewModel model)
    {
        ValidateParent(model);
        if (!ModelState.IsValid)
        {
            LoadParentCategories(null);
            return PartialView("_CategoryForm", model);
        }

        _categoryService.AddCategory(new Category
        {
            CategoryName = model.CategoryName.Trim(),
            CategoryDesciption = model.CategoryDesciption.Trim(),
            ParentCategoryId = model.ParentCategoryId,
            IsActive = model.IsActive
        });
        TempData["Success"] = $"Category \"{model.CategoryName}\" was created.";
        return Json(new { success = true });
    }

    [HttpGet]
    public IActionResult Edit(short id)
    {
        var category = _categoryService.GetCategoryById(id);
        if (category == null) return NotFound();
        LoadParentCategories(id);
        return PartialView("_CategoryForm", new CategoryFormViewModel
        {
            CategoryId = category.CategoryId,
            CategoryName = category.CategoryName,
            CategoryDesciption = category.CategoryDesciption,
            // In the sample data a root category points to itself; show it as "no parent".
            ParentCategoryId = category.ParentCategoryId == category.CategoryId ? null : category.ParentCategoryId,
            IsActive = category.IsActive == true
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(CategoryFormViewModel model)
    {
        var category = _categoryService.GetCategoryById(model.CategoryId);
        if (category == null) return NotFound();

        ValidateParent(model);
        if (!ModelState.IsValid)
        {
            LoadParentCategories(model.CategoryId);
            return PartialView("_CategoryForm", model);
        }

        category.CategoryName = model.CategoryName.Trim();
        category.CategoryDesciption = model.CategoryDesciption.Trim();
        category.ParentCategoryId = model.ParentCategoryId;
        category.IsActive = model.IsActive;
        _categoryService.UpdateCategory(category);
        TempData["Success"] = $"Category \"{model.CategoryName}\" was updated.";
        return Json(new { success = true });
    }

    [HttpGet]
    public IActionResult Delete(short id)
    {
        var category = _categoryService.GetCategoryById(id);
        if (category == null) return NotFound();
        return PartialView("_DeleteCategory", new DeleteCategoryViewModel
        {
            Category = category,
            CanDelete = _categoryService.CanDeleteCategory(id),
            NewsCount = _categoryService.CountNewsArticles(id)
        });
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(short id)
    {
        var category = _categoryService.GetCategoryById(id);
        if (category == null) return NotFound();
        if (!_categoryService.CanDeleteCategory(id))
        {
            return Json(new { success = false, message = "This category is already used by news articles and cannot be deleted." });
        }
        _categoryService.DeleteCategory(id);
        TempData["Success"] = $"Category \"{category.CategoryName}\" was deleted.";
        return Json(new { success = true });
    }

    private void ValidateParent(CategoryFormViewModel model)
    {
        if (model.ParentCategoryId.HasValue && _categoryService.GetCategoryById(model.ParentCategoryId.Value) == null)
        {
            ModelState.AddModelError(nameof(model.ParentCategoryId), "Parent category does not exist.");
        }
        if (model.IsEdit && model.ParentCategoryId == model.CategoryId)
        {
            ModelState.AddModelError(nameof(model.ParentCategoryId), "A category cannot be its own parent.");
        }
    }

    private void LoadParentCategories(short? excludeId)
    {
        ViewBag.ParentCategories = new SelectList(
            _categoryService.GetCategories().Where(c => c.CategoryId != excludeId),
            nameof(Category.CategoryId), nameof(Category.CategoryName));
    }
}
