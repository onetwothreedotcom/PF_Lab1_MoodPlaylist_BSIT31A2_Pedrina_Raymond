using System.ComponentModel.DataAnnotations;

namespace MoodPlaylistGenerator.Models
{
    public class Song
    {
        public int Id { get; set; }
        
        [Required]
        public string Title { get; set; } = string.Empty;
        
        [Required]
        public string Artist { get; set; } = string.Empty;
        
        [Url]
        public string YouTubeUrl { get; set; } = string.Empty;
        
        public string? LocalFilePath { get; set; }
        public string? LocalFileType { get; set; } // "audio" or "video"
        public string? LocalFileName { get; set; }
        public long? LocalFileSizeBytes { get; set; }
        public bool IsLocalMedia => !string.IsNullOrEmpty(LocalFilePath);
        
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public User User { get; set; } = null!;
        public List<SongMood> SongMoods { get; set; } = new();
        public List<PlaylistSong> PlaylistSongs { get; set; } = new();
    }
}
