namespace FlutnetUI.Ext
{
    /// <summary>
    /// Specifies constants defining which information to display.
    /// </summary>
    public enum MessageBoxIcon
    {
        /// <summary>
        /// The message box contains no symbols.
        /// </summary>
        None = 0x00,

        /// <summary>
        /// The message box contains a symbol consisting of white X in a circle with a red background.
        /// </summary>
        Hand = 0x10,

        /// <summary>
        /// The message box contains a symbol consisting of a question mark in a circle.
        /// The question mark message icon is no longer recommended because it does not clearly represent
        /// a specific type of message and because the phrasing of a message as a question could apply to any message type.
        /// In addition, users can confuse the question mark symbol with a help information symbol.
        /// Therefore, do not use this question mark symbol in your message boxes.
        /// The system continues to support its inclusion only for backward compatibility.
        /// </summary>
        Question = 0x20,

        /// <summary>
        /// The message box contains a symbol consisting of an exclamation point in a triangle with a yellow background.
        /// </summary>
        Exclamation = 0x30,

        /// <summary>
        /// The message box contains a symbol consisting of a lowercase letter i in a circle.
        /// </summary>
        Asterisk = 0x40,

        /// <summary>
        /// The message box contains a symbol consisting of white X in a circle with a red background.
        /// </summary>
        Stop = Hand,

        /// <summary>
        /// The message box contains a symbol consisting of white X in a circle with a red background.
        /// </summary>
        Error = Hand,

        /// <summary>
        /// The message box contains a symbol consisting of an exclamation point in a triangle with a yellow background.
        /// </summary>
        Warning = Exclamation,

        /// <summary>
        /// The message box contains a symbol consisting of a lowercase letter i in a circle.
        /// </summary>
        Information = Asterisk,
    }
}