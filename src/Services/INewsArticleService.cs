using BusinessObjects.Models;

namespace Services;

public interface INewsArticleService
{
    List<NewsArticle> SearchNewsArticles(string? keyword, short? categoryId, bool? status, short? createdById = null, int? tagId = null);
    List<NewsArticle> GetActiveNewsArticles(string? keyword, short? categoryId, int? tagId = null);
    List<NewsArticle> GetNewsHistory(short createdById, string? keyword = null);
    NewsArticle? GetNewsArticleById(string id);
    NewsReport GetReport(DateTime startDate, DateTime endDate);
    void AddNewsArticle(NewsArticle article, IEnumerable<int> tagIds);
    void UpdateNewsArticle(NewsArticle article, IEnumerable<int> tagIds);
    void DeleteNewsArticle(string id);
}
