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
using Avalonia;

namespace FlutnetUI.Ext.Utilities
{
    internal static class PropertyHelper
    {
        public static AvaloniaProperty Register<TOwner, T>(string name, T def, Action<AvaloniaObject, AvaloniaPropertyChangedEventArgs> changed) where TOwner : AvaloniaObject
        {
            var pp = AvaloniaProperty.Register<TOwner, T>(name, def);
            Action<AvaloniaPropertyChangedEventArgs> cb = args =>
            {
                changed(args.Sender, args);
            };

            pp.Changed.Subscribe(cb);
            return pp;
        }
    }
}
