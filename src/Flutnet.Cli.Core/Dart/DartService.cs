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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Flutnet.Cli.Core.Infrastructure;
using Flutnet.Utilities;

namespace Flutnet.Cli.Core.Dart
{
    internal class DartService
    {
        public DartType Type { get; }
        public List<DartMethod> Methods { get; }
        public List<DartEvent> Events { get; }

        public DartService(ICollection<Type> customTypes, Type type, List<MethodInfo> methods, List<EventInfo> events, string package, Func<MethodInfo, string> getMethodId)
        {
            Type = new DartType(customTypes,type,package);
            Methods = new List<DartMethod>();
            Events = new List<DartEvent>();

            foreach (MethodInfo method in methods)
            {
                Type r = DotNetTools.WrapReturnIntoFakeClass(type, method);
                Type p = DotNetTools.WrapParamsIntoFakeClass(type, method);

                Type[] tmp = {r, p};

                DartType returnType = new DartType(customTypes.Concat(tmp).ToList(), r,package);
                DartType paramType = new DartType(customTypes.Concat(tmp).ToList(), p, package);

                Methods.Add(new DartMethod(customTypes, method, returnType, paramType, package, getMethodId));
            }

            foreach (EventInfo @event in events)
            {
                Type args = @event.GetPlatformEventArgs();
                DartType dartArgs = new DartType(customTypes, args, package);

                Events.Add(new DartEvent(customTypes,@event, dartArgs, package));
            }
        }
    }
}