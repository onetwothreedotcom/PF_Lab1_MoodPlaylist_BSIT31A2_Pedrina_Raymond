using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using MoodPlaylistGenerator.Services;
using MoodPlaylistGenerator.ViewModels;
using MoodPlaylistGenerator.Models;

namespace MoodPlaylistGenerator.Controllers
{
    [Authorize]
    public class SongsController : Controller
    {
        private readonly SongService _songService;

        public SongsController(SongService songService)
        {
            _songService = songService;
        }

        private int GetCurrentUserId()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        }

        public async Task<IActionResult> Index(int? moodId, string? search)
        {
            var userId = GetCurrentUserId();
            var songs = await _songService.GetUserSongsAsync(userId);
            var moods = await _songService.GetAllMoodsAsync();

            // Filter by mood if selected
            if (moodId.HasValue)
            {
                songs = await _songService.GetSongsByMoodAsync(moodId.Value, userId);
            }

            // Filter by search term
            if (!string.IsNullOrEmpty(search))
            {
                songs = songs.Where(s => 
                    s.Title.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    s.Artist.Contains(search, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            var viewModel = new SongListViewModel
            {
                Songs = songs,
                Moods = moods,
                SelectedMoodId = moodId,
                SearchTerm = search ?? ""
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Details(int id)
        {
            var userId = GetCurrentUserId();
            var song = await _songService.GetSongByIdAsync(id, userId);
            
            if (song == null)
                return NotFound();

            var isLocalMediaMissing = _songService.IsLocalMediaMissing(song);
            var mediaUrl = _songService.GetMediaUrl(song);
            
            var viewModel = new SongDetailViewModel
            {
                Song = song,
                YouTubeVideoId = song.IsLocalMedia ? "" : _songService.ExtractYouTubeVideoId(song.YouTubeUrl),
                AssignedMoods = song.SongMoods.Select(sm => sm.Mood).ToList(),
                MediaUrl = mediaUrl,
                IsLocalMediaMissing = isLocalMediaMissing,
                IsUsingRickRollFallback = isLocalMediaMissing
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var viewModel = new CreateSongViewModel
            {
                AvailableMoods = await _songService.GetAllMoodsAsync(),
                UploadConstraints = _songService.GetUploadConstraints()
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateSongViewModel model)
        {
            // Validate based on media source
            if (model.MediaSource == "upload" && (model.MediaFile == null || model.MediaFile.Length == 0))
            {
                ModelState.AddModelError(nameof(model.MediaFile), "Please select a media file to upload.");
            }
            else if (model.MediaSource == "youtube" && string.IsNullOrEmpty(model.YouTubeUrl))
            {
                ModelState.AddModelError(nameof(model.YouTubeUrl), "Please provide a YouTube URL.");
            }

            if (!ModelState.IsValid)
            {
                model.AvailableMoods = await _songService.GetAllMoodsAsync();
                model.UploadConstraints = _songService.GetUploadConstraints();
                return View(model);
            }

            var userId = GetCurrentUserId();
            
            try
            {
                if (model.MediaSource == "upload" && model.MediaFile != null)
                {
                    await _songService.CreateSongWithLocalMediaAsync(
                        model.Title, 
                        model.Artist, 
                        model.MediaFile, 
                        userId, 
                        model.SelectedMoodIds);
                }
                else
                {
                    await _songService.CreateSongAsync(
                        model.Title, 
                        model.Artist, 
                        model.YouTubeUrl, 
                        userId, 
                        model.SelectedMoodIds);
                }

                TempData["SuccessMessage"] = "Song added successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                model.AvailableMoods = await _songService.GetAllMoodsAsync();
                model.UploadConstraints = _songService.GetUploadConstraints();
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = GetCurrentUserId();
            var song = await _songService.GetSongByIdAsync(id, userId);
            
            if (song == null)
                return NotFound();

            var viewModel = new EditSongViewModel
            {
                Id = song.Id,
                Title = song.Title,
                Artist = song.Artist,
                YouTubeUrl = song.YouTubeUrl,
                MediaSource = song.IsLocalMedia ? "upload" : "youtube",
                SelectedMoodIds = song.SongMoods.Select(sm => sm.MoodId).ToList(),
                AvailableMoods = await _songService.GetAllMoodsAsync(),
                UploadConstraints = _songService.GetUploadConstraints(),
                HasLocalMedia = song.IsLocalMedia,
                CurrentMediaType = song.LocalFileType ?? "",
                CurrentFileName = song.LocalFileName ?? ""
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditSongViewModel model)
        {
            // Validate based on media source and whether a file is uploaded
            if (model.MediaSource == "upload" && model.MediaFile != null && model.MediaFile.Length > 0)
            {
                // User is uploading a new file - this is valid
            }
            else if (model.MediaSource == "youtube" && string.IsNullOrEmpty(model.YouTubeUrl))
            {
                ModelState.AddModelError(nameof(model.YouTubeUrl), "Please provide a YouTube URL.");
            }
            else if (model.MediaSource == "upload" && !model.HasLocalMedia && (model.MediaFile == null || model.MediaFile.Length == 0))
            {
                ModelState.AddModelError(nameof(model.MediaFile), "Please select a media file to upload.");
            }

            if (!ModelState.IsValid)
            {
                model.AvailableMoods = await _songService.GetAllMoodsAsync();
                model.UploadConstraints = _songService.GetUploadConstraints();
                return View(model);
            }

            var userId = GetCurrentUserId();
            
            try
            {
                Song? updatedSong;
                
                if (model.MediaSource == "upload")
                {
                    // Handle local media update
                    updatedSong = await _songService.UpdateSongWithLocalMediaAsync(
                        model.Id,
                        userId,
                        model.Title,
                        model.Artist,
                        model.MediaFile,
                        model.SelectedMoodIds);
                }
                else
                {
                    // Handle YouTube URL update (this may clear local media if switching from local to YouTube)
                    updatedSong = await _songService.UpdateSongAsync(
                        model.Id, 
                        userId, 
                        model.Title, 
                        model.Artist, 
                        model.YouTubeUrl, 
                        model.SelectedMoodIds);
                }

                if (updatedSong == null)
                    return NotFound();

                TempData["SuccessMessage"] = "Song updated successfully!";
                return RedirectToAction(nameof(Details), new { id = model.Id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                model.AvailableMoods = await _songService.GetAllMoodsAsync();
                model.UploadConstraints = _songService.GetUploadConstraints();
                return View(model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetCurrentUserId();
            var success = await _songService.DeleteSongAsync(id, userId);
            
            if (!success)
                return NotFound();

            TempData["SuccessMessage"] = "Song deleted successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}
