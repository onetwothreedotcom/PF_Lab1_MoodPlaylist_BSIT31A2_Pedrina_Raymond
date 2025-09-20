using Microsoft.EntityFrameworkCore;
using MoodPlaylistGenerator.Data;
using MoodPlaylistGenerator.Models;

namespace MoodPlaylistGenerator.Services
{
    public class SongService
    {
        private readonly ApplicationDbContext _context;
        private readonly MediaStorageService _mediaStorageService;

        public SongService(ApplicationDbContext context, MediaStorageService mediaStorageService)
        {
            _context = context;
            _mediaStorageService = mediaStorageService;
        }

        public async Task<List<Song>> GetUserSongsAsync(int userId)
        {
            return await _context.Songs
                .Include(s => s.SongMoods)
                .ThenInclude(sm => sm.Mood)
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
        }

        public async Task<Song?> GetSongByIdAsync(int songId, int userId)
        {
            return await _context.Songs
                .Include(s => s.SongMoods)
                .ThenInclude(sm => sm.Mood)
                .FirstOrDefaultAsync(s => s.Id == songId && s.UserId == userId);
        }

        public async Task<Song> CreateSongAsync(string title, string artist, string youtubeUrl, int userId, List<int> moodIds)
        {
            var song = new Song
            {
                Title = title,
                Artist = artist,
                YouTubeUrl = youtubeUrl,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Songs.Add(song);
            await _context.SaveChangesAsync();

            // Add mood associations
            if (moodIds.Any())
            {
                foreach (var moodId in moodIds)
                {
                    _context.SongMoods.Add(new SongMood
                    {
                        SongId = song.Id,
                        MoodId = moodId
                    });
                }
                await _context.SaveChangesAsync();
            }

            return await GetSongByIdAsync(song.Id, userId) ?? song;
        }

        public async Task<Song?> UpdateSongAsync(int songId, int userId, string title, string artist, string youtubeUrl, List<int> moodIds)
        {
            var song = await _context.Songs
                .Include(s => s.SongMoods)
                .FirstOrDefaultAsync(s => s.Id == songId && s.UserId == userId);

            if (song == null)
                return null;

            // Update song properties
            song.Title = title;
            song.Artist = artist;
            song.YouTubeUrl = youtubeUrl;

            // Remove existing mood associations
            _context.SongMoods.RemoveRange(song.SongMoods);

            // Add new mood associations
            foreach (var moodId in moodIds)
            {
                song.SongMoods.Add(new SongMood
                {
                    SongId = song.Id,
                    MoodId = moodId
                });
            }

            await _context.SaveChangesAsync();
            return await GetSongByIdAsync(songId, userId);
        }

        public async Task<Song> CreateSongWithLocalMediaAsync(string title, string artist, IFormFile mediaFile, int userId, List<int> moodIds)
        {
            var uploadResult = await _mediaStorageService.SaveMediaFileAsync(mediaFile, userId);
            
            if (!uploadResult.Success)
                throw new InvalidOperationException(uploadResult.ErrorMessage ?? "Failed to upload media file");

            var song = new Song
            {
                Title = title,
                Artist = artist,
                YouTubeUrl = string.Empty, // Empty for local media
                LocalFilePath = uploadResult.FilePath,
                LocalFileName = uploadResult.FileName,
                LocalFileType = uploadResult.FileType,
                LocalFileSizeBytes = uploadResult.FileSize,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Songs.Add(song);
            await _context.SaveChangesAsync();

            // Add mood associations
            if (moodIds.Any())
            {
                foreach (var moodId in moodIds)
                {
                    _context.SongMoods.Add(new SongMood
                    {
                        SongId = song.Id,
                        MoodId = moodId
                    });
                }
                await _context.SaveChangesAsync();
            }

            return await GetSongByIdAsync(song.Id, userId) ?? song;
        }

        public async Task<Song?> UpdateSongWithLocalMediaAsync(int songId, int userId, string title, string artist, IFormFile? mediaFile, List<int> moodIds)
        {
            var song = await _context.Songs
                .Include(s => s.SongMoods)
                .FirstOrDefaultAsync(s => s.Id == songId && s.UserId == userId);

            if (song == null)
                return null;

            // Update basic properties
            song.Title = title;
            song.Artist = artist;

            // If new media file is provided, replace the existing one
            if (mediaFile != null && mediaFile.Length > 0)
            {
                // Delete old file if it exists
                if (!string.IsNullOrEmpty(song.LocalFilePath))
                {
                    _mediaStorageService.DeleteMediaFile(song.LocalFilePath);
                }

                var uploadResult = await _mediaStorageService.SaveMediaFileAsync(mediaFile, userId);
                
                if (!uploadResult.Success)
                    throw new InvalidOperationException(uploadResult.ErrorMessage ?? "Failed to upload media file");

                song.LocalFilePath = uploadResult.FilePath;
                song.LocalFileName = uploadResult.FileName;
                song.LocalFileType = uploadResult.FileType;
                song.LocalFileSizeBytes = uploadResult.FileSize;
                song.YouTubeUrl = string.Empty; // Clear YouTube URL when uploading local media
            }

            // Remove existing mood associations
            _context.SongMoods.RemoveRange(song.SongMoods);

            // Add new mood associations
            foreach (var moodId in moodIds)
            {
                song.SongMoods.Add(new SongMood
                {
                    SongId = song.Id,
                    MoodId = moodId
                });
            }

            await _context.SaveChangesAsync();
            return await GetSongByIdAsync(songId, userId);
        }

        public async Task<bool> DeleteSongAsync(int songId, int userId)
        {
            var song = await _context.Songs
                .FirstOrDefaultAsync(s => s.Id == songId && s.UserId == userId);

            if (song == null)
                return false;

            // Delete associated media file if it exists
            if (!string.IsNullOrEmpty(song.LocalFilePath))
            {
                _mediaStorageService.DeleteMediaFile(song.LocalFilePath);
            }

            _context.Songs.Remove(song);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Mood>> GetAllMoodsAsync()
        {
            return await _context.Moods.OrderBy(m => m.Name).ToListAsync();
        }

        public async Task<List<Song>> GetSongsByMoodAsync(int moodId, int userId)
        {
            return await _context.Songs
                .Include(s => s.SongMoods)
                .ThenInclude(sm => sm.Mood)
                .Where(s => s.UserId == userId && s.SongMoods.Any(sm => sm.MoodId == moodId))
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
        }

        public string ExtractYouTubeVideoId(string url)
        {
            try
            {
                // Extract video ID from various YouTube URL formats
                var uri = new Uri(url);
                
                if (uri.Host.Contains("youtu.be"))
                {
                    return uri.AbsolutePath.TrimStart('/');
                }
                
                if (uri.Host.Contains("youtube.com"))
                {
                    // Parse query string manually for .NET 9.0 compatibility
                    var queryString = uri.Query.TrimStart('?');
                    var queryParams = queryString.Split('&')
                        .Select(param => param.Split('='))
                        .Where(pair => pair.Length == 2)
                        .ToDictionary(pair => pair[0], pair => Uri.UnescapeDataString(pair[1]));
                    
                    return queryParams.TryGetValue("v", out var videoId) ? videoId : "";
                }

                return "";
            }
            catch
            {
                return "";
            }
        }

        public string GetMediaUrl(Song song)
        {
            if (song.IsLocalMedia)
            {
                // Check if file exists, if not return Rick Roll URL
                if (!_mediaStorageService.MediaFileExists(song.LocalFilePath))
                {
                    return _mediaStorageService.GetRickRollVideoUrl();
                }
                return _mediaStorageService.GetMediaUrl(song.LocalFilePath);
            }
            
            return song.YouTubeUrl;
        }

        public bool IsLocalMediaMissing(Song song)
        {
            return song.IsLocalMedia && !_mediaStorageService.MediaFileExists(song.LocalFilePath);
        }

        public (string[] VideoExtensions, string[] AudioExtensions, long MaxSizeBytes) GetUploadConstraints()
        {
            return _mediaStorageService.GetUploadConstraints();
        }
    }
}
