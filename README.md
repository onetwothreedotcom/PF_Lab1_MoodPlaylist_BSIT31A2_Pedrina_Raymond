# 🎵 Mood Playlist Generator

A .NET Core MVC application that helps you organize your favorite YouTube songs by mood and generate randomized playlists.

## Features

### 🔐 **Authentication**
- User registration and login
- Secure password hashing with BCrypt
- Password reset functionality
- Session-based authentication

### 🎵 **Song Management**
- Add YouTube songs with title and artist
- Multi-mood tagging (Happy, Sad, Relaxed, Energetic, Romantic, Focus)
- Search and filter songs
- Edit and delete your songs

### 🎧 **Playlist Generation**
- Generate random playlists based on mood
- Customizable playlist size (1-50 songs)
- Custom playlist naming
- View and manage generated playlists

### 📊 **Dashboard**
- Overview of your music library
- Recent songs and playlists
- Mood-based statistics
- Quick access to main features

## Technology Stack

- **Backend**: ASP.NET Core MVC (.NET 9.0)
- **Database**: SQLite with Entity Framework Core
- **Authentication**: Cookie-based authentication
- **Password Security**: BCrypt.Net
- **Frontend**: Bootstrap 5, Razor Views
- **Architecture**: MVC with Service Layer pattern

## Database Schema

### Core Models
- **User**: Authentication and profile data
- **Song**: YouTube URL, title, artist, user association
- **Mood**: Predefined mood categories with colors
- **Playlist**: Generated playlists with metadata
- **SongMood**: Many-to-many relationship for song mood tagging
- **PlaylistSong**: Many-to-many relationship for playlist composition

### Seed Data
6 pre-configured moods:
- 😊 Happy (Gold)
- 😢 Sad (Royal Blue)
- 😌 Relaxed (Light Green)
- ⚡ Energetic (Tomato)
- 💕 Romantic (Hot Pink)
- 🎯 Focus (Medium Slate Blue)

## Getting Started

### Prerequisites
- .NET 9.0 SDK
- Visual Studio 2022 or VS Code

### Installation

1. Clone the repository
```bash
git clone https://github.com/despicable-Nin/PF_Lab1.git
cd MoodPlaylistGenerator
```

2. Restore packages
```bash
dotnet restore
```

3. Apply database migrations
```bash
dotnet ef database update
```

4. Run the application
```bash
dotnet run
```

5. Open your browser to `https://localhost:5001` or `http://localhost:5000`

### Usage

1. **Register/Login**: Create an account or sign in
2. **Add Songs**: Add your favorite YouTube songs and tag them with moods
3. **Generate Playlists**: Select a mood and generate a randomized playlist
4. **Manage**: View, edit, and delete your songs and playlists

## Project Structure

```
MoodPlaylistGenerator/
├── Controllers/           # MVC Controllers
│   ├── AccountController.cs
│   ├── HomeController.cs
│   ├── PlaylistsController.cs
│   ├── SongsController.cs
│   └── TestController.cs
├── Data/                 # Database Context
│   └── ApplicationDbContext.cs
├── Models/               # Domain Models
│   ├── Mood.cs
│   ├── Playlist.cs
│   ├── Song.cs
│   └── User.cs
├── Services/             # Business Logic
│   ├── AuthService.cs
│   ├── PlaylistService.cs
│   └── SongService.cs
├── ViewModels/           # View Models
├── Views/                # Razor Views
├── wwwroot/              # Static files
└── Migrations/           # EF Core Migrations
```

## API Endpoints (Testing)

For backend testing, use these endpoints:
- `GET /api/test/status` - Check if backend is running
- `GET /api/test/moods` - Get all available moods
- `POST /api/test/test-user` - Create test user
- `POST /api/test/test-song` - Create test song
- `POST /api/test/test-playlist` - Generate test playlist
- `GET /api/test/database-info` - Database statistics

## Development Notes

- Built with 3-hour development constraint in mind
- Follows MVC pattern with service layer separation
- Uses Entity Framework Core with SQLite for lightweight deployment
- Responsive Bootstrap UI
- User data isolation (all operations are user-specific)

## Future Enhancements

- YouTube API integration for automatic metadata
- Spotify integration
- Social features (sharing playlists)
- Advanced mood analytics
- Mobile app companion

## License

This project is for educational/lab purposes.
