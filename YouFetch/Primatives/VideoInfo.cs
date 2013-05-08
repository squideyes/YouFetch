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
using System.Windows;
using System.IO;
using System.Globalization;

namespace YouFetch
{
    public class VideoInfo : IComparable<VideoInfo>
    {
        public string VideoId { get; internal set; }
        public string Title { get; internal set; }
        public string Extension { get; internal set; }
        public Uri VideoUri { get; internal set; }
        public long Length { get; internal set; }
        public Size Size { get; internal set; }
        public long Seconds { get; internal set; }

        public Uri ShortUri
        {
            get
            {
                return new Uri(Consts.WatchUrl + VideoId);
            }
        }

        public string FileSize
        {
            get
            {
                if (Length == 0)
                    return null;

                return ToKbMbOrGb(Length);
            }
        }

        private string ToKbMbOrGb(long size)
        {
            if (size >= 1073741824)
                return string.Format("{0:N2} GB", size / 1073741824.0);

            if (size >= 1048576)
                return string.Format("{0:N2} MB", size / 1048576.0);

            return string.Format("{0:N2} KB", size / 1024.0);
        }

        public string Duration
        {
            get
            {
                if (Seconds == 0)
                    return null;

                return TimeSpan.FromSeconds(Seconds).ToString(@"hh\:mm\:ss");
            }
        }

        internal void SetExtensionAndSize(string extension, Size size)
        {
            Extension = extension;
            Size = size;
        }

        internal string GetFileName(string basePath)
        {
            return Path.Combine(basePath, VideoId + "." + Extension);
        }

        public int CompareTo(VideoInfo other)
        {
            if (Extension == other.Extension)
            {
                if (Size.Width == other.Size.Width)
                    return Size.Height.CompareTo(other.Size.Height);
                else
                    return Size.Width.CompareTo(other.Size.Width);
            }
            else
            {
                return Extension.CompareTo(other.Extension);
            }
        }

        public override string ToString()
        {
            return string.Format("{0} ({1}x{2})",
                Extension.ToUpper(), Size.Width, Size.Height);
        }
    }
}
