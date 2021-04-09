using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using FlutnetUI.ViewModels;
using ReactiveUI;

namespace FlutnetUI.Views
{
    public class SetupWelcomeView : ReactiveUserControl<SetupWelcomeViewModel>
    {
        public SetupWelcomeView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.WhenActivated(disposables => { });
            AvaloniaXamlLoader.Load(this);
        }
    }
}