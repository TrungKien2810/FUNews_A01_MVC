using BusinessObjects.Models;

namespace TrungKienMVC.ViewModels;

/// <summary>
/// The public article page: the story itself plus what to read next. A view model rather
/// than ViewBag so the view is typed and the "related" set is impossible to forget.
/// </summary>
public class NewsDetailViewModel
{
    public NewsArticle Article { get; set; } = null!;
    public List<NewsArticle> Related { get; set; } = new();
}
