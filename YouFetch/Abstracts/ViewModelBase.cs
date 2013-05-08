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
using System.Linq.Expressions;
using System.Collections.Generic;
using PropertyDictionary = System.Collections.Generic.Dictionary<
    string, System.ComponentModel.PropertyChangedEventHandler>;

namespace YouFetch
{
    public abstract class ViewModelBase<VM, M> : MvvmBase, INotifyPropertyChanged
        where VM : ViewModelBase<VM, M>
        where M : ModelBase<M>
    {
        private Dictionary<string, PropertyDictionary> propertyHandlers
            = new Dictionary<string, PropertyDictionary>(); 
        
        public event PropertyChangedEventHandler PropertyChanged;

        public ViewModelBase(M model)
        {
            Contract.Requires(model != null);

            Model = model;
        }

        public M Model { get; private set; }

        protected virtual void AssociateProperties<MR, VMR>(
            Expression<Func<M, MR>> modelProperty, Expression<Func<VM, VMR>> viewModelProperty)
        {
            var modelPropertyName = ((MemberExpression)modelProperty.Body).Member.Name;
            var viewModelPropertyName = ((MemberExpression)viewModelProperty.Body).Member.Name;

            if (!propertyHandlers.ContainsKey(modelPropertyName))
                propertyHandlers.Add(modelPropertyName, new PropertyDictionary());

            var handlers = propertyHandlers[modelPropertyName];

            PropertyChangedEventHandler handler = (s, ea) =>
            {
                if (ea.PropertyName == modelPropertyName)
                    NotifyPropertyChanged(viewModelPropertyName, this, PropertyChanged);
            };

            Model.PropertyChanged += handler;

            handlers.Add(viewModelPropertyName, handler);
        }

        private void NotifyPropertyChanged(string propertyName,
            object sender, PropertyChangedEventHandler propertyChanged)
        {
            if (propertyChanged != null)
            {
                if (Dispatcher.CheckAccess())
                {
                    propertyChanged(sender, new PropertyChangedEventArgs(propertyName));

                }
                else
                {
                    Action action = () => propertyChanged
                        (sender, new PropertyChangedEventArgs(propertyName));

                    Dispatcher.BeginInvoke(action);
                }
            }
        }

        protected virtual void NotifyPropertyChanged<R>(Expression<Func<VM, R>> property)
        {
            Contract.Requires(property != null);

            BindingHelper.NotifyPropertyChanged(property, this, PropertyChanged, Dispatcher);
        }
    }
}
