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
using System.Diagnostics.Contracts;
using System.Windows;
using WindowsDispatcher = System.Windows.Threading.Dispatcher;

namespace YouFetch
{
    public sealed class UIDispatcher : IDispatcher
    {
        private static volatile IDispatcher dispatcher;

        private static readonly object SyncRoot = new Object();
        
        private readonly WindowsDispatcher windowsDispatcher;

        private UIDispatcher(WindowsDispatcher windowsDispatcher)
        {
            this.windowsDispatcher = windowsDispatcher;
        }

        public bool CheckAccess()
        {
            return windowsDispatcher.CheckAccess();
        }

        public void BeginInvoke(Action action)
        {
            Contract.Requires(action != null);

            windowsDispatcher.BeginInvoke(action);
        }

        public static void Initialize()
        {
            dispatcher = new UIDispatcher(
                WindowsDispatcher.CurrentDispatcher);
        }

        public static IDispatcher Current
        {
            get
            {
                if (dispatcher == null)
                {
                    lock (SyncRoot)
                    {
                        dispatcher = new UIDispatcher(
                            WindowsDispatcher.CurrentDispatcher);
                    }
                }

                return dispatcher;
            }
        }

        public static void Execute(Action action)
        {
            Contract.Requires(action != null);

            if (dispatcher.CheckAccess())
                action();
            else
                dispatcher.BeginInvoke(action);
        }
    }
}
