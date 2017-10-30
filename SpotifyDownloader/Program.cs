using System;
using System.Configuration;
using SpotifyAPI.Web; //Base Namespace
using SpotifyAPI.Web.Auth; //All Authentication-related classes
using SpotifyAPI.Web.Enums; //Enums
using SpotifyAPI.Web.Models; //Models for the JSON-responses
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Upload;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;

namespace SpotifyDownloader
{
    class Program
    {
        private static YoutubeClient _youtube;

        static void Main(string[] args)
        {
            _youtube = new YoutubeClient();

            Console.WriteLine("Please enter a playlist name");
            var name = Console.ReadLine();
            Console.WriteLine("Please enter a path");
            var path = Console.ReadLine();


            if (name != null)
                _youtube.SearchVideos(name, path);
            Console.ReadKey();
        }
    }
}
