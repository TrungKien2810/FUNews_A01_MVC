using BusinessObjects.Models;

namespace TrungKienMVC.ViewModels;

public class NewsIndexViewModel
{
    public string? Keyword { get; set; }
    public short? CategoryId { get; set; }
    public bool? Status { get; set; }
    public int? TagId { get; set; }
    public List<NewsArticle> Articles { get; set; } = new();
    public List<Category> Categories { get; set; } = new();
    public List<Tag> Tags { get; set; } = new();
}
