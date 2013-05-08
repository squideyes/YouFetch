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
using System.ComponentModel;
using System.IO;
using System.Net.Http;
using System.Net.Http.Handlers;
using System.Text.RegularExpressions;
using System.Windows.Media.Imaging;

namespace YouFetch
{
    public class MainViewModel :
        ViewModelBase<MainViewModel, MainModel>
    {
        private Regex videoIdRegex = new Regex("^[A-Za-z0-9-_]{6,12}$", RegexOptions.Compiled);

        private bool canGetInfo = true;
        private bool fetching = false;

        private bool showMarquee;
        private int progress;
        private string urlOrVideoId;

        public event EventHandler<NotifyArgs> OnBadUrlOrVideoId;
        public event EventHandler<NotifyArgs> OnClose;
        public event EventHandler<NotifyArgs<string>> OnOpen;
        public event EventHandler<NotifyArgs> OnHelp;
        public event EventHandler<NotifyArgs<string>> OnFetch;
        public event EventHandler<NotifyArgs<string>> OnNoVideoInfos;
        public event EventHandler<NotifyArgs<Exception>> OnError;

        public MainViewModel(MainModel model)
            : base(model)
        {
            VideoInfos = new BindingList<VideoInfo>();
            RecentVideoIds = new BindingList<string>();

            CanEdit = true;

            AssociateProperties(m => m.VideoInfo, vm => vm.FetchCommand);
        }

        public bool CanEdit { get; private set; }
        public BitmapImage Thumbnail { get; private set; }
        public BindingList<VideoInfo> VideoInfos { get; private set; }
        public BindingList<string> RecentVideoIds { get; private set; }

        public string UrlOrVideoId
        {
            get
            {
                return urlOrVideoId;
            }
            set
            {
                if (urlOrVideoId == value)
                    return;

                urlOrVideoId = value;

                canGetInfo = true;

                Thumbnail = null;

                VideoInfos.Clear();

                NotifyPropertyChanged(vm => vm.UrlOrVideoId);
                NotifyPropertyChanged(vm => vm.Thumbnail);
                NotifyPropertyChanged(vm => vm.GetInfoCommand);
            }
        }

        public bool ShowMarquee
        {
            get
            {
                return showMarquee;
            }
            set
            {
                showMarquee = value;

                NotifyPropertyChanged(m => m.ShowMarquee);
            }
        }

        public int Progress
        {
            get
            {
                return progress;
            }
            set
            {
                progress = value;

                NotifyPropertyChanged(m => m.Progress);
            }
        }

        private string GetVideoId(string urlOrVideoId)
        {
            var videoId = UrlOrVideoId == null ? UrlOrVideoId :
                string.Copy(UrlOrVideoId.Trim());

            if (string.IsNullOrWhiteSpace(videoId))
                return null;

            if (videoId.Length > 12)
            {
                if (!videoId.StartsWith(Consts.WatchUrl,
                    StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }

                videoId = videoId.Substring(Consts.WatchUrl.Length);
            }

            if ((videoId.Length < 10) || (videoId.Length > 12))
                return null;

            if (!videoIdRegex.IsMatch(videoId))
                return null;
            else
                return videoId;
        }

        public bool CanGetInfo
        {
            get
            {
                return canGetInfo && (GetVideoId(UrlOrVideoId) != null);
            }
        }

        public Uri GetThumbnailUri(string videoId)
        {
            const string IMAGEURL = "http://i3.ytimg.com/vi/{0}/default.jpg";

            return new Uri(string.Format(IMAGEURL, videoId));
        }

        private BitmapImage GetThumbnail()
        {
            var videoId = GetVideoId(UrlOrVideoId);

            if (videoId == null)
                return null;

            var bi = new BitmapImage();

            bi.BeginInit();
            bi.UriSource = GetThumbnailUri(videoId);
            bi.DecodePixelWidth = Consts.ThumbnailWidth;
            bi.DecodePixelHeight = Consts.ThumbnailHeight;
            bi.EndInit();

            return bi;
        }

        private async void Fetch()
        {
            try
            {
                CanEdit = false;

                fetching = true;

                NotifyPropertyChanged(vm => vm.CanEdit);
                NotifyPropertyChanged(vm => vm.FetchCommand);

                var path = Path.GetDirectoryName(FileName);

                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                var handler = new HttpClientHandler();

                var processMessageHander = new ProgressMessageHandler(handler);

                var client = new HttpClient(processMessageHander);

                processMessageHander.HttpReceiveProgress += (s, ea) =>
                    Progress = ea.ProgressPercentage;

                var webStream = await client.
                    GetStreamAsync(Model.VideoInfo.VideoUri).ConfigureAwait(false);

                using (var fileStream = File.OpenWrite(FileName))
                    await webStream.CopyToAsync(fileStream);

                NotifyPropertyChanged(vm => vm.OpenCommand);

                Notify(OnFetch, new NotifyArgs<string>(Path.GetFileName(FileName)));
            }
            catch (Exception error)
            {
                Notify(OnError, new NotifyArgs<Exception>(error));
            }

            CanEdit = true;

            fetching = false;

            NotifyPropertyChanged(vm => vm.CanEdit);
            NotifyPropertyChanged(vm => vm.FetchCommand);

        }

        public DelegateCommand CloseCommand
        {
            get
            {
                return new DelegateCommand(() => Notify(OnClose));
            }
        }

        public DelegateCommand GetInfoCommand
        {
            get
            {
                return new DelegateCommand(
                    async () =>
                    {
                        Progress = 0;
                        ShowMarquee = true;

                        canGetInfo = false;

                        Thumbnail = null;

                        VideoInfos.Clear();

                        NotifyPropertyChanged(vm => vm.Thumbnail);
                        NotifyPropertyChanged(vm => vm.GetInfoCommand);

                        List<VideoInfo> videoInfos;

                        try
                        {
                            var videoId = GetVideoId(UrlOrVideoId);

                            videoInfos = await YouTubeHelper.GetVideoInfos(videoId);

                            if (videoInfos.Count == 0)
                            {
                                canGetInfo = true;

                                Notify(OnNoVideoInfos, new NotifyArgs<string>(videoId));
                            }
                            else
                            {
                                videoInfos.ForEach(
                                    videoInfo => VideoInfos.Add(videoInfo));

                                Model.VideoInfo = VideoInfos[0];  //?????????????????

                                Thumbnail = GetThumbnail();

                                NotifyPropertyChanged(vm => vm.Thumbnail);
                            }
                        }
                        catch (HttpRequestException error)
                        {
                            canGetInfo = true;

                            if (error.Message.Contains("404 (Not Found)."))
                                Notify(OnBadUrlOrVideoId, new NotifyArgs());
                            else
                                Notify(OnError, new NotifyArgs<Exception>(error));
                        }
                        catch (Exception error)
                        {
                            canGetInfo = true;

                            Notify(OnError, new NotifyArgs<Exception>(error));
                        }

                        Progress = 0;
                        ShowMarquee = false;

                        NotifyPropertyChanged(vm => vm.GetInfoCommand);

                    },
                    () => CanEdit && CanGetInfo);
            }
        }

        public DelegateCommand FetchCommand
        {
            get
            {
                return new DelegateCommand(
                    () => Fetch(), 
                    () => (!fetching) && (Model.VideoInfo != null));
            }
        }

        public DelegateCommand HelpCommand
        {
            get
            {
                return new DelegateCommand(() => Notify(OnHelp));
            }
        }

        public DelegateCommand OpenCommand
        {
            get
            {
                return new DelegateCommand(
                    () => Notify<string>(OnOpen, new NotifyArgs<string>(FileName)),
                    () => GetCanOpen);
            }
        }

        private string FileName
        {
            get
            {
                return Model.VideoInfo.GetFileName(
                    Properties.Settings.Default.SaveToPath);
            }
        }

        private bool GetCanOpen
        {
            get
            {
                if (Model.VideoInfo == null)
                    return false;

                var fileInfo = new FileInfo(FileName);

                return (fileInfo.Length == Model.VideoInfo.Length);
            }
        }
    }
}
