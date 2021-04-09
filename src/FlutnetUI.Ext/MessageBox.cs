using System.Threading.Tasks;
using Avalonia.Controls;
using FlutnetUI.Ext.ViewModels;
using FlutnetUI.Ext.Views;

namespace FlutnetUI.Ext
{
    public static class MessageBox
    {
        /// <summary>
        /// Displays a message box in front of the specified window and with specified text, caption, buttons, icon, default button and options.
        /// </summary>
        public static Task<DialogResult> Show(Window owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton, MessageBoxOptions options)
        {
            return ShowCore(owner, text, caption, buttons, icon, defaultButton, options);
        }

        /// <summary>
        /// Displays a message box in front of the specified window and with specified text, caption, buttons, icon and default button.
        /// </summary>
        public static Task<DialogResult> Show(Window owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton)
        {
            return ShowCore(owner, text, caption, buttons, icon, defaultButton, MessageBoxOptions.Default);
        }

        /// <summary>
        /// Displays a message box in front of the specified window and with specified text, caption, buttons and icon.
        /// </summary>
        public static Task<DialogResult> Show(Window owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            return ShowCore(owner, text, caption, buttons, icon, MessageBoxDefaultButton.Button1, MessageBoxOptions.Default);
        }

        /// <summary>
        /// Displays a message box in front of the specified window and with specified text, caption and buttons.
        /// </summary>
        public static Task<DialogResult> Show(Window owner, string text, string caption, MessageBoxButtons buttons)
        {
            return ShowCore(owner, text, caption, buttons, MessageBoxIcon.None, MessageBoxDefaultButton.Button1, MessageBoxOptions.Default);
        }

        /// <summary>
        /// Displays a message box in front of the specified window and with specified text and caption.
        /// </summary>
        public static Task<DialogResult> Show(Window owner, string text, string caption)
        {
            return ShowCore(owner, text, caption, MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1, MessageBoxOptions.Default);
        }

        /// <summary>
        /// Displays a message box in front of the specified window and with specified text.
        /// </summary>
        public static Task<DialogResult> Show(Window owner, string text)
        {
            return ShowCore(owner, text, string.Empty, MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1, MessageBoxOptions.Default);
        }

        /// <summary>
        /// Displays a message box with specified text, caption, buttons, icon, default button and options.
        /// </summary>
        public static Task<DialogResult> Show(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton, MessageBoxOptions options)
        {
            return ShowCore(null, text, caption, buttons, icon, defaultButton, options);
        }

        /// <summary>
        /// Displays a message box with specified text, caption, buttons, icon and default button.
        /// </summary>
        public static Task<DialogResult> Show(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton)
        {
            return ShowCore(null, text, caption, buttons, icon, defaultButton, MessageBoxOptions.Default);
        }

        /// <summary>
        /// Displays a message box with specified text, caption, buttons and icon.
        /// </summary>
        public static Task<DialogResult> Show(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            return ShowCore(null, text, caption, buttons, icon, MessageBoxDefaultButton.Button1, MessageBoxOptions.Default);
        }

        /// <summary>
        /// Displays a message box with specified text, caption and buttons.
        /// </summary>
        public static Task<DialogResult> Show(string text, string caption, MessageBoxButtons buttons)
        {
            return ShowCore(null, text, caption, buttons, MessageBoxIcon.None, MessageBoxDefaultButton.Button1, MessageBoxOptions.Default);
        }

        /// <summary>
        /// Displays a message box with specified text and caption.
        /// </summary>
        public static Task<DialogResult> Show(string text, string caption)
        {
            return ShowCore(null, text, caption, MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1, MessageBoxOptions.Default);
        }

        /// <summary>
        /// Displays a message box with specified text.
        /// </summary>
        public static Task<DialogResult> Show(string text)
        {
            return ShowCore(null, text, string.Empty, MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1, MessageBoxOptions.Default);
        }

        private static Task<DialogResult> ShowCore(Window owner, string text, string caption, MessageBoxButtons buttons, 
            MessageBoxIcon icon, 
            MessageBoxDefaultButton defaultButton,
            MessageBoxOptions options)
        {
            MessageBoxView window = new MessageBoxView(owner, options);
            window.DataContext = new MessageBoxViewModel(text, caption, buttons, window.GetIconDrawing(icon), defaultButton, options);
            return owner != null ? window.ShowDialog(owner) : window.Show();
        }
    }
}