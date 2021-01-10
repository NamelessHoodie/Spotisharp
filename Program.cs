﻿using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using VideoLibrary;

namespace SpotiSharp
{
    public static class Program
    {

        public static void Main(string[] args)
        {

            Console.WriteLine("  ██████  ██▓███   ▒█████  ▄▄▄█████▓ ██▓  ██████  ██░ ██  ▄▄▄       ██▀███   ██▓███  \n" +
                "▒██    ▒ ▓██░  ██▒▒██▒  ██▒▓  ██▒ ▓▒▓██▒▒██    ▒ ▓██░ ██▒▒████▄    ▓██ ▒ ██▒▓██░  ██▒\n" +
                "░ ▓██▄   ▓██░ ██▓▒▒██░  ██▒▒ ▓██░ ▒░▒██▒░ ▓██▄   ▒██▀▀██░▒██  ▀█▄  ▓██ ░▄█ ▒▓██░ ██▓▒\n" +
                "  ▒   ██▒▒██▄█▓▒ ▒▒██   ██░░ ▓██▓ ░ ░██░  ▒   ██▒░▓█ ░██ ░██▄▄▄▄██ ▒██▀▀█▄  ▒██▄█▓▒ ▒\n" +
                "▒██████▒▒▒██▒ ░  ░░ ████▓▒░  ▒██▒ ░ ░██░▒██████▒▒░▓█▒░██▓ ▓█   ▓██▒░██▓ ▒██▒▒██▒ ░  ░\n" +
                "▒ ▒▓▒ ▒ ░▒▓▒░ ░  ░░ ▒░▒░▒░   ▒ ░░   ░▓  ▒ ▒▓▒ ▒ ░ ▒ ░░▒░▒ ▒▒   ▓▒█░░ ▒▓ ░▒▓░▒▓▒░ ░  ░\n" +
                "░ ░▒  ░ ░░▒ ░       ░ ▒ ▒░     ░     ▒ ░░ ░▒  ░ ░ ▒ ░▒░ ░  ▒   ▒▒ ░  ░▒ ░ ▒░░▒ ░     \n" +
                "░  ░  ░  ░░       ░ ░ ░ ▒    ░       ▒ ░░  ░  ░   ░  ░░ ░  ░   ▒     ░░   ░ ░░       \n" +
                $"      ░               ░ ░            ░        ░   ░  ░  ░      ░  ░   ░       {Utilities.ApplicationVersion}\n");

            if (Utilities.IsRoot)
            {
                Console.WriteLine("SpotiSharp won't run with root permissions, exiting...");
                Environment.Exit(1);
            }

            Config.Initialize(); // Initialize configuration file

            if (!DependencyHelpers.IsFFmpegPresent())
            {
                Console.WriteLine("FFmpeg not found. Downloading...");
                string ffmpegZipPath = Path.Combine(Config.Properties.FFmpegPath, "ffmpeg.zip");
                new WebClient().DownloadFile(
                    DependencyHelpers.getFFmpegDownloadUrl(),
                    ffmpegZipPath
                );
                Utilities.UnZip(ffmpegZipPath, Config.Properties.FFmpegPath, true);
                File.Delete(ffmpegZipPath);
                if (Utilities.IsLinux) {
                    Console.WriteLine("This message is visible for linux users after downloading FFmpeg.\n" +
                        "Make sure you granted execution permissions for ffmpeg before running SpotiSharp again.\n" +
                        "SpotiSharp will now close.");
                    Environment.Exit(1);
                }
            }

            var (isNewVersionAvailable, Version) = Utilities.CheckForLatestApplicationVersion();

            if (isNewVersionAvailable)
                Console.WriteLine($"Out of date!: {Version}\n");

            if (args.Length == 0)
            {
                Console.WriteLine("SpotiSharp is a Open-Source CLI application made in .NET Core\n" +
                    "Usage: .\\SpotiSharp.exe \"Text | PlaylistUrl | AlbumUrl\"\n" +
                    "No arguments passed...");
                Environment.Exit(1);
            }

            string input = args[0];

            Console.WriteLine("Connecting To Spotify...");
            var client = SpotifyHelpers.ConnectToSpotify();
            var trackQueue = new ConcurrentQueue<TrackInfo>();
            var youTube = YouTube.Default;

            Console.WriteLine("Making requests to Spotify...");
            if (input.IsSpotifyUrl())
            {
                var (type, url) = input.GetSpotifyUrlId();
                switch (type)
                {
                    case UrlType.Playlist:
                        var taskPlaylist = client.QueueSpotifyTracksFromPlaylist(url, trackQueue);
                        while (!taskPlaylist.IsCompleted)
                        {
                            while (trackQueue.TryDequeue(out var info))
                            {
                                Console.WriteLine($"Downloading ::::: {info.Artist} - {info.Title} | Queue: {trackQueue.Count}");
                                youTube.DownloadAndConvertTrack(info);
                                Console.WriteLine($"Done        ::::: {info.Artist} - {info.Title}");
                            }
                            Thread.Sleep(200);
                        }
                        while (trackQueue.TryDequeue(out var info))
                        {
                            Console.WriteLine($"Downloading ::::: {info.Artist} - {info.Title} | Queue: {trackQueue.Count}");
                            youTube.DownloadAndConvertTrack(info);
                            Console.WriteLine($"Done        ::::: {info.Artist} - {info.Title}");
                        }
                        break;
                    case UrlType.Album:
                        var taskAlbum = client.QueueSpotifyTracksFromAlbum(url, trackQueue);
                        while (!taskAlbum.IsCompleted)
                        {
                            while (trackQueue.TryDequeue(out var info))
                            {
                                Console.WriteLine($"Downloading ::::: {info.Artist} - {info.Title} | Queue: {trackQueue.Count}");
                                youTube.DownloadAndConvertTrack(info);
                                Console.WriteLine($"Done        ::::: {info.Artist} - {info.Title}");
                            }
                            Thread.Sleep(200);
                        }
                        while (trackQueue.TryDequeue(out var info))
                        {
                            Console.WriteLine($"Downloading ::::: {info.Artist} - {info.Title} | Queue: {trackQueue.Count}");
                            youTube.DownloadAndConvertTrack(info);
                            Console.WriteLine($"Done        ::::: {info.Artist} - {info.Title}");
                        }
                        break;
                    case UrlType.Track:
                        var track = client.GetSpotifyTrack(input).GetAwaiter().GetResult();
                        if (track == null)
                            Environment.Exit(1);
                        Console.WriteLine($"Downloading ::::: {track.Artist} - {track.Title}");
                        youTube.DownloadAndConvertTrack(track);
                        Console.WriteLine($"Done        ::::: {track.Artist} - {track.Title}");
                        break;
                }
            }
            else
            {
                var track = client.GetSpotifyTrack(input).GetAwaiter().GetResult();
                if (track == null)
                    Environment.Exit(1);
                Console.WriteLine($"Downloading ::::: {track.Artist} - {track.Title}");
                youTube.DownloadAndConvertTrack(track);
                Console.WriteLine($"Done        ::::: {track.Artist} - {track.Title}");
            }
        }    
    }
}
