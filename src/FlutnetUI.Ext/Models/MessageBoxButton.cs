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