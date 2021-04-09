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