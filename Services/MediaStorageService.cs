using System.Security.Cryptography;
using System.Text;

namespace MoodPlaylistGenerator.Services
{
    public class MediaStorageService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IConfiguration _configuration;
        private readonly string _uploadDirectory;
        private readonly long _maxFileSizeBytes;
        private readonly string[] _allowedVideoExtensions;
        private readonly string[] _allowedAudioExtensions;
        private readonly string _rickRollVideoUrl;

        public MediaStorageService(IWebHostEnvironment webHostEnvironment, IConfiguration configuration)
        {
            _webHostEnvironment = webHostEnvironment;
            _configuration = configuration;
            
            _uploadDirectory = _configuration["MediaUpload:UploadDirectory"] ?? "wwwroot/uploads/media";
            _maxFileSizeBytes = _configuration.GetValue<long>("MediaUpload:MaxFileSizeBytes", 104857600); // 100MB default
            _allowedVideoExtensions = _configuration.GetSection("MediaUpload:AllowedVideoExtensions").Get<string[]>() ?? 
                new[] { ".mp4", ".avi", ".mov", ".wmv", ".webm" };
            _allowedAudioExtensions = _configuration.GetSection("MediaUpload:AllowedAudioExtensions").Get<string[]>() ?? 
                new[] { ".mp3", ".wav", ".ogg", ".m4a", ".aac" };
            _rickRollVideoUrl = _configuration["MediaUpload:RickRollVideoUrl"] ?? 
                "https://www.youtube.com/embed/dQw4w9WgXcQ?enablejsapi=1&modestbranding=1&rel=0";
        }

        public async Task<(bool Success, string? FilePath, string? FileName, string FileType, long FileSize, string? ErrorMessage)> SaveMediaFileAsync(IFormFile file, int userId)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return (false, null, null, "", 0, "No file provided");

                if (file.Length > _maxFileSizeBytes)
                    return (false, null, null, "", 0, $"File size exceeds maximum allowed size of {_maxFileSizeBytes / 1024 / 1024}MB");

                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                var fileType = GetFileType(extension);
                
                if (fileType == null)
                    return (false, null, null, "", 0, "File type not supported");

                if (!IsValidFileExtension(extension))
                    return (false, null, null, "", 0, "Invalid file extension");

                // Create upload directory if it doesn't exist
                var fullUploadPath = Path.Combine(_webHostEnvironment.ContentRootPath, _uploadDirectory);
                Directory.CreateDirectory(fullUploadPath);

                // Generate unique filename
                var uniqueFileName = GenerateUniqueFileName(file.FileName, userId);
                var filePath = Path.Combine(fullUploadPath, uniqueFileName);

                // Save file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Return relative path for database storage
                var relativePath = Path.Combine(_uploadDirectory, uniqueFileName).Replace("\\", "/");
                
                return (true, relativePath, uniqueFileName, fileType, file.Length, null);
            }
            catch (Exception ex)
            {
                return (false, null, null, "", 0, $"Error saving file: {ex.Message}");
            }
        }

        public bool DeleteMediaFile(string? filePath)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath))
                    return true;

                var fullPath = Path.Combine(_webHostEnvironment.ContentRootPath, filePath);
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    return true;
                }
                return true; // File doesn't exist, consider as success
            }
            catch
            {
                return false;
            }
        }

        public bool MediaFileExists(string? filePath)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath))
                    return false;

                var fullPath = Path.Combine(_webHostEnvironment.ContentRootPath, filePath);
                return File.Exists(fullPath);
            }
            catch
            {
                return false;
            }
        }

        public string GetMediaUrl(string? filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return "";

            // Convert to web URL format
            return "/" + filePath.Replace("wwwroot/", "").Replace("\\", "/");
        }

        public string GetRickRollVideoUrl()
        {
            return _rickRollVideoUrl;
        }

        private string? GetFileType(string extension)
        {
            if (_allowedVideoExtensions.Contains(extension))
                return "video";
            
            if (_allowedAudioExtensions.Contains(extension))
                return "audio";
            
            return null;
        }

        private bool IsValidFileExtension(string extension)
        {
            return _allowedVideoExtensions.Contains(extension) || _allowedAudioExtensions.Contains(extension);
        }

        private string GenerateUniqueFileName(string originalFileName, int userId)
        {
            var extension = Path.GetExtension(originalFileName);
            var nameWithoutExtension = Path.GetFileNameWithoutExtension(originalFileName);
            
            // Create hash from user ID and timestamp for uniqueness
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
            var hashInput = $"{userId}_{timestamp}_{nameWithoutExtension}";
            
            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(hashInput));
            var hash = Convert.ToHexString(hashBytes)[..8].ToLowerInvariant();
            
            return $"{userId}_{hash}_{nameWithoutExtension}{extension}";
        }

        public (string[] VideoExtensions, string[] AudioExtensions, long MaxSizeBytes) GetUploadConstraints()
        {
            return (_allowedVideoExtensions, _allowedAudioExtensions, _maxFileSizeBytes);
        }
    }
}
