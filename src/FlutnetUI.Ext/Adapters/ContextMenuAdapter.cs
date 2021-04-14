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
using TheArtOfDev.HtmlRenderer.Adapters;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;

namespace FlutnetUI.Ext.Adapters
{
    /// <summary>
    /// Adapter for Avalonia context menu.
    /// </summary>
    internal sealed class NullContextMenuAdapter : RContextMenu
    {
        //TODO: actually implement context menu

        private int _itemCount;
        public override int ItemsCount => _itemCount;
        public override void AddDivider()
        {
            
        }

        public override void AddItem(string text, bool enabled, EventHandler onClick)
        {
            _itemCount++;
        }

        public override void RemoveLastDivider()
        {
            _itemCount++;
        }

        public override void Show(RControl parent, RPoint location)
        {
        }

        public override void Dispose()
        {
        }
    }
}