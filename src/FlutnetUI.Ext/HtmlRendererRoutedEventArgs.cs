using Avalonia.Interactivity;

namespace FlutnetUI.Ext
{
    public class HtmlRendererRoutedEventArgs<T> : RoutedEventArgs
    {
        public T Event { get; set; }
    }
}
