#region Copyright, Author Details and Related Context
//<notice lastUpdateOn="5/8/2013">
//  <assembly>YouFetch</assembly>
//  <description>A Simple YouTube Video Downloader</description>
//  <copyright>
//    Copyright (C) 2013 Louis S. Berman

//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.

//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.

//    You should have received a copy of the GNU General Public License
//    along with this program.  If not, see http://www.gnu.org/licenses/.
//  </copyright>
//  <author>
//    <fullName>Louis S. Berman</fullName>
//    <email>louis@squideyes.com</email>
//    <website>http://squideyes.com</website>
//  </author>
//</notice>
#endregion 
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace YouFetch
{
    public static class YouTubeAbouter
    {
        public async static Task<List<VideoInfo>> GetVideoInfos(string videoId)
        {
            const string SHORTURL = "http://www.youtube.com/watch?v=";

            var videoInfos = new List<VideoInfo>();

            var shortUrl = SHORTURL + videoId;

            var html = await GetStringAsync(new Uri(shortUrl));

            var title = GetTitle(html);

            foreach (var url in ExtractUrls(html))
            {
                var videoInfo = new VideoInfo();

                videoInfo.VideoId = videoId;
                videoInfo.Title = title;

                videoInfo.VideoUri = new Uri(url + "&title=" + title);

                if (!await Task<bool>.Factory.StartNew(() => videoInfo.GetSize()))
                    continue;

                videoInfo.Seconds = long.Parse(
                    Regex.Match(html, "\"length_seconds\":(.+?),",
                    RegexOptions.Singleline).Groups[1].ToString());

                if (videoInfo.SetExtensionAndSize(html.IsWideScreen()))
                {
                    if (!videoInfo.Extension.StartsWith("itag-"))
                        videoInfos.Add(videoInfo);
                }
            }

            videoInfos.Sort();

            return videoInfos;
        }

        private static async Task<string> GetStringAsync(Uri uri)
        {
            string result;

            using (var client = new HttpClient())
                result = await new HttpClient().GetStringAsync(uri).ConfigureAwait(false);

            return result;
        }

        private static string GetTitle(string rss)
        {
            var title = rss.TextBetween("'VIDEO_TITLE': '", "'", 0);

            if (title == "")
                title = rss.TextBetween("\"title\" content=\"", "\"", 0);

            if (title == "")
                title = rss.TextBetween("&title=", "&", 0);

            title = title.Replace(@"\", "").Replace("'", "&#39;").
                Replace("\"", "&quot;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("+", " ");

            return title;
        }

        private static List<string> ExtractUrls(string html)
        {
            const string START = "\"url_encoded_fmt_stream_map\":\\s+\"(.+?)&";

            var urls = new List<string>();

            var group = Uri.UnescapeDataString(Regex.Match(html, START,
                RegexOptions.Singleline).Groups[1].ToString());

            var firstPattern = group.Substring(0, group.IndexOf('=') + 1);

            var matches = Regex.Split(group, firstPattern);

            for (int i = 0; i < matches.Length; i++)
                matches[i] = firstPattern + matches[i];

            foreach (var match in matches)
            {
                if (!match.Contains("url="))
                    continue;

                var url = match.TextBetween("url=", "\\u0026", 0);

                if (url == "")
                    url = match.TextBetween("url=", ",url", 0);

                if (url == "")
                    url = match.TextBetween("url=", "\",", 0);

                var sig = match.TextBetween("sig=", "\\u0026", 0);

                if (sig == "")
                    sig = match.TextBetween("sig=", ",sig", 0);

                if (sig == "")
                    sig = match.TextBetween("sig=", "\",", 0);

                while ((url.EndsWith(",")) || (url.EndsWith(".")) || (url.EndsWith("\"")))
                    url = url.Remove(url.Length - 1, 1);

                while ((sig.EndsWith(",")) || (sig.EndsWith(".")) || (sig.EndsWith("\"")))
                    sig = sig.Remove(sig.Length - 1, 1);

                if (string.IsNullOrEmpty(url))
                    continue;

                if (!string.IsNullOrEmpty(sig))
                    url += "&signature=" + sig;

                urls.Add(url);
            }

            return urls;
        }

        private static bool SetExtensionAndSize(this VideoInfo videoInfo, bool isWideScreen)
        {
            int value;

            string tag = Regex.Match(videoInfo.VideoUri.AbsoluteUri,
                @"itag=([1-9]?[0-9]?[0-9])", RegexOptions.Singleline).Groups[1].ToString();

            if (tag != "")
            {
                if (!int.TryParse(tag, out value))
                    value = 0;

                switch (value)
                {
                    case 5:
                        videoInfo.SetExtensionAndSize("flv",
                            new Size(320, (isWideScreen ? 180 : 240)));
                        break;
                    case 6:
                        videoInfo.SetExtensionAndSize("flv",
                            new Size(480, (isWideScreen ? 270 : 360)));
                        break;
                    case 17:
                        videoInfo.SetExtensionAndSize("3gp",
                            new Size(176, (isWideScreen ? 99 : 144)));
                        break;
                    case 18:
                        videoInfo.SetExtensionAndSize("mp4",
                            new Size(640, (isWideScreen ? 360 : 480)));
                        break;
                    case 22:
                        videoInfo.SetExtensionAndSize("mp4",
                            new Size(1280, (isWideScreen ? 720 : 960)));
                        break;
                    case 34:
                        videoInfo.SetExtensionAndSize("flv",
                            new Size(640, (isWideScreen ? 360 : 480)));
                        break;
                    case 35:
                        videoInfo.SetExtensionAndSize("flv",
                            new Size(854, (isWideScreen ? 480 : 640)));
                        break;
                    case 36:
                        videoInfo.SetExtensionAndSize("3gp",
                            new Size(320, (isWideScreen ? 180 : 240)));
                        break;
                    case 37:
                        videoInfo.SetExtensionAndSize("mp4",
                            new Size(1920, (isWideScreen ? 1080 : 1440)));
                        break;
                    case 38:
                        videoInfo.SetExtensionAndSize("mp4",
                            new Size(2048, (isWideScreen ? 1152 : 1536)));
                        break;
                    case 43:
                        videoInfo.SetExtensionAndSize("webm",
                            new Size(640, (isWideScreen ? 360 : 480)));
                        break;
                    case 44:
                        videoInfo.SetExtensionAndSize("webm",
                            new Size(854, (isWideScreen ? 480 : 640)));
                        break;
                    case 45:
                        videoInfo.SetExtensionAndSize("webm",
                            new Size(1280, (isWideScreen ? 720 : 960)));
                        break;
                    case 46:
                        videoInfo.SetExtensionAndSize("webm",
                            new Size(1920, (isWideScreen ? 1080 : 1440)));
                        break;
                    case 82:
                        videoInfo.SetExtensionAndSize("3D.mp4",
                            new Size(480, (isWideScreen ? 270 : 360)));
                        break;
                    case 83:
                        videoInfo.SetExtensionAndSize("3D.mp4",
                            new Size(640, (isWideScreen ? 360 : 480)));
                        break;
                    case 84:
                        videoInfo.SetExtensionAndSize("3D.mp4",
                            new Size(1280, (isWideScreen ? 720 : 960)));
                        break;
                    case 85:
                        videoInfo.SetExtensionAndSize("3D.mp4",
                            new Size(1920, (isWideScreen ? 1080 : 1440)));
                        break;
                    case 100:
                        videoInfo.SetExtensionAndSize("3D.webm",
                            new Size(640, (isWideScreen ? 360 : 480)));
                        break;
                    case 101:
                        videoInfo.SetExtensionAndSize("3D.webm",
                            new Size(640, (isWideScreen ? 360 : 480)));
                        break;
                    case 102:
                        videoInfo.SetExtensionAndSize("3D.webm",
                            new Size(1280, (isWideScreen ? 720 : 960)));
                        break;
                    case 120:
                        videoInfo.SetExtensionAndSize("live.flv",
                            new Size(1280, (isWideScreen ? 720 : 960)));
                        break;
                    default:
                        videoInfo.SetExtensionAndSize("itag-" + tag, new Size(0, 0));
                        break;
                }

                return true;
            }

            return false;
        }

        private static bool IsWideScreen(this string html)
        {
            var match = Regex.Match(html, @"'IS_WIDESCREEN':\s+(.+?)\s+",
                RegexOptions.Singleline).Groups[1].ToString().ToLower().Trim();

            return ((match == "true") || (match == "true,"));
        }

        private static bool GetSize(this VideoInfo videoInfo)
        {
            var request = (HttpWebRequest)HttpWebRequest.Create(videoInfo.VideoUri);

            var response = (HttpWebResponse)request.GetResponse();

            request.Abort();

            if (response.ContentLength == -1)
                return false;

            videoInfo.Length = response.ContentLength;

            return true;
        }
    }
}
