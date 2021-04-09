using System;
using System.Reactive;
using System.Reactive.Linq;
using Flutnet.Cli.DTO;
using FlutnetUI.Models;
using FlutnetUI.Utilities;
using ReactiveUI;

namespace FlutnetUI.ViewModels
{
    public class LicensingViewModel : PageViewModel
    {
        public LicensingViewModel(LicenseInfo currentLicense, IScreen screen = null) : base("licensing", screen)
        {
            Title = "Licensing";
            
            Description = "Insert your Flutnet License Key and after that go in \"App Keys\" section to handle your Android and iOS application Keys.";
            IsDescriptionVisible = true;

            _licenseInfo = currentLicense;
            _productKey = currentLicense.ProductKey;

            this.WhenAnyValue(
                    t => t.LicenseInfo, t => t.IsRenewalRequested, t => t.IsChangeProductKeyRequested,
                    (info, b1, b2) => b1 || b2 || info.LicenseStatus != LicenseStatus.Licensed)
                .ToProperty(this, t => t.NeedsActivation, out _needsActivation, initialValue: currentLicense.LicenseStatus != LicenseStatus.Licensed);

            this.WhenAnyValue(t => t.IsRenewalRequested, t => t.IsChangeProductKeyRequested, (b1, b2) => b1 || b2)
                .ToProperty(this, t => t.IsActivationCancelable, out _isActivationCancelable);

            // Create the command that calls Flutnet CLI 
            // for activating a new Flutnet license (or renewing an existing one)
            ActivateLicense = ReactiveCommand.CreateFromTask(async ct =>
            {
                ActivationInArg args = new ActivationInArg {ProductKey = ProductKey};
                CommandLineCallResult callResult = await CommandLineTools.Call<ActivationOutArg>(args, ct);

                if (callResult.Canceled || callResult.Failed)
                {
                    return new LicenseInfo
                    {
                        LoadError = true,
                        LoadErrorMessage = "Unable to complete activation, please retry."
                    };
                }

                ActivationOutArg result = (ActivationOutArg) callResult.CommandResult;
                if (!result.Success)
                {
                    return new LicenseInfo
                    {
                        LoadError = true,
                        LoadErrorMessage = result.ErrorMessage
                    };
                }

                return new LicenseInfo
                {
                    ProductKey = args.ProductKey,
                    LicenseStatus = result.LicenseStatus,
                    LicenseOwner = result.LicenseOwner,
                    LicenseType = result.LicenseType
                };
            });

            ActivateLicense.IsExecuting.ToProperty(this, x => x.IsActivating, out _isActivating);

            ActivateLicense.Where(result => result.LoadError).Subscribe(async result =>
            {
                await ShowError.Handle(result.LoadErrorMessage);
            });
            ActivateLicense.Where(result => !result.LoadError).BindTo(this, x => x.LicenseInfo);

            RenewLicense = ReactiveCommand.Create(() => { IsRenewalRequested = true; });

            ChangeProductKey = ReactiveCommand.CreateFromTask(async () =>
            {
                bool confirm = await ConfirmChangeProductKey.Handle(Unit.Default);
                if (confirm)
                {
                    IsChangeProductKeyRequested = true;
                    ProductKey = string.Empty;
                }
            });

            Cancel = ReactiveCommand.Create(() =>
            {
                IsRenewalRequested = false;
                IsChangeProductKeyRequested = false;
            }, canExecute: this.WhenAnyValue(t => t.IsActivating, b => !b));
        }

        public LicenseInfo LicenseInfo
        {
            get => _licenseInfo;
            private set => this.RaiseAndSetIfChanged(ref _licenseInfo, value);
        }
        LicenseInfo _licenseInfo;

        public string ProductKey
        {
            get => _productKey;
            set => this.RaiseAndSetIfChanged(ref _productKey, value);
        }
        string _productKey;

        public ReactiveCommand<Unit, Unit> RenewLicense { get; }

        public bool IsRenewalRequested
        {
            get => _isRenewalRequested;
            private set => this.RaiseAndSetIfChanged(ref _isRenewalRequested, value);
        }
        bool _isRenewalRequested;

        public ReactiveCommand<Unit, Unit> ChangeProductKey { get; }

        public Interaction<Unit, bool> ConfirmChangeProductKey { get; } = new Interaction<Unit, bool>();

        public bool IsChangeProductKeyRequested
        {
            get => _isChangeProductKeyRequested;
            private set => this.RaiseAndSetIfChanged(ref _isChangeProductKeyRequested, value);
        }
        bool _isChangeProductKeyRequested;

        public ReactiveCommand<Unit, LicenseInfo> ActivateLicense { get; }

        public bool IsActivating => _isActivating.Value;
        ObservableAsPropertyHelper<bool> _isActivating;

        public bool NeedsActivation => _needsActivation.Value;
        ObservableAsPropertyHelper<bool> _needsActivation;

        public bool IsActivationCancelable => _isActivationCancelable.Value;
        ObservableAsPropertyHelper<bool> _isActivationCancelable;

        public ReactiveCommand<Unit, Unit> Cancel { get; }

        public Interaction<string, Unit> ShowError { get; } = new Interaction<string, Unit>();
    }
}
