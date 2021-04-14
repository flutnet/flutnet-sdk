// Copyright (c) 2020-2021 Novagem Solutions S.r.l.
//
// This file is part of Flutnet.
//
// Flutnet is a free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// Flutnet is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY, without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with Flutnet.  If not, see <http://www.gnu.org/licenses/>.

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