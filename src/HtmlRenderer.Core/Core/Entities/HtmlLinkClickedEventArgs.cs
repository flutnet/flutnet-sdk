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
using System.Collections.Generic;

namespace TheArtOfDev.HtmlRenderer.Core.Entities
{
    /// <summary>
    /// Raised when the user clicks on a link in the html.
    /// </summary>
    public sealed class HtmlLinkClickedEventArgs : EventArgs
    {
        /// <summary>
        /// the link href that was clicked
        /// </summary>
        private readonly string _link;

        /// <summary>
        /// collection of all the attributes that are defined on the link element
        /// </summary>
        private readonly Dictionary<string, string> _attributes;

        /// <summary>
        /// use to cancel the execution of the link
        /// </summary>
        private bool _handled;

        /// <summary>
        /// Init.
        /// </summary>
        /// <param name="link">the link href that was clicked</param>
        public HtmlLinkClickedEventArgs(string link, Dictionary<string, string> attributes)
        {
            _link = link;
            _attributes = attributes;
        }

        /// <summary>
        /// the link href that was clicked
        /// </summary>
        public string Link
        {
            get { return _link; }
        }

        /// <summary>
        /// collection of all the attributes that are defined on the link element
        /// </summary>
        public Dictionary<string, string> Attributes
        {
            get { return _attributes; }
        }

        /// <summary>
        /// use to cancel the execution of the link
        /// </summary>
        public bool Handled
        {
            get { return _handled; }
            set { _handled = value; }
        }

        public override string ToString()
        {
            return string.Format("Link: {0}, Handled: {1}", _link, _handled);
        }
    }
}