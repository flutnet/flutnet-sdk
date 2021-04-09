using System;
using System.Reactive;
using System.Reactive.Disposables;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input.Platform;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using FlutnetUI.Ext;
using FlutnetUI.Utilities;
using FlutnetUI.ViewModels;
using ReactiveUI;

namespace FlutnetUI.Views
{
    public class AppKeysView : ReactiveUserControl<AppKeysViewModel>
    {
        public AppKeysView()
        {
            InitializeComponent();

            if (Design.IsDesignMode)
                return;

            TextBox newAppIdText = this.Find<TextBox>("NewAppId");
            ScrollViewer scrollViewer = this.Find<ScrollViewer>("ScrollViewer");

            PropertyChanged += (sender, args) =>
            {
                if (string.Equals(args.Property.Name, nameof(Bounds)))
                {
                    scrollViewer.MaxHeight = Bounds.Height - Padding.Top - Padding.Bottom - newAppIdText.Height;
                }
            };
        }

        private void InitializeComponent()
        {
            this.WhenActivated(disposables =>
            {
                if (ViewModel == null)
                    return;

                ViewModel
                    .ShowAppKey
                    .RegisterHandler(async interaction =>
                    {
                        Window window = Application.Current.GetMainWindow();
                        string title = $"App Key [{interaction.Input.ApplicationId}]";
                        string message = interaction.Input.AppKey;

                        DialogResult result = await MessageBox.Show(window, message, title, MessageBoxButtons.Custom1Custom2, MessageBoxIcon.None, MessageBoxDefaultButton.Button1, new MessageBoxOptions()
                        {
                            ButtonTexts = new[] { "Copy to Clipboard", "OK" },
                            MaxWidth = 400,
                            MaxHeight = 150
                        }).ConfigureAwait(false);
                        if (result == DialogResult.Custom1)
                            await AvaloniaLocator.Current.GetService<IClipboard>().SetTextAsync(message);
                        interaction.SetOutput(Unit.Default);
                    })
                    .DisposeWith(disposables);

                ViewModel
                    .ShowError
                    .RegisterHandler(async interaction =>
                    {
                        Window window = Application.Current.GetMainWindow();
                        await MessageBox.Show(window, interaction.Input, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error).ConfigureAwait(false);
                        interaction.SetOutput(Unit.Default);
                    })
                    .DisposeWith(disposables);

                ViewModel
                    .ConfirmRemove
                    .RegisterHandler(async interaction =>
                    {
                        Window window = Application.Current.GetMainWindow();
                        string message = $"Are you sure you want to delete the app key for {interaction.Input}?";
                        DialogResult result = await MessageBox.Show(window, message, "Warning", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2).ConfigureAwait(false);
                        interaction.SetOutput(result == DialogResult.OK);
                    })
                    .DisposeWith(disposables);

                // Execute the command when the View is activated
                ViewModel.GetAppKeys.Execute(Unit.Default).Subscribe().DisposeWith(disposables);
            });
            AvaloniaXamlLoader.Load(this);
        }
    }
}