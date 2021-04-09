using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;

namespace FlutnetUI.ViewModels
{
    /// <summary>
    /// Denotes a ViewModel that represents a generic page with a title.
    /// </summary>
    interface IPageViewModel : IRoutableViewModel
    {
        string Title { get; }

        string Description { get; }

        bool IsDescriptionVisible { get; }

        bool IsBusy { get; }
    }

    /// <summary>
    /// Denotes a ViewModel that represents a wizard page with a title and navigation buttons.
    /// </summary>
    interface IWizardPageViewModel : IPageViewModel
    {
        string CancelText { get; }
        bool CancelVisible { get; }

        string BackText { get; }
        bool BackVisible { get; }

        string NextText { get; }
        bool NextEnabled { get; }

        Task OnNext();
        bool IsFinishPage { get; }

        IRoutableViewModel GetNextPage();
    }

    /// <summary>
    /// Denotes a ViewModel that represents a highly customizable wizard page with a title and navigation buttons.
    /// </summary>
    interface IAdvancedWizardPageViewModel : IPageViewModel
    {
        string Button1Text { get; }
        bool Button1Visible { get; }

        string Button2Text { get; }
        bool Button2Visible { get; }

        string Button3Text { get; }
        bool Button3Visible { get; }
        bool Button3Enabled { get; }

        ReactiveCommand<Unit, Unit> Button1Command { get; }
        ReactiveCommand<Unit, Unit> Button2Command { get; }
        ReactiveCommand<Unit, Unit> Button3Command { get; }

        bool IsFinishPage { get; }
    }
}