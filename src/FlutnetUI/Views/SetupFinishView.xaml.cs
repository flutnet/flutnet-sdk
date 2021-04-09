using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using FlutnetUI.ViewModels;

namespace FlutnetUI.Views
{
    public class SetupFinishView : ReactiveUserControl<SetupFinishViewModel>
    {
        public SetupFinishView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}