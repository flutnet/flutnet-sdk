namespace FlutnetUI.Ext
{
    /// <summary>
    /// Specifies constants defining which buttons to display on a MessageBox.
    /// </summary>
    public enum MessageBoxButtons
    {
        /// <summary>
        /// The message box contains an OK button.
        /// </summary>
        OK = 0,

        /// <summary>
        /// The message box contains OK and Cancel buttons.
        /// </summary>
        OKCancel = 1,

        /// <summary>
        /// The message box contains Abort, Retry, and Ignore buttons.
        /// </summary>
        AbortRetryIgnore = 2,

        /// <summary>
        /// The message box contains Yes, No, and Cancel buttons.
        /// </summary>
        YesNoCancel = 3,

        /// <summary>
        /// The message box contains Yes and No buttons.
        /// </summary>
        YesNo = 4,

        /// <summary>
        /// The message box contains Retry and Cancel buttons.
        /// </summary>
        RetryCancel = 5,

        /// <summary>
        /// The message box contains a custom button.
        /// </summary>
        Custom1 = 6,

        /// <summary>
        /// The message box contains two custom buttons.
        /// </summary>
        Custom1Custom2 = 7,

        /// <summary>
        /// The message box contains three custom buttons.
        /// </summary>
        Custom1Custom2Custom3 = 8
    }
}