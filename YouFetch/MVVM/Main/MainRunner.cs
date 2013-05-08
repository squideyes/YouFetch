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
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Linq;
using System.Windows.Media.Imaging;

namespace YouFetch
{
    public static class MainRunner
    {
        private static IWpfContext context = new WpfContext();

        public static void Run()
        {
            const string BADURLORVIDEOID =
                "The YouTube URL or Video ID is invalid!";

            const string NOVIDEOINFOS =
                "For some reason, no information was returned for the \"{0}\" video!";

            const string WASFETCHED =
                "The \"{0}\" video file was successfully fetched!";

            var view = new MainView();

            var model = new MainModel();

            var viewModel = new MainViewModel(model);

            viewModel.OnClose += (s, e) =>
                view.Close();

            if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.RecentVideoIds))
            {
                Properties.Settings.Default.RecentVideoIds.Split(',').ToList().
                    ForEach(videoId => viewModel.RecentVideoIds.Add(videoId));
            }

            viewModel.UrlOrVideoId = Properties.Settings.Default.LastVideoId;

            viewModel.OnError += (s, e) =>
                MessageBox.Show(view, e.Data.Message, "Warning",
                MessageBoxButton.OK, MessageBoxImage.Warning);

            viewModel.OnBadUrlOrVideoId += (s, e) =>
                MessageBox.Show(view, BADURLORVIDEOID, "Warning",
                MessageBoxButton.OK, MessageBoxImage.Warning);

            viewModel.OnNoVideoInfos += (s, e) =>
                MessageBox.Show(view, string.Format(NOVIDEOINFOS, e.Data),
                "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);

            viewModel.OnAbout += (s, e) =>
                Process.Start("http://squideyes.com/2013/05/08/url-hunting/");

            viewModel.OnOpen += (s, e) => Process.Start(e.Data);

            viewModel.OnFetch += (s, e) =>
            {
                var videoId = e.Data.Substring(0, e.Data.IndexOf("."));

                Properties.Settings.Default.LastVideoId = videoId;

                viewModel.RecentVideoIds.Add(videoId);

                Properties.Settings.Default.RecentVideoIds =
                    string.Join(",", viewModel.RecentVideoIds.ToArray());

                Properties.Settings.Default.Save();

                MessageBox.Show(view, string.Format(WASFETCHED, e.Data),
                    "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            };

            view.DataContext = viewModel;

            view.ShowDialog();
        }

        private static void Dispatch(Action action)
        {
            context.BeginInvoke(action);
        }
    }
}
