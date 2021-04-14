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

namespace FlutnetUI.Ext.Models
{
    public class MessageBoxButton
    {
        public MessageBoxButton(DialogResult result) : this(null, result)
        {
        }

        public MessageBoxButton(string text, DialogResult result)
        {
            Name = Enum.GetName(typeof(DialogResult), result);
            Result = result;
            Text = !string.IsNullOrEmpty(text) ? text : Name;
        }

        public string Name { get; }
        public DialogResult Result { get; set; }
        public string Text { get; set; }
        public bool IsDefault { get; set; }
        public bool HasAccent { get; set; }
        public string Tag => HasAccent ? "Accent" : null;
    }
}