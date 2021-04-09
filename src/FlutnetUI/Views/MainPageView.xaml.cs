using System.Reactive;
using System.Reactive.Disposables;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using FlutnetUI.Utilities;
using FlutnetUI.ViewModels;
using ReactiveUI;
using Splat;

namespace FlutnetUI.Views
{
    public class MainPageView : ReactiveUserControl<MainPageViewModel>
    {
        public MainPageView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.WhenActivated(disposables => 
            {
                if (ViewModel == null)
                    return;

                ViewModel
                    .ShowSetupWizard
                    .RegisterHandler(async interaction =>
                    {
                        Window window = Application.Current.GetMainWindow();
                        Window wizard = (Window)Locator.Current.GetService<IViewFor<SetupWindowViewModel>>();
                        wizard.DataContext = new SetupWindowViewModel();
                        wizard.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                        await wizard.ShowDialog(window);
                        interaction.SetOutput(Unit.Default);
                    })
                    .DisposeWith(disposables);
            });
            AvaloniaXamlLoader.Load(this);
        }
    }
}