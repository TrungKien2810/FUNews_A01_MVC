using BusinessObjects.Models;
using DataAccessObjects;

namespace Repositories;

public class NewsArticleRepository : INewsArticleRepository
{
    public List<NewsArticle> SearchNewsArticles(string? keyword, short? categoryId, bool? status, short? createdById = null, int? tagId = null)
        => NewsArticleDAO.Instance.SearchNewsArticles(keyword, categoryId, status, createdById, tagId);
    public NewsArticle? GetNewsArticleById(string id) => NewsArticleDAO.Instance.GetNewsArticleById(id);
    public List<NewsArticle> GetNewsArticlesByPeriod(DateTime startDate, DateTime endDate) => NewsArticleDAO.Instance.GetNewsArticlesByPeriod(startDate, endDate);
    public List<NewsArticle> GetRelatedArticles(string articleId, short? categoryId, IReadOnlyCollection<int> tagIds, int take)
        => NewsArticleDAO.Instance.GetRelatedArticles(articleId, categoryId, tagIds, take);
    public void AddNewsArticle(NewsArticle article, IEnumerable<int> tagIds) => NewsArticleDAO.Instance.AddNewsArticle(article, tagIds);
    public void UpdateNewsArticle(NewsArticle article, IEnumerable<int> tagIds) => NewsArticleDAO.Instance.UpdateNewsArticle(article, tagIds);
    public void DeleteNewsArticle(string id) => NewsArticleDAO.Instance.DeleteNewsArticle(id);
}
