// Copyright (c) 2020-2021 Novagem Solutions S.r.l.
//
// This file is part of Flutnet.
//
// Flutnet is a free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// Flutnet is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY, without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with Flutnet.  If not, see <http://www.gnu.org/licenses/>.

using System;
using ReactiveUI;
using Splat;

namespace FlutnetUI.ViewModels
{
    public abstract class PageViewModel : ReactiveObject, IPageViewModel
    {
        protected PageViewModel(string path, IScreen screen = null)
        {
            HostScreen = screen ?? Locator.Current.GetService<IScreen>();
            UrlPathSegment = path;
            _title = path;
        }

        #region IRoutableViewModel implementation

        // Reference to IScreen that owns the routable view model.
        public IScreen HostScreen { get; }

        // Unique identifier for the routable view model.
        public string UrlPathSegment { get; }

        #endregion

        public string Title
        {
            get => _title;
            protected set => this.RaiseAndSetIfChanged(ref _title, value);
        }
        string _title;

        public string Description
        {
            get => _description;
            protected set => this.RaiseAndSetIfChanged(ref _description, value);
        }
        string _description = string.Empty;

        public bool IsDescriptionVisible
        {
            get => _isDescriptionVisible;
            protected set => this.RaiseAndSetIfChanged(ref _isDescriptionVisible, value);
        }
        bool _isDescriptionVisible;

        public bool IsBusy
        {
            get => _isBusy;
            protected set => this.RaiseAndSetIfChanged(ref _isBusy, value);
        }
        bool _isBusy;
    }
}