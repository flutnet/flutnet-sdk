using System;
using ReactiveUI;
using Splat;

namespace FlutnetUI.ViewModels
{
    public abstract class PageViewModel : ReactiveObject, IPageViewModel
    {
        protected PageViewModel(string path, IScreen screen = null)
        {
            HostScreen = screen ?? Locator.Current.GetService<IScreen>();
            UrlPathSegment = path;
            _title = path;
        }

        #region IRoutableViewModel implementation

        // Reference to IScreen that owns the routable view model.
        public IScreen HostScreen { get; }

        // Unique identifier for the routable view model.
        public string UrlPathSegment { get; }

        #endregion

        public string Title
        {
            get => _title;
            protected set => this.RaiseAndSetIfChanged(ref _title, value);
        }
        string _title;

        public string Description
        {
            get => _description;
            protected set => this.RaiseAndSetIfChanged(ref _description, value);
        }
        string _description = string.Empty;

        public bool IsDescriptionVisible
        {
            get => _isDescriptionVisible;
            protected set => this.RaiseAndSetIfChanged(ref _isDescriptionVisible, value);
        }
        bool _isDescriptionVisible;

        public bool IsBusy
        {
            get => _isBusy;
            protected set => this.RaiseAndSetIfChanged(ref _isBusy, value);
        }
        bool _isBusy;
    }
}