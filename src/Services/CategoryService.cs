using BusinessObjects.Models;
using Repositories;

namespace Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;

    public CategoryService(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public List<Category> GetCategories() => _categoryRepository.GetCategories();

    public List<Category> GetActiveCategories() => _categoryRepository.GetActiveCategories();

    public List<Category> SearchCategories(string? keyword, bool? isActive) => _categoryRepository.SearchCategories(keyword, isActive);

    public Category? GetCategoryById(short id) => _categoryRepository.GetCategoryById(id);

    /// <summary>A category can only be deleted when no news article (or sub category) uses it.</summary>
    public bool CanDeleteCategory(short id) => !_categoryRepository.IsCategoryInUse(id);

    public int CountNewsArticles(short id) => _categoryRepository.CountNewsArticles(id);

    public void AddCategory(Category category) => _categoryRepository.AddCategory(category);

    public void UpdateCategory(Category category) => _categoryRepository.UpdateCategory(category);

    public void DeleteCategory(short id)
    {
        if (!CanDeleteCategory(id))
        {
            throw new InvalidOperationException("This category is already used by news articles and cannot be deleted.");
        }
        _categoryRepository.DeleteCategory(id);
    }
}
