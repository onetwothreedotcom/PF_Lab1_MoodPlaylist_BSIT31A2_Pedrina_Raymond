using System.ComponentModel.DataAnnotations;
using MoodPlaylistGenerator.Models;

namespace MoodPlaylistGenerator.ViewModels
{
    public class CreateSongViewModel
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Artist { get; set; } = string.Empty;

        [Url]
        [Display(Name = "YouTube URL")]
        public string YouTubeUrl { get; set; } = string.Empty;

        [Display(Name = "Upload Media File")]
        public IFormFile? MediaFile { get; set; }

        [Display(Name = "Media Source")]
        public string MediaSource { get; set; } = "youtube"; // "youtube" or "upload"

        [Display(Name = "Moods")]
        public List<int> SelectedMoodIds { get; set; } = new();

        public List<Mood> AvailableMoods { get; set; } = new();
        public (string[] VideoExtensions, string[] AudioExtensions, long MaxSizeBytes)? UploadConstraints { get; set; }
    }

    public class EditSongViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Artist { get; set; } = string.Empty;

        [Url]
        [Display(Name = "YouTube URL")]
        public string YouTubeUrl { get; set; } = string.Empty;

        [Display(Name = "Replace Media File")]
        public IFormFile? MediaFile { get; set; }

        [Display(Name = "Media Source")]
        public string MediaSource { get; set; } = "youtube"; // "youtube" or "upload"

        [Display(Name = "Moods")]
        public List<int> SelectedMoodIds { get; set; } = new();

        public List<Mood> AvailableMoods { get; set; } = new();
        public (string[] VideoExtensions, string[] AudioExtensions, long MaxSizeBytes)? UploadConstraints { get; set; }
        
        // Properties to display current media info
        public bool HasLocalMedia { get; set; }
        public string CurrentMediaType { get; set; } = string.Empty;
        public string CurrentFileName { get; set; } = string.Empty;
    }

    public class SongListViewModel
    {
        public List<Song> Songs { get; set; } = new();
        public List<Mood> Moods { get; set; } = new();
        public int? SelectedMoodId { get; set; }
        public string SearchTerm { get; set; } = string.Empty;
    }

    public class SongDetailViewModel
    {
        public Song Song { get; set; } = null!;
        public string YouTubeVideoId { get; set; } = string.Empty;
        public List<Mood> AssignedMoods { get; set; } = new();
        public string MediaUrl { get; set; } = string.Empty;
        public bool IsLocalMediaMissing { get; set; }
        public bool IsUsingRickRollFallback { get; set; }
    }
}
