using SpotifyAPI.Web; //Base Namespace
using SpotifyAPI.Web.Auth; //All Authentication-related classes
using SpotifyAPI.Web.Enums; //Enums
using SpotifyAPI.Web.Models; //Models for the JSON-responses
using System;
using System.Configuration;

namespace SpotifyDownloader
{
    public class SpotifyAPIWrapper
    {
        private static SpotifyWebAPI _spotify;
        private static PrivateProfile _user;

        public SpotifyAPIWrapper()
        {
            Init();
        }

        public async void Init()
        {
            WebAPIFactory webApiFactory = new WebAPIFactory(
                 "http://localhost",
                 8000,
                 ConfigurationManager.AppSettings["SpotifyClientID"],
                 Scope.UserReadPrivate,
                 TimeSpan.FromSeconds(20)
            );
            Console.WriteLine(ConfigurationManager.AppSettings["SpotifyClientID"]);

            try
            {
                //This will open the user's browser and returns once
                //the user is authorized.
                _spotify = await webApiFactory.GetWebApi();
                _user = await _spotify.GetPrivateProfileAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            if (_spotify == null)
                return;
        }

        public async void PrintPlaylistTracks(string playListName = "")
        {
            //if (_spotify == null || _user == null) return null;

            if(playListName == "")
            {
                Paging<SimplePlaylist> userPlaylists = await _spotify.GetUserPlaylistsAsync(_user.Id);
                foreach(var playlist in userPlaylists.Items)
                {
                    Paging<PlaylistTrack> tracks = await _spotify.GetPlaylistTracksAsync(_user.Id, playlist.Id);
                    while (true)
                    {
                        tracks.Items.ForEach(track => Console.WriteLine(track.Track.Name));

                        if (!tracks.HasNextPage())
                            break;
                        tracks = await _spotify.GetNextPageAsync(tracks);
                    }
                }
            }
        }
    }
}
