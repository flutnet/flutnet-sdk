using System.Reactive;
using System.Reactive.Disposables;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input.Platform;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Flutnet.Cli.DTO;
using FlutnetUI.Ext;
using FlutnetUI.Utilities;
using FlutnetUI.ViewModels;
using ReactiveUI;

namespace FlutnetUI.Views
{
    public class LicensingView : ReactiveUserControl<LicensingViewModel>
    {
        Panel infoPanel;
        Panel notLicensedPanel;
        TextBlock notLicensedText;
        DrawingPresenter notLicensedIcon;

        public LicensingView()
        {
            InitializeComponent();

            infoPanel = this.Find<Panel>("InfoPanel");
            notLicensedPanel = this.Find<Panel>("NotLicensedPanel");
            notLicensedText = this.Find<TextBlock>("NotLicensedText");
            notLicensedIcon = this.Find<DrawingPresenter>("NotLicensedIcon");
        }

        private void InitializeComponent()
        {
            this.WhenActivated(disposables =>
            {
                if (ViewModel == null)
                    return;

                ViewModel.WhenAnyValue(t => t.LicenseInfo, t => t.NeedsActivation, (info, b) => info.LicenseStatus == LicenseStatus.Licensed && !b)
                    .BindTo(infoPanel, p => p.IsVisible)
                    .DisposeWith(disposables);

                ViewModel.WhenAnyValue(t => t.LicenseInfo, t => t.NeedsActivation, (info, b) => info.LicenseStatus != LicenseStatus.Licensed || b)
                    .BindTo(notLicensedPanel, p => p.IsVisible)
                    .DisposeWith(disposables);

                ViewModel.WhenAnyValue(t => t.LicenseInfo, t => t.IsChangeProductKeyRequested, t => t.IsRenewalRequested, selector: (info, b1, b2) =>
                    {
                        if (b1)
                            return "Please enter your new Product Key below and press \"Activate\".";

                        if (b2)
                            return "Press \"Activate\" to extend your license period (a license upgrade must have been purchased).";

                        if (info.LoadError)
                            return info.LoadErrorMessage;

                        switch (info.LicenseStatus)
                        {
                            case LicenseStatus.Trial:
                                return "No license file detected, software is running in trial mode. If you already purchased a license, please enter the Product Key below and press \"Activate\".";

                            case LicenseStatus.Invalid:
                                switch (info.InvalidReasons)
                                {
                                    case LicenseInvalidReasons.UnsupportedSoftwareRelease:
                                        return "Your license is not compatible with this software release.";

                                    default:
                                        return "Invalid license file detected, software is running in trial mode.";
                                }

                            case LicenseStatus.Expired:
                                return "Your Flutnet license has expired.\nPress \"Activate\" to extend your license period (a license upgrade must have been purchased).";

                            default:
                                return string.Empty;
                        }
                    })
                    .BindTo(notLicensedText, t => t.Text)
                    .DisposeWith(disposables);

                ViewModel.WhenAnyValue(t => t.IsChangeProductKeyRequested, t => t.IsRenewalRequested, (b1, b2) => !b1 && !b2)
                    .BindTo(notLicensedIcon, t => t.IsVisible)
                    .DisposeWith(disposables);

                ViewModel
                    .ConfirmChangeProductKey
                    .RegisterHandler(async interaction =>
                    {
                        Window window = Application.Current.GetMainWindow();
                        string message = $"Are you sure you want to change the product key?{System.Environment.NewLine}The current license will be overwritten.";
                        DialogResult result = await MessageBox.Show(window, message, "Warning", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2).ConfigureAwait(false);
                        interaction.SetOutput(result == DialogResult.OK);
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
            });
            AvaloniaXamlLoader.Load(this);
        }
    }
}