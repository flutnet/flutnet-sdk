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
