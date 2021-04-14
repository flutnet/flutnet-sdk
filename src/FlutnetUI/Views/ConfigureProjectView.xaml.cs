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
using System.Reactive;
using System.Reactive.Disposables;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using FlutnetUI.ViewModels;
using ReactiveUI;

namespace FlutnetUI.Views
{
    public class ConfigureProjectView : ReactiveUserControl<ConfigureProjectViewModel>
    {
        public ConfigureProjectView()
        {
            InitializeComponent();
            TreeView projectTree = this.Find<TreeView>("ProjectTree");
     
        }

        private void InitializeComponent()
        {
            this.WhenActivated(disposables =>
            {
                if (ViewModel == null)
                    return;

                // Execute the command when the View is activated
                ViewModel.CheckFlutterVersion.Execute(Unit.Default).Subscribe().DisposeWith(disposables);
            });
            AvaloniaXamlLoader.Load(this);
        }
    }
}