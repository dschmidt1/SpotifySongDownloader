using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using MediaToolkit;
using MediaToolkit.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using YoutubeExtractor;

namespace SpotifyDownloader
{
    public class YoutubeClient
    {
        private static YouTubeService _youtube;
        private static SpotifyClient _spotify;
        private List<SearchResult> _videos;

        public YoutubeClient()
        {
            _youtube = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = ConfigurationManager.AppSettings["YoutubeSecret"],
                ApplicationName = this.GetType().ToString()
            });

            _spotify = new SpotifyClient();
            _videos = new List<SearchResult>();
        }

        public void SearchVideos(string playlistName, string path)
        {
            if (_youtube == null) return;

            foreach(var track in _spotify.GetPlayListTracks())
            {
                var searchListRequest = _youtube.Search.List("snippet");
                searchListRequest.Q = track + " lyrics";
                searchListRequest.MaxResults = 10;

                var searchListResponse = searchListRequest.Execute();

                foreach (var searchResult in searchListResponse.Items)
                {
                    if (searchResult.Id.Kind == "youtube#video")
                    {
                        try
                        {
                            DownloadVideo(searchResult.Id.VideoId.ToString(), track, path);
                            break;
                        }
                            catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                        }
                    }
                }
            }

            Console.WriteLine(_videos.Count);
        }

        public void DownloadVideo(string id, string title, string path)
        {
            var link = "http://www.youtube.com/watch?v=" + id;
            IEnumerable<VideoInfo> videoInfos = DownloadUrlResolver.GetDownloadUrls(link);

            VideoInfo video = videoInfos
                .First(info => info.VideoType == VideoType.Mp4 && info.Resolution == 360);

            if (video.RequiresDecryption)
            {
                DownloadUrlResolver.DecryptDownloadUrl(video);
            }

            try
            {
                var save = Path.Combine(path, title);
                save.Replace(@"\\", @"\");

                var videoDownloader = new VideoDownloader(video, save);
                videoDownloader.Execute();

                Convert(title, path);
            }
            catch (Exception e)
            {
                Console.WriteLine(title + " " + e.Message);
            }
}

        private void Convert(string title, string path)
        {
            var save = Path.Combine(path, title);
            save.Replace(@"\\", @"\");

            var inputFile = new MediaFile { Filename = save };
            var outputFile = new MediaFile { Filename = save + ".mp3" };
            using (var engine = new Engine())
            {
                engine.Convert(inputFile, outputFile);
            }
            if (File.Exists(save))
            {
                File.Delete(save);
            }
        }
    }
}
