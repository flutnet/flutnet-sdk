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

namespace FlutnetUI.Ext
{
    /// <summary>
    /// Specifies constants defining which buttons to display on a MessageBox.
    /// </summary>
    public enum MessageBoxButtons
    {
        /// <summary>
        /// The message box contains an OK button.
        /// </summary>
        OK = 0,

        /// <summary>
        /// The message box contains OK and Cancel buttons.
        /// </summary>
        OKCancel = 1,

        /// <summary>
        /// The message box contains Abort, Retry, and Ignore buttons.
        /// </summary>
        AbortRetryIgnore = 2,

        /// <summary>
        /// The message box contains Yes, No, and Cancel buttons.
        /// </summary>
        YesNoCancel = 3,

        /// <summary>
        /// The message box contains Yes and No buttons.
        /// </summary>
        YesNo = 4,

        /// <summary>
        /// The message box contains Retry and Cancel buttons.
        /// </summary>
        RetryCancel = 5,

        /// <summary>
        /// The message box contains a custom button.
        /// </summary>
        Custom1 = 6,

        /// <summary>
        /// The message box contains two custom buttons.
        /// </summary>
        Custom1Custom2 = 7,

        /// <summary>
        /// The message box contains three custom buttons.
        /// </summary>
        Custom1Custom2Custom3 = 8
    }
}