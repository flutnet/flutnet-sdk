using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using FlutnetUI.ViewModels;

namespace FlutnetUI.Views
{
    public class SetupCheckFlutterErrorView : ReactiveUserControl<SetupCheckFlutterErrorViewModel>
    {
        public SetupCheckFlutterErrorView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}