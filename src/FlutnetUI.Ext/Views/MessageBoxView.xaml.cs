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
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.ReactiveUI;
using FlutnetUI.Ext.Extensions;
using FlutnetUI.Ext.ViewModels;
using ReactiveUI;

namespace FlutnetUI.Ext.Views
{
    public class MessageBoxView : ReactiveWindow<MessageBoxViewModel>
    {
        public MessageBoxView()
        {
            InitializeComponent();
        }

        public MessageBoxView(Window owner, MessageBoxOptions options)
        {
            // Theme and Icon must be set BEFORE Avalonia loads the XAML
            // in order to avoid awkward behaviors
            this.SetStyle(options.GetStyleUri());
            if (owner != null && options.UseOwnerIcon)
                Icon = owner.Icon;
            else
                this.SetIcon(options.WindowIconUri);

            InitializeComponent();

            // CanResize must be set BEFORE SizeToContent
            // https://github.com/AvaloniaUI/Avalonia/issues/3360
            CanResize = options.CanResize;
            SizeToContent = SizeToContent.WidthAndHeight;
            HasSystemDecorations = options.HasSystemDecorations;

            DockPanel panel = this.Find<DockPanel>("ContentPanel");
            if (options.MaxHeight > 0)
                panel.MaxHeight = options.MaxHeight;
            if (options.MaxWidth > 0)
                panel.MaxWidth = options.MaxWidth;
        }

        private void InitializeComponent()
        {
            this.WhenActivated(disposables =>
            {
                if (ViewModel == null)
                    return;

                ViewModel.WhenAnyValue(vm => vm.Result).Skip(1).Subscribe(result => Close(result)).DisposeWith(disposables);
            });
            AvaloniaXamlLoader.Load(this);
        }

        internal Drawing GetIconDrawing(MessageBoxIcon icon)
        {
            switch (icon)
            {
                case MessageBoxIcon.Error:
                    return (Drawing) this.FindResource("VSCodeDark.error");

                case MessageBoxIcon.Information:
                    return (Drawing) this.FindResource("VSCodeDark.info");

                case MessageBoxIcon.Warning:
                    return (Drawing) this.FindResource("VSCodeDark.warning");
            }

            return default;
        }

        public new Task<DialogResult> Show()
        {
            TaskCompletionSource<DialogResult> tcs = new TaskCompletionSource<DialogResult>();
            Closed += delegate { tcs.TrySetResult(ViewModel.Result); };
            base.Show();
            return tcs.Task;
        }

        public new Task<DialogResult> ShowDialog(Window owner)
        { 
            return ShowDialog<DialogResult>(owner);
        }
    }
}