using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Media;
using FlutnetUI.Models;
using FlutnetUI.Utilities;
using ReactiveUI;
using static FlutnetUI.Utilities.NamingConventions;

namespace FlutnetUI.ViewModels
{
    public class ConfigureAppViewModel : WizardPageViewModel
    {
        public ConfigureAppViewModel(NewProjectViewModel screen = null) : base("newprj_app", screen)
        {
            Title = "New Project";
            Description = "Follow these steps to generate the Xamarin solution: specify your app name, organization and supported platforms.\nFurther settings in the \"Next\" section.";
            IsDescriptionVisible = true;

            BackVisible = false;

            TrialMode = true;
            screen?.WhenAnyValue(p => p.TrialMode).BindTo(this, p => p.TrialMode);

            this.WhenAnyValue(t => t.AppName, t => t.OrganizationId, t => t.TargetAndroid, t => t.TargetIos,
                              (app, org, target1, target2) => !string.IsNullOrWhiteSpace(app) && !string.IsNullOrWhiteSpace(org) && (target1 || target2))
                .BindTo(this, t => t.NextEnabled);

            this.WhenAnyValue(t => t.AppName, t => t.OrganizationId, BuildAndroidAppId)
                .ToProperty(this, t => t.AndroidAppId, out _androidAppId, initialValue: BuildAndroidAppId(AppName, OrganizationId));

            this.WhenAnyValue(t => t.AppName, t => t.OrganizationId, BuildIosAppId)
                .ToProperty(this, t => t.IosAppId, out _iosAppId, initialValue: BuildIosAppId(AppName, OrganizationId));

            // Command to check if app can target Android / iOS
            ReactiveCommand<Unit, Unit> checkPlatformRequirementsCommand = ReactiveCommand.CreateFromTask(() => Task.Run(() =>
            {
                // Check iOS support
                if (OperatingSystem.IsWindows())
                {
                    CanTargetIos = false;
                    SetPlatformMessage(Platform.iOS, PlatformMessageType.Normal, "Flutter iOS projects are supported on macOS only.");
                }

                // Check Android SDK installation
                bool canTargetAndroid = AndroidSdkLocator.TryLocate(out _);

                CanTargetAndroid = canTargetAndroid;
                if (!canTargetAndroid)
                    SetPlatformMessage(Platform.Android, PlatformMessageType.Normal, "Android SDK not detected.");

                TargetIos = CanTargetIos;
                TargetAndroid = CanTargetAndroid;
            }));

            checkPlatformRequirementsCommand.Execute();
        }

        public override IRoutableViewModel GetNextPage()
        {
            return new ConfigureProjectViewModel(BuildSettings(), HostScreen);
        }

        public bool TrialMode
        {
            get => _trialMode;
            set => this.RaiseAndSetIfChanged(ref _trialMode, value);
        }
        bool _trialMode;

        public string AppName
        {
            get => _appName;
            set => this.RaiseAndSetIfChanged(ref _appName, value);
        }
        string _appName = DefaultAppName;

        public string OrganizationId
        {
            get => _organizationId;
            set => this.RaiseAndSetIfChanged(ref _organizationId, value);
        }
        string _organizationId = DefaultOrganizationId;

        /// <summary>
        /// A proper Android application ID (https://developer.android.com/studio/build/application-id)
        /// built upon the app name and organization identifier provided by the user.
        /// </summary>
        public string AndroidAppId => _androidAppId.Value;
        ObservableAsPropertyHelper<string> _androidAppId;

        /// <summary>
        /// A proper iOS bundle ID (https://developer.apple.com/documentation/bundleresources/information_property_list/cfbundleidentifier)
        /// built upon the app name and organization identifier provided by the user.
        /// </summary>
        ObservableAsPropertyHelper<string> _iosAppId;
        public string IosAppId => _iosAppId.Value;

        /// <summary>
        /// Indicates whether a Xamarin.Android application should be created.
        /// </summary>
        public bool TargetAndroid
        {
            get => _targetAndroid;
            set => this.RaiseAndSetIfChanged(ref _targetAndroid, value);
        }
        bool _targetAndroid = true;

        public bool CanTargetAndroid
        {
            get => _canTargetAndroid;
            private set => this.RaiseAndSetIfChanged(ref _canTargetAndroid, value);
        }
        bool _canTargetAndroid = true;

        /// <summary>
        /// Indicates whether a Xamarin.iOS application should be created.
        /// </summary>
        public bool TargetIos
        {         
            get => _targetIos;
            set => this.RaiseAndSetIfChanged(ref _targetIos, value);
        }
        bool _targetIos = Utilities.OperatingSystem.IsMacOS();

        public bool CanTargetIos
        {
            get => _canTargetIos;
            private set => this.RaiseAndSetIfChanged(ref _canTargetIos, value);
        }
        bool _canTargetIos = Utilities.OperatingSystem.IsMacOS();

        private FlutnetAppSettings BuildSettings()
        {
            FlutnetAppSettings settings = new FlutnetAppSettings
            {
                AppName = AppName,
                OrganizationId = OrganizationId,
                AndroidAppId = AndroidAppId,
                IosAppId = IosAppId,
                TargetAndroid = TargetAndroid,
                TargetIos = TargetIos
            };
            return settings;
        }

        public string AndroidPlatformMessage
        {
            get => _androidPlatformMessage;
            private set => this.RaiseAndSetIfChanged(ref _androidPlatformMessage, value);
        }
        string _androidPlatformMessage = string.Empty;

        public IBrush AndroidPlatformMessageColor
        {
            get => _androidPlatformMessageColor;
            private set => this.RaiseAndSetIfChanged(ref _androidPlatformMessageColor, value);
        }
        IBrush _androidPlatformMessageColor = Brushes.White;

        public string IosPlatformMessage
        {
            get => _iosPlatformMessage;
            private set => this.RaiseAndSetIfChanged(ref _iosPlatformMessage, value);
        }
        string _iosPlatformMessage = string.Empty;

        public IBrush IosPlatformMessageColor
        {
            get => _iosPlatformMessageColor;
            private set => this.RaiseAndSetIfChanged(ref _iosPlatformMessageColor, value);
        }
        IBrush _iosPlatformMessageColor = Brushes.White;

        private void ResetPlatformMessages()
        {
            AndroidPlatformMessage = string.Empty;
            AndroidPlatformMessageColor = Brushes.White;
            IosPlatformMessage = string.Empty;
            IosPlatformMessageColor = Brushes.White;
        }

        private void SetPlatformMessage(Platform platform, PlatformMessageType messageType, string message)
        {
            switch (platform)
            {
                case Platform.Android:
                    AndroidPlatformMessage = message;
                    switch (messageType)
                    {
                        case PlatformMessageType.Normal:
                            AndroidPlatformMessageColor = Brushes.DarkGray;
                            break;
                        case PlatformMessageType.Warning:
                            AndroidPlatformMessageColor = Brushes.Yellow;
                            break;
                        case PlatformMessageType.Error:
                            AndroidPlatformMessageColor = Brushes.DarkRed;
                            break;
                    }
                    break;

                case Platform.iOS:
                    IosPlatformMessage = message;
                    switch (messageType)
                    {
                        case PlatformMessageType.Normal:
                            IosPlatformMessageColor = Brushes.DarkGray;
                            break;
                        case PlatformMessageType.Warning:
                            IosPlatformMessageColor = Brushes.Yellow;
                            break;
                        case PlatformMessageType.Error:
                            IosPlatformMessageColor = Brushes.DarkRed;
                            break;
                    }
                    break;
            }
        }

        enum Platform
        {
            Android,
            iOS
        }

        enum PlatformMessageType
        {
            Normal,
            Warning,
            Error
        }
    }
}