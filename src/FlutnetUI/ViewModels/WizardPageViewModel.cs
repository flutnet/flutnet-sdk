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

using System.Threading.Tasks;
using ReactiveUI;

namespace FlutnetUI.ViewModels
{
    public abstract class WizardPageViewModel : PageViewModel, IWizardPageViewModel
    {
        protected WizardPageViewModel(string path, IScreen screen = null) : base(path, screen)
        {
        }

        public string CancelText
        {
            get => _cancelText;
            protected set => this.RaiseAndSetIfChanged(ref _cancelText, value);
        }
        string _cancelText = "Cancel";

        public bool CancelVisible
        {
            get => _cancelVisible;
            protected set => this.RaiseAndSetIfChanged(ref _cancelVisible, value);
        }
        bool _cancelVisible = true;

        public string BackText
        {
            get => _backText;
            protected set => this.RaiseAndSetIfChanged(ref _backText, value);
        }
        string _backText = "Back";

        public bool BackVisible
        {
            get => _backVisible;
            protected set => this.RaiseAndSetIfChanged(ref _backVisible, value);
        }
        bool _backVisible = true;

        public string NextText
        {
            get => _nextText;
            protected set => this.RaiseAndSetIfChanged(ref _nextText, value);
        }
        string _nextText = "Next";

        public bool NextEnabled
        {
            get => _nextEnabled;
            protected set => this.RaiseAndSetIfChanged(ref _nextEnabled, value);
        }
        bool _nextEnabled = true;



        public virtual Task OnNext()
        {
            return Task.CompletedTask;
        }

        public bool IsFinishPage
        {
            get => _isFinishPage;
            protected set => this.RaiseAndSetIfChanged(ref _isFinishPage, value);
        }
        bool _isFinishPage;

        public virtual IRoutableViewModel GetNextPage()
        {
            return null;
        }
    }
}