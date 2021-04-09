using ReactiveUI;

namespace FlutnetUI.ViewModels
{
    public class MainWindowViewModel : ReactiveObject, IScreen
    {
        public MainWindowViewModel()
        {
            Router.Navigate.Execute(new MainPageViewModel(this));
        }

        #region IScreen implementation

        // The Router associated with this Screen.
        public RoutingState Router { get; } = new RoutingState();

        #endregion
    }
}