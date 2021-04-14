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

namespace FlutnetUI.Ext
{
    /// <summary>
    /// Flags for defining the accent (colored) buttons on a MessageBox.
    /// </summary>
    [Flags]
    public enum MessageBoxAccentButtons
    {
        /// <summary>
        /// No button on the message box has accent.
        /// </summary>
        None = 0,

        /// <summary>
        /// The first button on the message box has accent.
        /// </summary>
        Button1 = 1,

        /// <summary>
        /// The second button on the message box has accent.
        /// </summary>
        Button2 = 2,

        /// <summary>
        /// The third button on the message box has accent.
        /// </summary>
        Button3 = 4
    }
}