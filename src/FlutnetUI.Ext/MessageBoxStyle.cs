using System;
using System.Collections;
using System.Reflection;

namespace FlutnetUI.Ext
{
    public enum MessageBoxStyle
    {
        None,
        [MessageBoxStyle("avares://FlutnetUI.Ext/Styles/Windows/Windows.xaml")]
        Windows,
        [MessageBoxStyle("avares://FlutnetUI.Ext/Styles/MacOs/MacOs.xaml")]
        MacOs,
        [MessageBoxStyle("avares://FlutnetUI.Ext/Styles/Ubuntu/Ubuntu.xaml")]
        UbuntuLinux,
        [MessageBoxStyle("avares://FlutnetUI.Ext/Styles/Mint/Mint.xaml")]
        MintLinux,
        [MessageBoxStyle("avares://FlutnetUI.Ext/Styles/Dark/Dark.xaml")]
        DarkMode,
        [MessageBoxStyle("avares://FlutnetUI.Ext/Styles/RoundButtons/RoundButtons.xaml")]
        RoundButtons,
        Custom
    }

    internal class MessageBoxStyleAttribute : Attribute
    {
        public MessageBoxStyleAttribute(string uri)
        {
            Uri = uri;
        }

        public string Uri { get; set; }
    }

    internal static class MessageBoxStyleExtensions
    {
        static readonly Hashtable Cache = new Hashtable();

        static MessageBoxStyleExtensions()
        {
            foreach (MessageBoxStyle style in Enum.GetValues(typeof(MessageBoxStyle)))
                Cache.Add(style, GetAttribute(style));
        }

        /// <summary>
        /// Returns all <see cref="MessageBoxStyle"/> values.
        /// </summary>
        public static ICollection GetValues()
        {
            return Cache.Keys;
        }

        public static string GetUri(this MessageBoxStyle style)
        {
            MessageBoxStyleAttribute attribute = (MessageBoxStyleAttribute) Cache[style];
            return attribute?.Uri;
        }

        private static MessageBoxStyleAttribute GetAttribute(MessageBoxStyle style)
        {
            return ForValue(style).GetCustomAttribute<MessageBoxStyleAttribute>();
            //return (MessageBoxStyleAttribute) Attribute.GetCustomAttribute(ForValue(style), typeof(MessageBoxStyleAttribute));
        }

        private static MemberInfo ForValue(MessageBoxStyle style)
        {
            return typeof(MessageBoxStyle).GetField(Enum.GetName(typeof(MessageBoxStyle), style));
        }
    }
}