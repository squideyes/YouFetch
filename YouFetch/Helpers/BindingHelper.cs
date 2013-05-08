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
using System.Threading;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Diagnostics.Contracts;

namespace YouFetch
{
    public static class BindingHelper
    {
        public static void NotifyPropertyChanged<M, R>(this M model, Expression<Func<M, R>> property,
            PropertyChangedEventHandler propertyChanged, IDispatcher dispatcher)
        {
            Contract.Requires(model != null);
            Contract.Requires(property != null);
            Contract.Requires(dispatcher != null);

            var propertyName = ((MemberExpression)property.Body).Member.Name;

            InternalNotifyPropertyChanged(propertyName, model, propertyChanged, dispatcher);
        }

        public static void NotifyPropertyChanged<M, R>(Expression<Func<M, R>> property,
            object sender, PropertyChangedEventHandler propertyChanged, IDispatcher dispatcher)
        {
            Contract.Requires(property != null);
            Contract.Requires(sender!= null);
            Contract.Requires(dispatcher != null);

            var propertyName = ((MemberExpression)property.Body).Member.Name;

            InternalNotifyPropertyChanged(propertyName, sender, propertyChanged, dispatcher);
        }

        internal static void InternalNotifyPropertyChanged(string propertyName,
            object sender, PropertyChangedEventHandler propertyChanged, IDispatcher dispatcher)
        {
            if (propertyChanged != null)
            {
                if (dispatcher.CheckAccess())
                {
                    propertyChanged(sender, new PropertyChangedEventArgs(propertyName));
                }
                else
                {
                    dispatcher.BeginInvoke(() => propertyChanged(
                        sender, new PropertyChangedEventArgs(propertyName)));
                }
            }
        }
    }
}
