using HtmlAgilityPack;
using PenumbraModForwarder.Common.Interfaces;
using PenumbraModForwarder.Common.Models;
using Serilog;
using System.Net;

namespace PenumbraModForwarder.Common.Services;

public class XmaModDisplay : IXmaModDisplay
{
    private readonly ILogger _logger;

    public XmaModDisplay()
    {
        _logger = Log.Logger.ForContext<XmaModDisplay>();
    }

    /// <summary>
    /// Fetches and combines results from page 1 and page 2 of the "time_published" descending search.
    /// </summary>
    /// <returns>A list of XmaMods containing name, publisher, type, image URL, and link.</returns>
    public async Task<List<XmaMods>> GetRecentMods()
    {
        var page1Results = await ParsePageAsync(1);
        var page2Results = await ParsePageAsync(2);

        // Combine and deduplicate mods by ImageUrl
        var distinctMods = page1Results.Concat(page2Results)
            .GroupBy(m => m.ImageUrl)
            .Select(g => g.First())
            .ToList();

        return distinctMods;
    }

    /// <summary>
    /// Helper method to parse a single page of mod results.
    /// </summary>
    /// <param name="pageNumber">The page number to retrieve (1-based).</param>
    private async Task<List<XmaMods>> ParsePageAsync(int pageNumber)
    {
        //NOTE: This won't pick up NSFW mods (Should we be looking for them?)
        string url = $"https://www.xivmodarchive.com/search?sortby=time_published&sortorder=desc&dt_compat=1&page={pageNumber}";
        const string domain = "https://www.xivmodarchive.com";

        using var client = new HttpClient();
        var html = await client.GetStringAsync(url);

        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var results = new List<XmaMods>();
        var modCards = doc.DocumentNode.SelectNodes("//div[contains(@class, 'mod-card')]");
        if (modCards == null) return results;

        foreach (var modCard in modCards)
        {
            var linkNode = modCard.SelectSingleNode(".//a[@href]");
            var linkAttr = linkNode?.GetAttributeValue("href", "");
            var fullLink = string.IsNullOrWhiteSpace(linkAttr) ? "" : domain + linkAttr;

            var nameNode = modCard.SelectSingleNode(".//h5[contains(@class, 'card-title')]");
            var rawName = nameNode?.InnerText?.Trim() ?? "";
            var normalizedName = NormalizeModName(rawName);

            var publisherNode = modCard.SelectSingleNode(".//p[contains(@class, 'card-text')]/a[@href]");
            var publisherText = publisherNode?.InnerText?.Trim() ?? "";

            var typeNode = modCard.SelectNodes(".//code[contains(@class, 'text-light')]")
                ?.FirstOrDefault(n => n.InnerText.Trim().StartsWith("Type:"));
            var typeText = typeNode?.InnerText.Replace("Type:", "").Trim() ?? "";

            var imgNode = modCard.SelectSingleNode(".//img[contains(@class, 'card-img-top')]");
            var imgUrl = imgNode?.GetAttributeValue("src", "") ?? "";

            _logger.Debug("Mod parsed: Name={Name}, ImageUrl={ImageUrl}", normalizedName, imgUrl);

            // Skip mods with missing or duplicate ImageUrl
            if (string.IsNullOrWhiteSpace(imgUrl))
            {
                _logger.Warning("Mod skipped due to missing image URL: Name={Name}", normalizedName);
                continue;
            }

            results.Add(new XmaMods
            {
                Name = normalizedName,
                Publisher = publisherText,
                Type = typeText,
                ImageUrl = imgUrl,
                ModUrl = fullLink
            });
        }

        return results;
    }

    /// <summary>
    /// Normalizes mod names to ensure compatibility with Avalonia.
    /// Decodes HTML-encoded characters, removes non-ASCII characters, trims whitespace, and replaces problematic characters.
    /// </summary>
    /// <param name="name">The raw mod name.</param>
    /// <returns>The normalized mod name.</returns>
    private string NormalizeModName(string name)
    {
        // Decode HTML-encoded characters (e.g., &#39; becomes ')
        var decoded = WebUtility.HtmlDecode(name);

        // Remove non-ASCII characters
        var asciiOnly = string.Concat(decoded.Where(c => c <= 127));

        // Replace problematic characters (e.g., tabs, newlines)
        var sanitized = asciiOnly.Replace("\n", " ").Replace("\r", " ").Replace("\t", " ");

        // Collapse multiple spaces into one
        var normalized = System.Text.RegularExpressions.Regex.Replace(sanitized, "\\s+", " ").Trim();

        return normalized;
    }
}
