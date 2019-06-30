﻿/*AmpShell : .NET front-end for DOSBox
 * Copyright (C) 2009, 2019 Maximilien Noal
 *This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
 * This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the GNU General Public License for more details.
 * You should have received a copy of the GNU General Public License along with this program.
 * If not, see <http://www.gnu.org/licenses/>.*/

using AmpShell.Configuration;
using AmpShell.Model;
using AmpShell.Notification;

namespace AmpShell.ViewModel
{
    public class CategoryViewModel : PropertyChangedNotifier
    {
        private string _name = "";
        private readonly string _editedCategorySignature;

        public CategoryViewModel()
        {
        }

        public CategoryViewModel(string editedCategorySignature)
        {
            this._editedCategorySignature = editedCategorySignature;
            this.Name = RootModelQuery.GetCategoryWithSignature(_editedCategorySignature).Title;
        }

        public string Name
        {
            get => _name;
            set { Set<string>(ref _name, value); }
        }

        public void CreateCategory()
        {
            if(string.IsNullOrWhiteSpace(_editedCategorySignature))
            {
                var category = new Category(Name, RootModelQuery.GetAUniqueSignature());
                UserDataLoaderSaver.UserData.AddChild(category);
            }
            else
            {
                RootModelQuery.GetCategoryWithSignature(_editedCategorySignature).Title = Name;
            }
        }

        public bool IsDataValid()
        {
            return string.IsNullOrWhiteSpace(Name) == false;
        }
    }
}
