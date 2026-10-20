namespace TrungKienMVC.Infrastructure;

/// <summary>
/// Reading time shown next to a byline. A plain 200 words per minute, floored at one
/// minute so a two-sentence brief does not claim "0 min read".
///
/// This is a presentation calculation, not a business rule: nothing decides anything
/// with it, and the only consumer is the article and story views. It lives here rather
/// than in Services so the domain layer keeps no opinion about how long a page takes
/// to read.
/// </summary>
public static class ReadTime
{
    private const int WordsPerMinute = 200;

    public static int Minutes(string? content)
    {
        if (string.IsNullOrWhiteSpace(content)) return 1;

        var words = content.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length;
        return Math.Max(1, (int)Math.Ceiling(words / (double)WordsPerMinute));
    }
}
