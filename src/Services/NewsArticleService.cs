using BusinessObjects.Models;
using Repositories;

namespace Services;

public class NewsArticleService : INewsArticleService
{
    private readonly INewsArticleRepository _newsArticleRepository;

    public NewsArticleService(INewsArticleRepository newsArticleRepository)
    {
        _newsArticleRepository = newsArticleRepository;
    }

    public List<NewsArticle> SearchNewsArticles(string? keyword, short? categoryId, bool? status, short? createdById = null, int? tagId = null)
        => _newsArticleRepository.SearchNewsArticles(keyword, categoryId, status, createdById, tagId);

    public List<NewsArticle> GetActiveNewsArticles(string? keyword, short? categoryId, int? tagId = null)
        => _newsArticleRepository.SearchNewsArticles(keyword, categoryId, true, null, tagId);

    public List<NewsArticle> GetNewsHistory(short createdById, string? keyword = null)
        => _newsArticleRepository.SearchNewsArticles(keyword, null, null, createdById);

    public NewsArticle? GetNewsArticleById(string id) => _newsArticleRepository.GetNewsArticleById(id);

    public NewsReport GetReport(DateTime startDate, DateTime endDate)
    {
        var articles = _newsArticleRepository.GetNewsArticlesByPeriod(startDate, endDate);

        static List<NewsStatistic> Group(IEnumerable<NewsArticle> source, Func<NewsArticle, string> key) =>
            source.GroupBy(key)
                .Select(g => new NewsStatistic
                {
                    GroupName = g.Key,
                    Total = g.Count(),
                    Active = g.Count(a => a.NewsStatus == true),
                    Inactive = g.Count(a => a.NewsStatus != true)
                })
                .OrderByDescending(s => s.Total)
                .ThenBy(s => s.GroupName)
                .ToList();

        return new NewsReport
        {
            StartDate = startDate.Date,
            EndDate = endDate.Date,
            Articles = articles, // already sorted by CreatedDate descending
            ByCategory = Group(articles, a => a.Category?.CategoryName ?? "(No category)"),
            ByAuthor = Group(articles, a => a.CreatedBy?.AccountName ?? "(Unknown)"),
            ByDate = articles
                .GroupBy(a => a.CreatedDate?.Date)
                .OrderByDescending(g => g.Key)
                .Select(g => new NewsStatistic
                {
                    GroupName = g.Key?.ToString("dd/MM/yyyy") ?? "(No date)",
                    Total = g.Count(),
                    Active = g.Count(a => a.NewsStatus == true),
                    Inactive = g.Count(a => a.NewsStatus != true)
                })
                .ToList()
        };
    }

    public void AddNewsArticle(NewsArticle article, IEnumerable<int> tagIds) => _newsArticleRepository.AddNewsArticle(article, tagIds);

    public void UpdateNewsArticle(NewsArticle article, IEnumerable<int> tagIds) => _newsArticleRepository.UpdateNewsArticle(article, tagIds);

    public void DeleteNewsArticle(string id) => _newsArticleRepository.DeleteNewsArticle(id);
}
