using System.Reactive;
using System.Reactive.Disposables;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using FlutnetUI.Ext;
using FlutnetUI.Utilities;
using FlutnetUI.ViewModels;
using ReactiveUI;
using Splat;

namespace FlutnetUI.Views
{
    public class NewProjectView : ReactiveUserControl<NewProjectViewModel>
    {
        public NewProjectView()
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
                        Window wizard = (Window) Locator.Current.GetService<IViewFor<SetupWindowViewModel>>();
                        wizard.DataContext = new SetupWindowViewModel();
                        wizard.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                        await wizard.ShowDialog(window);
                        interaction.SetOutput(Unit.Default);
                    })
                    .DisposeWith(disposables);

                ViewModel
                    .ShowError
                    .RegisterHandler(async interaction =>
                    {
                        Window window = Application.Current.GetMainWindow();
                        await MessageBox.Show(window, "An unexpected error occured, please retry.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error).ConfigureAwait(false);
                        interaction.SetOutput(Unit.Default);
                    })
                    .DisposeWith(disposables);
            });
            AvaloniaXamlLoader.Load(this);
        }
    }
}