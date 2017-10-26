using SpotifyAPI.Web; //Base Namespace
using SpotifyAPI.Web.Auth; //All Authentication-related classes
using SpotifyAPI.Web.Enums; //Enums
using SpotifyAPI.Web.Models; //Models for the JSON-responses
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace SpotifyDownloader
{
    public class SpotifyClient
    {
        private static SpotifyWebAPI _spotify;
        private static PrivateProfile _user;
        private List<string> tracksToDownload;

        public SpotifyClient()
        {
            Init();
        }

        public async void Init()
        {
            var webApiFactory = new WebAPIFactory(
                  "http://localhost",
                  8000,
                  ConfigurationManager.AppSettings["SpotifyClientID"],
                  Scope.UserReadPrivate,
                  TimeSpan.FromSeconds(20)
             );

            try
            {
                //This will open the user's browser and returns once
                //the user is authorized.
                _spotify = await webApiFactory.GetWebApi();
                _user = _spotify.GetPrivateProfile();
                tracksToDownload = new List<string>();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            if (_spotify == null)
                return;
        }

        public List<string> GetPlayListTracks(string playListName = "")
        {
            if (_spotify == null || _user == null) return null;

            if (playListName == "")
            {
                Paging<SimplePlaylist> userPlaylists = _spotify.GetUserPlaylists(_user.Id);
                foreach (var playlist in userPlaylists.Items)
                {
                    Paging<PlaylistTrack> tracks = _spotify.GetPlaylistTracks(_user.Id, playlist.Id);
                    while (true)
                    {
                        foreach(var track in tracks.Items)
                            tracksToDownload.Add(track.Track.Name + " - " + track.Track.Artists[0].Name);

                        if (!tracks.HasNextPage())
                            break;

                        tracks = _spotify.GetNextPage(tracks);
                    }
                }
            } else
            {
                Paging<SimplePlaylist> userPlaylists = _spotify.GetUserPlaylists(_user.Id);
                var playlist = userPlaylists.Items.Where(w => w.Name == playListName).FirstOrDefault();

                if (playlist == null) Console.WriteLine("Wrong playlist name!");

                Paging<PlaylistTrack> tracks = _spotify.GetPlaylistTracks(_user.Id, playlist.Id);
                while (true)
                {
                    foreach (var track in tracks.Items)
                        tracksToDownload.Add(track.Track.Name + " - " + track.Track.Artists[0].Name);

                    if (!tracks.HasNextPage())
                        break;

                    tracks = _spotify.GetNextPage(tracks);
                }
            }

            tracksToDownload = tracksToDownload.Distinct().ToList();
            return tracksToDownload;
        }
    }
}
