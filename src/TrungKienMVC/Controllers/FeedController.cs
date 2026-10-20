using System.Text;
using System.Xml;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services;

namespace TrungKienMVC.Controllers;

/// <summary>
/// RSS 2.0 feed of the active stories, so a reader or an aggregator can follow the
/// newsroom without opening the site. The feed renders data the public news page
/// already exposes through <see cref="INewsArticleService.GetActiveNewsArticles"/>;
/// it adds no query of its own.
/// </summary>
[AllowAnonymous]
public class FeedController : Controller
{
    private const int MaxItems = 25;

    private readonly INewsArticleService _newsService;

    public FeedController(INewsArticleService newsService)
    {
        _newsService = newsService;
    }

    [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Any)]
    [Route("feed.rss")]
    public IActionResult Index()
    {
        var site = $"{Request.Scheme}://{Request.Host}";
        var articles = _newsService.GetActiveNewsArticles(null, null).Take(MaxItems);

        var settings = new XmlWriterSettings { Indent = true, Encoding = new UTF8Encoding(false) };
        using var buffer = new MemoryStream();
        using (var xml = XmlWriter.Create(buffer, settings))
        {
            xml.WriteStartDocument();
            xml.WriteStartElement("rss");
            xml.WriteAttributeString("version", "2.0");
            xml.WriteStartElement("channel");

            xml.WriteElementString("title", "FUNews — campus newsroom");
            xml.WriteElementString("link", site + Url.Action(nameof(NewsController.Index), "News"));
            xml.WriteElementString("description", "Published stories from every department.");
            xml.WriteElementString("language", "en");

            foreach (var article in articles)
            {
                var link = site + Url.Action(nameof(NewsController.Details), "News", new { id = article.NewsArticleId });

                xml.WriteStartElement("item");
                xml.WriteElementString("title", article.NewsTitle ?? string.Empty);
                xml.WriteElementString("link", link);
                xml.WriteElementString("guid", link);
                xml.WriteElementString("description", article.Headline ?? string.Empty);
                if (article.CreatedDate.HasValue)
                {
                    xml.WriteElementString("pubDate", article.CreatedDate.Value.ToString("R"));
                }
                xml.WriteEndElement();
            }

            xml.WriteEndElement(); // channel
            xml.WriteEndElement(); // rss
            xml.WriteEndDocument();
        }

        return Content(Encoding.UTF8.GetString(buffer.ToArray()), "application/rss+xml; charset=utf-8");
    }
}
