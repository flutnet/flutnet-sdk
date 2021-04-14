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

using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;

namespace FlutnetUI.ViewModels
{
    /// <summary>
    /// Denotes a ViewModel that represents a generic page with a title.
    /// </summary>
    interface IPageViewModel : IRoutableViewModel
    {
        string Title { get; }

        string Description { get; }

        bool IsDescriptionVisible { get; }

        bool IsBusy { get; }
    }

    /// <summary>
    /// Denotes a ViewModel that represents a wizard page with a title and navigation buttons.
    /// </summary>
    interface IWizardPageViewModel : IPageViewModel
    {
        string CancelText { get; }
        bool CancelVisible { get; }

        string BackText { get; }
        bool BackVisible { get; }

        string NextText { get; }
        bool NextEnabled { get; }

        Task OnNext();
        bool IsFinishPage { get; }

        IRoutableViewModel GetNextPage();
    }

    /// <summary>
    /// Denotes a ViewModel that represents a highly customizable wizard page with a title and navigation buttons.
    /// </summary>
    interface IAdvancedWizardPageViewModel : IPageViewModel
    {
        string Button1Text { get; }
        bool Button1Visible { get; }

        string Button2Text { get; }
        bool Button2Visible { get; }

        string Button3Text { get; }
        bool Button3Visible { get; }
        bool Button3Enabled { get; }

        ReactiveCommand<Unit, Unit> Button1Command { get; }
        ReactiveCommand<Unit, Unit> Button2Command { get; }
        ReactiveCommand<Unit, Unit> Button3Command { get; }

        bool IsFinishPage { get; }
    }
}