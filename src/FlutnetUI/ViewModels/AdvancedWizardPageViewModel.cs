using System.Reactive;
using ReactiveUI;

namespace FlutnetUI.ViewModels
{
    public abstract class AdvancedWizardPageViewModel : PageViewModel, IAdvancedWizardPageViewModel
    {
        protected AdvancedWizardPageViewModel(string path, IScreen screen = null) : base(path, screen)
        {
            _button2Command = screen?.Router.NavigateBack;
        }

        public string Button1Text
        {
            get => _button1Text;
            protected set => this.RaiseAndSetIfChanged(ref _button1Text, value);
        }
        string _button1Text = "Cancel";

        public bool Button1Visible
        {
            get => _button1Visible;
            protected set => this.RaiseAndSetIfChanged(ref _button1Visible, value);
        }
        bool _button1Visible = false;

        public string Button2Text
        {
            get => _button2Text;
            protected set => this.RaiseAndSetIfChanged(ref _button2Text, value);
        }
        string _button2Text = "Back";

        public bool Button2Visible
        {
            get => _button2Visible;
            protected set => this.RaiseAndSetIfChanged(ref _button2Visible, value);
        }
        bool _button2Visible = false;

        public string Button3Text
        {
            get => _button3Text;
            protected set => this.RaiseAndSetIfChanged(ref _button3Text, value);
        }
        string _button3Text = "Next";

        public bool Button3Visible
        {
            get => _button3Visible;
            protected set => this.RaiseAndSetIfChanged(ref _button3Visible, value);
        }
        bool _button3Visible = true;

        public bool Button3Enabled
        {
            get => _button3Enabled;
            protected set => this.RaiseAndSetIfChanged(ref _button3Enabled, value);
        }
        bool _button3Enabled = true;

        public ReactiveCommand<Unit, Unit> Button1Command
        {
            get => _button1Command;
            protected set => this.RaiseAndSetIfChanged(ref _button1Command, value);
        }
        ReactiveCommand<Unit, Unit> _button1Command;

        public ReactiveCommand<Unit, Unit> Button2Command
        {
            get => _button2Command;
            protected set => this.RaiseAndSetIfChanged(ref _button2Command, value);
        }
        ReactiveCommand<Unit, Unit> _button2Command;

        public ReactiveCommand<Unit, Unit> Button3Command
        {
            get => _button3Command;
            protected set => this.RaiseAndSetIfChanged(ref _button3Command, value);
        }
        ReactiveCommand<Unit, Unit> _button3Command;

        public bool IsFinishPage
        {
            get => _isFinishPage;
            protected set => this.RaiseAndSetIfChanged(ref _isFinishPage, value);
        }
        bool _isFinishPage;
    }
}