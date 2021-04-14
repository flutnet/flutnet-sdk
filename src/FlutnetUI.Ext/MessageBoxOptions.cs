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

using Avalonia.Controls;

namespace FlutnetUI.Ext
{
    /// <summary>
    /// Specifies further options on a MessageBox.
    /// </summary>
    public class MessageBoxOptions
    {
        internal static readonly MessageBoxOptions Default = new MessageBoxOptions();

        public MessageBoxStyle Style { get; set; }

        public string CustomStyleUri { get; set; }

        public WindowStartupLocation StartupLocation { get; set; } = WindowStartupLocation.CenterOwner;

        public string HeaderText { get; set; }

        /// <summary>
        /// The maximum height of the window.
        /// It actually sets the maximum height of the content panel.
        /// </summary>
        public double MaxHeight { get; set; }

        /// <summary>
        /// The maximum width of the window.
        /// It actually sets the maximum width of the content panel.
        /// </summary>
        public double MaxWidth { get; set; }

        /// <summary>
        /// Enables or disables resizing of the window.
        /// If <see cref="HasSystemDecorations"/> is set to False, then this property has no effect
        /// and should be treated as a recommendation for the user setting <see cref="HasSystemDecorations"/>. 
        /// </summary>
        public bool CanResize { get; set; }

        /// <summary>
        /// Enables or disables system window decorations (title bar, buttons, etc).
        /// </summary>
        public bool HasSystemDecorations { get; set; } = true;

        /// <summary>
        /// The URL scheme that references the desired asset or manifest resource.
        /// </summary>
        /// <example>
        /// "avares://MyAssembly/Assets/icon.png"
        /// "resm:MyApp.Assets.icon.png"
        /// resm:MyApp.Assets.icon.png?assembly=MyAssembly
        /// </example>
        public string WindowIconUri { get; set; }

        public bool UseOwnerIcon { get; set; } = true;

        public MessageBoxAccentButtons AccentButtons { get; set; } = MessageBoxAccentButtons.None;

        /// <summary>
        /// Text for custom buttons.
        /// </summary>
        public string[] ButtonTexts { get; set; }

        internal string GetStyleUri()
        {
            switch (Style)
            {
                case MessageBoxStyle.Custom:
                    return CustomStyleUri;

                default:
                    return Style.GetUri();
            }
        }
    }
}
