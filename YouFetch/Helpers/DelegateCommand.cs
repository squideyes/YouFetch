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
using System.Linq;
using System.Reflection;
using System.Windows.Input;

namespace YouFetch
{
    public class DelegateCommand : ICommand
    {
        private readonly Func<bool> canExecute;
        private readonly Action action;

        public event EventHandler CanExecuteChanged;

        public DelegateCommand(Action action, Func<bool> canExecute)
        {
            Contract.Requires(action != null);
            Contract.Requires(canExecute != null);

            this.action = action;
            this.canExecute = canExecute;
        }

        public DelegateCommand(Action action)
        {
            Contract.Requires(action != null);

            this.action = action;
            canExecute = () => true;
        }

        public bool CanExecute(object parameter)
        {
            return (canExecute == null) || canExecute();
        }

        public void Execute(object parameter)
        {
            action();
        }

        public void RaiseCanExecuteChanged()
        {
            if (CanExecuteChanged != null)
                CanExecuteChanged(this, EventArgs.Empty);
        }
    }

    public class DelegateCommand<T> : ICommand
    {
        private readonly Func<T, bool> canExecute;
        private readonly Action<T> execute;

        public event EventHandler CanExecuteChanged;

        public DelegateCommand(Action<T> execute, Func<T, bool> canExecute)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }

        public DelegateCommand(Action<T> execute)
        {
            this.execute = execute;
            this.canExecute = x => true;
        }

        public void RaiseCanExecuteChanged()
        {
            if (CanExecuteChanged != null)
                CanExecuteChanged(this, EventArgs.Empty);
        }

        public bool CanExecute(object parameter)
        {
            if (parameter == null)
                return true;

            T param = ConvertParameter(parameter);

            return canExecute == null || canExecute(param);
        }

        public void Execute(object parameter)
        {
            T param = ConvertParameter(parameter);

            execute(param);
        }

        private T ConvertParameter(object parameter)
        {
            if (parameter == null)
                return default(T);

            var casetErrorMessage = string.Format(
                "Cannot convert \"{0}\" to \"{1}\"", parameter.GetType(), typeof(T));

            T result = default(T);

            if (parameter is T)
            {
                result = (T)parameter;
            }
            else if (parameter is string)
            {
                var mi = (from m in typeof(T).GetMethods(
                              BindingFlags.Static | BindingFlags.Public)
                          where m.Name == "Parse" && m.GetParameters().Count() == 1
                          select m).FirstOrDefault();

                if (mi != null)
                {
                    try
                    {
                        result = (T)mi.Invoke(null, new object[] { parameter });
                    }
                    catch (Exception error)
                    {
                        if (error.InnerException != null)
                            throw error.InnerException;

                        throw new InvalidCastException(casetErrorMessage);
                    }
                }
            }
            else
            {
                throw new InvalidCastException(casetErrorMessage);
            }

            return result;
        }
    }
}
