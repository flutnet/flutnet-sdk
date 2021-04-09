using System;

namespace FlutnetUI.Ext
{
    /// <summary>
    /// Flags for defining the accent (colored) buttons on a MessageBox.
    /// </summary>
    [Flags]
    public enum MessageBoxAccentButtons
    {
        /// <summary>
        /// No button on the message box has accent.
        /// </summary>
        None = 0,

        /// <summary>
        /// The first button on the message box has accent.
        /// </summary>
        Button1 = 1,

        /// <summary>
        /// The second button on the message box has accent.
        /// </summary>
        Button2 = 2,

        /// <summary>
        /// The third button on the message box has accent.
        /// </summary>
        Button3 = 4
    }
}