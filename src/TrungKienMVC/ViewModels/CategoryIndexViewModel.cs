using BusinessObjects.Models;

namespace TrungKienMVC.ViewModels;

public class CategoryIndexViewModel
{
    public string? Keyword { get; set; }
    public bool? IsActive { get; set; }
    public List<Category> Categories { get; set; } = new();
}
