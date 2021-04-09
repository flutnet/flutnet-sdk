using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using FlutnetUI.Ext;
using FlutnetUI.ViewModels;
using ReactiveUI;

namespace FlutnetUI.Views
{
    public class SetupWindow : ReactiveWindow<SetupWindowViewModel>
    {
        public SetupWindow()
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
                    .CloseWizard
                    .RegisterHandler(interaction =>
                    {
                        interaction.SetOutput(Unit.Default);
                        Close();
                    })
                    .DisposeWith(disposables);

                ViewModel
                    .ShowError
                    .RegisterHandler(async interaction =>
                    {
                        await MessageBox.Show(this, "An unexpected error occured, please retry.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error).ConfigureAwait(false);
                        interaction.SetOutput(Unit.Default);
                    })
                    .DisposeWith(disposables);
            });
            AvaloniaXamlLoader.Load(this);
        }
    }
}
