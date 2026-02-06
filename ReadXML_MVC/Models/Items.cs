namespace TechTest.Models
{
    public class Items
    {
        public string? Title { get; set; }
        public string? Artist { get; set; }
        public string? Category { get; set; }
        public List<Track>? TrackList { get; set; } = [];
    }

    public class Track
    {
        public string? TrackOrder{ get; set; }
        public string? TrackName { get; set; }
    }
}
