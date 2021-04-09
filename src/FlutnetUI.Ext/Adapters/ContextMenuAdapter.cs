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