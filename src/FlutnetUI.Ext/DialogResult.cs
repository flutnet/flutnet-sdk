namespace FlutnetUI.Ext
{
    /// <summary>
    /// Specifies identifiers to indicate the return value of a dialog box.
    /// </summary>
    public enum DialogResult
    {
        /// <summary>
        /// Nothing is returned from the dialog box.
        /// This means that the modal dialog continues running.
        /// </summary>
        None = 0,

        /// <summary>
        /// The dialog box return value is OK (usually sent from a button labeled OK).
        /// </summary>
        OK = 1,

        /// <summary>
        /// The dialog box return value is Cancel (usually sent from a button labeled Cancel).
        /// </summary>
        Cancel = 2,

        /// <summary>
        /// The dialog box return value is Abort (usually sent from a button labeled Abort).
        /// </summary>
        Abort = 3,

        /// <summary>
        /// The dialog box return value is Retry (usually sent from a button labeled Retry).
        /// </summary>
        Retry = 4,

        /// <summary>
        /// The dialog box return value is Ignore (usually sent from a button labeled Ignore).
        /// </summary>
        Ignore = 5,

        /// <summary>
        /// The dialog box return value is Yes (usually sent from a button labeled Yes).
        /// </summary>
        Yes = 6,

        /// <summary>
        /// The dialog box return value is No (usually sent from a button labeled No).
        /// </summary>
        No = 7,

        /// <summary>
        /// The dialog box return value is Custom1 (usually sent from the first button of a dialog box containing custom buttons).
        /// </summary>
        Custom1 = 8,

        /// <summary>
        /// The dialog box return value is Custom2 (usually sent from the second button of a dialog box containing custom buttons).
        /// </summary>
        Custom2 = 9,

        /// <summary>
        /// The dialog box return value is Custom3 (usually sent from the third button of a dialog box containing custom buttons).
        /// </summary>
        Custom3 = 10
    }
}