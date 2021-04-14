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

using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using Avalonia.Controls;
using Avalonia.Media;
using FlutnetUI.Ext.Models;
using ReactiveUI;

namespace FlutnetUI.Ext.ViewModels
{
    public class MessageBoxViewModel : ReactiveObject
    {
        public MessageBoxViewModel(
            string text, string caption, MessageBoxButtons buttons,
            Drawing contentIcon,
            MessageBoxDefaultButton defaultButton,
            MessageBoxOptions options)
        {
            Title = caption;
            StartupLocation = options.StartupLocation;

            ContentHeader = options.HeaderText;
            ContentMessage = text;
            ContentIcon = contentIcon;

            Buttons = new ObservableCollection<MessageBoxButton>();
            switch (buttons)
            {
                case MessageBoxButtons.OK:
                    Buttons.Add(new MessageBoxButton(DialogResult.OK)
                    {
                        IsDefault = defaultButton == MessageBoxDefaultButton.Button1,
                        HasAccent = options.AccentButtons.HasFlag(MessageBoxAccentButtons.Button1)
                    });
                    break;

                case MessageBoxButtons.OKCancel:
                    Buttons.Add(new MessageBoxButton(DialogResult.OK)
                    {
                        IsDefault = defaultButton == MessageBoxDefaultButton.Button1,
                        HasAccent = options.AccentButtons.HasFlag(MessageBoxAccentButtons.Button1)
                    });
                    Buttons.Add(new MessageBoxButton(DialogResult.Cancel)
                    {
                        IsDefault = defaultButton == MessageBoxDefaultButton.Button2,
                        HasAccent = options.AccentButtons.HasFlag(MessageBoxAccentButtons.Button2)
                    });
                    break;

                case MessageBoxButtons.AbortRetryIgnore:
                    Buttons.Add(new MessageBoxButton(DialogResult.Abort)
                    {
                        IsDefault = defaultButton == MessageBoxDefaultButton.Button1,
                        HasAccent = options.AccentButtons.HasFlag(MessageBoxAccentButtons.Button1)
                    });
                    Buttons.Add(new MessageBoxButton(DialogResult.Retry)
                    {
                        IsDefault = defaultButton == MessageBoxDefaultButton.Button2,
                        HasAccent = options.AccentButtons.HasFlag(MessageBoxAccentButtons.Button2)
                    });
                    Buttons.Add(new MessageBoxButton(DialogResult.Ignore)
                    {
                        IsDefault = defaultButton == MessageBoxDefaultButton.Button3,
                        HasAccent = options.AccentButtons.HasFlag(MessageBoxAccentButtons.Button3)
                    });
                    break;

                case MessageBoxButtons.YesNoCancel:
                    Buttons.Add(new MessageBoxButton(DialogResult.Yes)
                    {
                        IsDefault = defaultButton == MessageBoxDefaultButton.Button1,
                        HasAccent = options.AccentButtons.HasFlag(MessageBoxAccentButtons.Button1)
                    });
                    Buttons.Add(new MessageBoxButton(DialogResult.No)
                    {
                        IsDefault = defaultButton == MessageBoxDefaultButton.Button2,
                        HasAccent = options.AccentButtons.HasFlag(MessageBoxAccentButtons.Button2)
                    });
                    Buttons.Add(new MessageBoxButton(DialogResult.Cancel)
                    {
                        IsDefault = defaultButton == MessageBoxDefaultButton.Button3,
                        HasAccent = options.AccentButtons.HasFlag(MessageBoxAccentButtons.Button3)
                    });
                    break;

                case MessageBoxButtons.YesNo:
                    Buttons.Add(new MessageBoxButton(DialogResult.Yes)
                    {
                        IsDefault = defaultButton == MessageBoxDefaultButton.Button1,
                        HasAccent = options.AccentButtons.HasFlag(MessageBoxAccentButtons.Button1)
                    });
                    Buttons.Add(new MessageBoxButton(DialogResult.No)
                    {
                        IsDefault = defaultButton == MessageBoxDefaultButton.Button2,
                        HasAccent = options.AccentButtons.HasFlag(MessageBoxAccentButtons.Button2)
                    });
                    break;

                case MessageBoxButtons.RetryCancel:
                    Buttons.Add(new MessageBoxButton(DialogResult.Retry)
                    {
                        IsDefault = defaultButton == MessageBoxDefaultButton.Button1,
                        HasAccent = options.AccentButtons.HasFlag(MessageBoxAccentButtons.Button1)
                    });
                    Buttons.Add(new MessageBoxButton(DialogResult.Cancel)
                    {
                        IsDefault = defaultButton == MessageBoxDefaultButton.Button2,
                        HasAccent = options.AccentButtons.HasFlag(MessageBoxAccentButtons.Button2)
                    });
                    break;

                case MessageBoxButtons.Custom1:
                    Buttons.Add(new MessageBoxButton(options.ButtonTexts?.ElementAtOrDefault(0), DialogResult.Custom1)
                    {
                        IsDefault = defaultButton == MessageBoxDefaultButton.Button1,
                        HasAccent = options.AccentButtons.HasFlag(MessageBoxAccentButtons.Button1)
                    });
                    break;

                case MessageBoxButtons.Custom1Custom2:
                    Buttons.Add(new MessageBoxButton(options.ButtonTexts?.ElementAtOrDefault(0), DialogResult.Custom1)
                    {
                        IsDefault = defaultButton == MessageBoxDefaultButton.Button1,
                        HasAccent = options.AccentButtons.HasFlag(MessageBoxAccentButtons.Button1)
                    });
                    Buttons.Add(new MessageBoxButton(options.ButtonTexts?.ElementAtOrDefault(1), DialogResult.Custom2)
                    {
                        IsDefault = defaultButton == MessageBoxDefaultButton.Button2,
                        HasAccent = options.AccentButtons.HasFlag(MessageBoxAccentButtons.Button2)
                    });
                    break;

                case MessageBoxButtons.Custom1Custom2Custom3:
                    Buttons.Add(new MessageBoxButton(options.ButtonTexts?.ElementAtOrDefault(0), DialogResult.Custom1)
                    {
                        IsDefault = defaultButton == MessageBoxDefaultButton.Button1,
                        HasAccent = options.AccentButtons.HasFlag(MessageBoxAccentButtons.Button1)
                    });
                    Buttons.Add(new MessageBoxButton(options.ButtonTexts?.ElementAtOrDefault(1), DialogResult.Custom2)
                    {
                        IsDefault = defaultButton == MessageBoxDefaultButton.Button2,
                        HasAccent = options.AccentButtons.HasFlag(MessageBoxAccentButtons.Button2)
                    });
                    Buttons.Add(new MessageBoxButton(options.ButtonTexts?.ElementAtOrDefault(2), DialogResult.Custom3)
                    {
                        IsDefault = defaultButton == MessageBoxDefaultButton.Button3,
                        HasAccent = options.AccentButtons.HasFlag(MessageBoxAccentButtons.Button3)
                    });
                    break;
            }

            ButtonClick = ReactiveCommand.Create<Button, Unit>(sender =>
            {
                MessageBoxButton button = (MessageBoxButton) sender.DataContext;
                Result = button.Result;
                return Unit.Default;
            });
        }

        #region Window properties

        public string Title { get; }

        public WindowStartupLocation StartupLocation { get; }

        public DialogResult Result
        {
            get => _result;
            set => this.RaiseAndSetIfChanged(ref _result, value);
        }
        DialogResult _result = DialogResult.None;

        #endregion

        #region Content properties

        public string ContentHeader { get; }
        public bool HasHeader => !(string.IsNullOrEmpty(ContentHeader));

        public string ContentMessage { get; }

        public Drawing ContentIcon { get; }
        public bool HasIcon => ContentIcon != null;

        public ObservableCollection<MessageBoxButton> Buttons { get; }

        public ReactiveCommand<Button, Unit> ButtonClick { get; }

        #endregion
    }
}