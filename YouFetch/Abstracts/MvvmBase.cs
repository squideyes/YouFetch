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
using System.ComponentModel;
using System.Diagnostics.Contracts;

namespace YouFetch
{
    public abstract class MvvmBase 
    {
        protected readonly IDispatcher Dispatcher;

        public MvvmBase()
        {
            Dispatcher = UIDispatcher.Current;
        }

        protected void Notify(EventHandler handler)
        {
            if (handler != null)
                InternalNotify(() => handler(this, new NotifyArgs()));
        }

        protected void Notify(EventHandler handler, NotifyArgs e)
        {
            if (handler != null)
                InternalNotify(() => handler(this, e));
        }

        protected void Notify(EventHandler<NotifyArgs> handler)
        {
            if (handler != null)
                InternalNotify(() => handler(this, new NotifyArgs()));
        }

        protected void Notify(EventHandler<NotifyArgs> handler, NotifyArgs e)
        {
            if (handler != null)
                InternalNotify(() => handler(this, e));
        }

        protected void Notify<O>(EventHandler<NotifyArgs<O>> handler, NotifyArgs<O> e)
        {
            if (handler != null)
                InternalNotify(() => handler(this, e));
        }

        private void InternalNotify(Action method)
        {
            Contract.Requires(method != null);

            if (UIDispatcher.Current.CheckAccess())
                method();
            else
                UIDispatcher.Current.BeginInvoke(method);
        }
    }
}
