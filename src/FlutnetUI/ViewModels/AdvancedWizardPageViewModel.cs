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
using ReactiveUI;

namespace FlutnetUI.ViewModels
{
    public abstract class AdvancedWizardPageViewModel : PageViewModel, IAdvancedWizardPageViewModel
    {
        protected AdvancedWizardPageViewModel(string path, IScreen screen = null) : base(path, screen)
        {
            _button2Command = screen?.Router.NavigateBack;
        }

        public string Button1Text
        {
            get => _button1Text;
            protected set => this.RaiseAndSetIfChanged(ref _button1Text, value);
        }
        string _button1Text = "Cancel";

        public bool Button1Visible
        {
            get => _button1Visible;
            protected set => this.RaiseAndSetIfChanged(ref _button1Visible, value);
        }
        bool _button1Visible = false;

        public string Button2Text
        {
            get => _button2Text;
            protected set => this.RaiseAndSetIfChanged(ref _button2Text, value);
        }
        string _button2Text = "Back";

        public bool Button2Visible
        {
            get => _button2Visible;
            protected set => this.RaiseAndSetIfChanged(ref _button2Visible, value);
        }
        bool _button2Visible = false;

        public string Button3Text
        {
            get => _button3Text;
            protected set => this.RaiseAndSetIfChanged(ref _button3Text, value);
        }
        string _button3Text = "Next";

        public bool Button3Visible
        {
            get => _button3Visible;
            protected set => this.RaiseAndSetIfChanged(ref _button3Visible, value);
        }
        bool _button3Visible = true;

        public bool Button3Enabled
        {
            get => _button3Enabled;
            protected set => this.RaiseAndSetIfChanged(ref _button3Enabled, value);
        }
        bool _button3Enabled = true;

        public ReactiveCommand<Unit, Unit> Button1Command
        {
            get => _button1Command;
            protected set => this.RaiseAndSetIfChanged(ref _button1Command, value);
        }
        ReactiveCommand<Unit, Unit> _button1Command;

        public ReactiveCommand<Unit, Unit> Button2Command
        {
            get => _button2Command;
            protected set => this.RaiseAndSetIfChanged(ref _button2Command, value);
        }
        ReactiveCommand<Unit, Unit> _button2Command;

        public ReactiveCommand<Unit, Unit> Button3Command
        {
            get => _button3Command;
            protected set => this.RaiseAndSetIfChanged(ref _button3Command, value);
        }
        ReactiveCommand<Unit, Unit> _button3Command;

        public bool IsFinishPage
        {
            get => _isFinishPage;
            protected set => this.RaiseAndSetIfChanged(ref _isFinishPage, value);
        }
        bool _isFinishPage;
    }
}