using BusinessObjects.Models;

namespace Services;

public interface ICategoryService
{
    List<Category> GetCategories();
    List<Category> GetActiveCategories();
    List<Category> SearchCategories(string? keyword, bool? isActive);
    Category? GetCategoryById(short id);
    bool CanDeleteCategory(short id);
    int CountNewsArticles(short id);
    void AddCategory(Category category);
    void UpdateCategory(Category category);
    void DeleteCategory(short id);
}
