using System;
using System.Reactive;
using System.Reactive.Disposables;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using FlutnetUI.ViewModels;
using ReactiveUI;

namespace FlutnetUI.Views
{
    public class ConfigureProjectView : ReactiveUserControl<ConfigureProjectViewModel>
    {
        public ConfigureProjectView()
        {
            InitializeComponent();
            TreeView projectTree = this.Find<TreeView>("ProjectTree");
     
        }

        private void InitializeComponent()
        {
            this.WhenActivated(disposables =>
            {
                if (ViewModel == null)
                    return;

                // Execute the command when the View is activated
                ViewModel.CheckFlutterVersion.Execute(Unit.Default).Subscribe().DisposeWith(disposables);
            });
            AvaloniaXamlLoader.Load(this);
        }
    }
}