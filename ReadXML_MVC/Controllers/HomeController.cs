using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;
using TechTest.Models;

namespace TechTest.Controllers
{
    public class HomeController : Controller
    {
        public readonly string _xmlPath = Path.Combine(Directory.GetCurrentDirectory(), "Data.xml");

        //Read XML Data
        public List<Items> GetItemsFromXML()
        {
            if (!System.IO.File.Exists(_xmlPath))
                return [];

            var doc = XDocument.Load(_xmlPath);
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            var result = doc.Descendants("entry")
                .Select(x =>
                    new Items
                    {
                        Title = (string)x.Attribute("title") ?? "",
                        Artist = (string)x.Attribute("artist") ?? "",
                        Category = (string)x.Attribute("category") ?? "",
                        TrackList = [.. x.Elements("track").Select(t => new Track
                        {
                            TrackOrder = (string)t.Attribute("order") ?? "",
                            TrackName = t.Value
                        })]
                    }).ToList();
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

            return result;
        }

        public IActionResult Index() => View();
        public IActionResult Search() => View();
        public IActionResult Details(string title) => View("Details", title);


        //API For Front End Functionalities
        [HttpGet("/api/home-info")]
        public IActionResult GetHomeInfo()
        {
            var result = GetItemsFromXML().ToList();
            return result != null ? Ok(result) : NotFound();
        }

        [HttpGet("/api/search-item")]
        public IActionResult SearchItem(string query)
        {
            var allItems = GetItemsFromXML();
            if (string.IsNullOrWhiteSpace(query))
                return Ok(allItems);

            // 1. Split query into individual words (e.g., "Pink Wall" -> ["Pink", "Wall"])
            var keywords = query.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            var result = allItems
                .Where(i =>
                    // 2. Ensure EVERY keyword matches SOMETHING in this album
                    keywords.All(word =>
                        i.Title.Contains(word, StringComparison.OrdinalIgnoreCase) ||
                        i.Artist.Contains(word, StringComparison.OrdinalIgnoreCase) ||
                        i.Category.Contains(word, StringComparison.OrdinalIgnoreCase) ||
                        i.TrackList.Any(t => t.TrackName.Contains(word, StringComparison.OrdinalIgnoreCase))
                    ))
                .Select(i =>
                {
                    // 3. Optional: Filter tracks to show only those that match ANY of the keywords
                    // If the user searched "Pink Wall", they probably want to see "The Wall" track.
                    var matchingTracks = i.TrackList
                        .Where(t => keywords.Any(word => t.TrackName.Contains(word, StringComparison.OrdinalIgnoreCase)))
                        .ToList();

                    return new
                    {
                        i.Title,
                        i.Artist,
                        i.Category,
                        // If keywords matched specific tracks, show only those. 
                        // Otherwise, show the whole album (the match was in the Title/Artist).
                        TrackList = matchingTracks.Any() ? matchingTracks : i.TrackList
                    };
                })
                .ToList();

            return result.Any() ? Ok(result) : NotFound(new { message = "No matches found." });
        }

        [HttpGet("/api/details/{title}")]
        public IActionResult GetDetails([FromRoute] string title)
        {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            var result = GetItemsFromXML()
                .FirstOrDefault(i => i.Title.Equals(title, StringComparison.OrdinalIgnoreCase));
#pragma warning restore CS8602 // Dereference of a possibly null reference.

            return result != null ? Ok(result) : NotFound();
        }

    }
}
