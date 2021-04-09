using System.Threading.Tasks;
using ReactiveUI;

namespace FlutnetUI.ViewModels
{
    public abstract class WizardPageViewModel : PageViewModel, IWizardPageViewModel
    {
        protected WizardPageViewModel(string path, IScreen screen = null) : base(path, screen)
        {
        }

        public string CancelText
        {
            get => _cancelText;
            protected set => this.RaiseAndSetIfChanged(ref _cancelText, value);
        }
        string _cancelText = "Cancel";

        public bool CancelVisible
        {
            get => _cancelVisible;
            protected set => this.RaiseAndSetIfChanged(ref _cancelVisible, value);
        }
        bool _cancelVisible = true;

        public string BackText
        {
            get => _backText;
            protected set => this.RaiseAndSetIfChanged(ref _backText, value);
        }
        string _backText = "Back";

        public bool BackVisible
        {
            get => _backVisible;
            protected set => this.RaiseAndSetIfChanged(ref _backVisible, value);
        }
        bool _backVisible = true;

        public string NextText
        {
            get => _nextText;
            protected set => this.RaiseAndSetIfChanged(ref _nextText, value);
        }
        string _nextText = "Next";

        public bool NextEnabled
        {
            get => _nextEnabled;
            protected set => this.RaiseAndSetIfChanged(ref _nextEnabled, value);
        }
        bool _nextEnabled = true;



        public virtual Task OnNext()
        {
            return Task.CompletedTask;
        }

        public bool IsFinishPage
        {
            get => _isFinishPage;
            protected set => this.RaiseAndSetIfChanged(ref _isFinishPage, value);
        }
        bool _isFinishPage;

        public virtual IRoutableViewModel GetNextPage()
        {
            return null;
        }
    }
}