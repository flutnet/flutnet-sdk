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

using TheArtOfDev.HtmlRenderer.Core;

namespace TheArtOfDev.HtmlRenderer.Adapters.Entities
{
    /// <summary>
    /// Even class for handling keyboard events in <see cref="HtmlContainerInt"/>.
    /// </summary>
    public sealed class RKeyEvent
    {
        /// <summary>
        /// is control is pressed
        /// </summary>
        private readonly bool _control;

        /// <summary>
        /// is 'A' key is pressed
        /// </summary>
        private readonly bool _aKeyCode;

        /// <summary>
        /// is 'C' key is pressed
        /// </summary>
        private readonly bool _cKeyCode;

        /// <summary>
        /// Init.
        /// </summary>
        public RKeyEvent(bool control, bool aKeyCode, bool cKeyCode)
        {
            _control = control;
            _aKeyCode = aKeyCode;
            _cKeyCode = cKeyCode;
        }

        /// <summary>
        /// is control is pressed
        /// </summary>
        public bool Control
        {
            get { return _control; }
        }

        /// <summary>
        /// is 'A' key is pressed
        /// </summary>
        public bool AKeyCode
        {
            get { return _aKeyCode; }
        }

        /// <summary>
        /// is 'C' key is pressed
        /// </summary>
        public bool CKeyCode
        {
            get { return _cKeyCode; }
        }
    }
}