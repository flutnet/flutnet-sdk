using System;
using System.Diagnostics;
using System.Reactive.Concurrency;
using ReactiveUI;

namespace FlutnetUI.Utilities
{
    public class RxUnhandledExceptionObserver : IObserver<Exception>
    {
        public void OnNext(Exception value)
        {
            if (Debugger.IsAttached) Debugger.Break();

            //Analytics.Current.TrackEvent("MyRxHandler", new Dictionary<string, string>()
            //{
            //    {"Type", value.GetType().ToString()},
            //    {"Message", value.Message},
            //});

            Log.Ex(value);
            RxApp.MainThreadScheduler.Schedule(() => { throw value; });
        }

        public void OnError(Exception error)
        {
            if (Debugger.IsAttached) Debugger.Break();

            //Analytics.Current.TrackEvent("MyRxHandler Error", new Dictionary<string, string>()
            //{
            //    {"Type", error.GetType().ToString()},
            //    {"Message", error.Message},
            //});

            Log.Ex(error);
            RxApp.MainThreadScheduler.Schedule(() => { throw error; });
        }

        public void OnCompleted()
        {
            if (Debugger.IsAttached) Debugger.Break();

            RxApp.MainThreadScheduler.Schedule(() => { throw new NotImplementedException(); });
        }
    }
}