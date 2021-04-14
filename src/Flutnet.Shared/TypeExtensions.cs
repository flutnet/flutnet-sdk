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
using System.Threading.Tasks;
using Flutnet.ServiceModel;

namespace Flutnet.Utilities
{
    internal static class TypeExtensions
    {
        #region System.Type global extensions

        /// <summary>
        /// Gets a valute indicating whether this type is a nullable type.
        /// </summary>
        /// <returns>True if nullable, otherwise False</returns>
        public static bool IsNullable(this Type type, out Type underlyingType)
        {
            underlyingType = Nullable.GetUnderlyingType(type);
            return underlyingType != null;
        }

        /// <summary>
        /// Gets a valute indicating whether this type is a generic type.
        /// </summary>
        /// <returns>True if generic, otherwise False</returns>
        public static bool IsGeneric(this Type type)
        {
            return type.IsGenericType
                   && type.Name.Contains("`"); //TODO: Figure out why IsGenericType isn't good enough and document (or remove) this condition
        }

        /// <summary>
        /// Gets the fully qualified type name of this type.
        /// This will use any keywords in place of types where possible (string instead of System.String for example).
        /// </summary>
        /// <param name="type"></param>
        /// <returns>The fully qualified name for this type</returns>
        public static string GetQualifiedTypeName(this Type type)
        {
            switch (type.Name)
            {
                case nameof(Char):
                    return "char";
                case nameof(String):
                    return "string";
                case nameof(Int16):
                    return "short";
                case nameof(Int32):
                    return "int";
                case nameof(Int64):
                    return "long";
                case nameof(UInt16):
                    return "ushort";
                case nameof(UInt32):
                    return "uint";
                case nameof(UInt64):
                    return "ulong";
                case nameof(Single):
                    return "float";
                case nameof(Double):
                    return "double";
                case nameof(Decimal):
                    return "decimal";
                case nameof(Object):
                    return "object";
                case "Void":
                    return "void";
                case nameof(Boolean):
                    return "bool";
                case nameof(Byte):
                    return "byte";
                case nameof(SByte):
                    return "sbyte";
            }
             
            //TODO: Figure out how type.FullName could be null and document (or remove) this condition
            string signature = string.IsNullOrWhiteSpace(type.FullName)
                ? type.Name
                : type.FullName;

            if (IsGeneric(type))
                signature = RemoveGenericTypeNameArgumentCount(signature);

            return signature;
        }

        /// <summary>
        /// Removes the `{argument-count} from the signature of a generic type.
        /// </summary>
        /// <param name="genericTypeSignature">Signature of a generic type</param>
        /// <returns><paramref name="genericTypeSignature"/> without any argument count</returns>
        private static string RemoveGenericTypeNameArgumentCount(string genericTypeSignature)
        {
            return genericTypeSignature.Substring(0, genericTypeSignature.IndexOf('`'));
        }

        #endregion

        #region Flutter support utilities

        static readonly Type[] FlutterUnsupportedPrimitiveTypes =
        {
            typeof(ushort), typeof(uint), typeof(ulong), typeof(IntPtr), typeof(UIntPtr)
        };

        static readonly Type[] FlutterSupportedBuiltinTypes =
        {
            typeof(string), typeof(object)
        };

        public static bool IsFlutterSupportedType(this Type type)
        {
            Type t = Nullable.GetUnderlyingType(type) ?? type;

            if (t.IsPrimitive)
            {
                return !FlutterUnsupportedPrimitiveTypes.Contains(t);
            }

            if (t.IsArray)
            {
                Type elementType = t.GetElementType();
                return IsFlutterSupportedType(elementType);
            }

            if (t.IsGenericType)
            {
                bool implementsDictionary = t.GetInterfaces()
                    .Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IDictionary<,>));

                //bool isDictionary = t.GetGenericTypeDefinition() == typeof(IDictionary<,>);

                if (implementsDictionary)
                {
                    Type[] types = t.GetGenericArguments();
                    return types[0] == typeof(string) && types[1].IsFlutterSupportedType();
                }

                bool implementsEnumerable = t.GetInterfaces()
                    .Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>));

                //bool isEnumerable = t.GetGenericTypeDefinition() == typeof(IEnumerable<>);

                if (implementsEnumerable)
                {
                    Type[] types = t.GetGenericArguments();
                    return types[0].IsFlutterSupportedType();
                }

                return false;
            }

            return t.GetCustomAttributes(typeof(PlatformDataAttribute), false).Length > 0 || FlutterSupportedBuiltinTypes.Contains(t);
        }

        #endregion

        #region PlatformService + PlatformOperation utilities

        public static bool IsValidPlatformService(this Type type)
        {
            object[] attributes = type.GetCustomAttributes(typeof(PlatformServiceAttribute), true);
            if (attributes.Length == 0)
            {
                if (type.IsInterface)
                {
                    // This interface isn't decorated with PlatformService attribute
                    // nor any of the inherited interfaces
                    return false;
                }

                // This class isn't decorated with PlatformService attribute
                // nor any of its base classes
                // Try searching on implemented interfaces...
                return type.GetInterfaces()
                    .Any(i => i.GetCustomAttribute(typeof(PlatformServiceAttribute), true) != null);
            }
            return true;
        }

        public static Type[] GetPlatformServiceTypeDefinitions(this Type type)
        {
            object[] attributes = type.GetCustomAttributes(typeof(PlatformServiceAttribute), true);
            if (attributes.Length == 0)
            {
                if (type.IsInterface)
                {
                    // This interface isn't decorated with PlatformService attribute
                    // nor any of the inherited interfaces
                    return new Type[0];
                }

                // This class isn't decorated with PlatformService attribute
                // nor any of its base classes
                // Try searching on implemented interfaces...
                return type.GetInterfaces()
                    .Where(i => i.GetCustomAttribute(typeof(PlatformServiceAttribute), true) != null)
                    .ToArray();
            }
            return new [] {type};
        }

        public static MethodInfo[] GetPlatformOperations(this Type type)
        {
            if (type.GetCustomAttributes(typeof(PlatformServiceAttribute), true).Length == 0)
                return new MethodInfo[0];
            
            List<MethodInfo> methods = new List<MethodInfo>();

            if (type.IsInterface)
            {
                methods.AddRange(type.GetMethods()
                        .Where(m => m.GetCustomAttribute(typeof(PlatformOperationAttribute), true) != null));

                foreach (Type inherited in type.GetInterfaces())
                    methods.AddRange(inherited.GetPlatformOperations());
            }
            else
            {
                // right now explicit interface methods are NOT supported
                methods.AddRange(type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
                    .Where(m => m.GetCustomAttribute(typeof(PlatformOperationAttribute), true) != null));
            }

            return methods.ToArray();
        }

        public static MethodInfo[] GetUnsupportedPlatformOperations(this Type type)
        {
            if (type.GetCustomAttributes(typeof(PlatformServiceAttribute), true).Length == 0)
                return new MethodInfo[0];

            if (type.IsInterface)
                return new MethodInfo[0];

            // right now private methods (including explicit interface methods) are NOT supported
            return type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                .Where(m => m.GetCustomAttribute(typeof(PlatformOperationAttribute), true) != null)
                .ToArray();
        }

        #endregion

        #region PlatformEvent utilities

        public static EventInfo[] GetPlatformEvents(this Type type)
        {
            if (type.GetCustomAttributes(typeof(PlatformServiceAttribute), true).Length == 0)
                return new EventInfo[0];

            return type.GetEvents(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
                .Where(e => e.GetCustomAttribute(typeof(PlatformEventAttribute), false) != null)
                .Where(e => e.IsSupportedPlatformEvent())
                .ToArray();
        }

        public static EventInfo[] GetUnsupportedPlatformEvents(this Type type)
        {
            if (type.GetCustomAttributes(typeof(PlatformServiceAttribute), true).Length == 0)
                return new EventInfo[0];

            return type.GetEvents(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                .Where(e => e.GetCustomAttribute(typeof(PlatformEventAttribute), false) != null)
                .Where(e => !e.IsSupportedPlatformEvent())
                .ToArray();
        }

        private static bool IsSupportedPlatformEvent(this EventInfo e)
        {
            return e.EventHandlerType == typeof(EventHandler) || 
                   e.EventHandlerType.IsGenericType && e.EventHandlerType.GetGenericTypeDefinition() == typeof(EventHandler<>);
        }

        public static Type GetPlatformEventArgs(this EventInfo e)
        {
            if (e.EventHandlerType.IsGenericType && e.EventHandlerType.GetGenericTypeDefinition() == typeof(EventHandler<>))
            {
                return e.EventHandlerType.GenericTypeArguments.First();
            }
            else if (e.EventHandlerType == typeof(EventHandler))
            {
                return typeof(EventArgs);
            }
            else
            {
                throw new InvalidOperationException("Cannot call this method when the target event is not a supported PlatformEvent.");
            }

        }

        #endregion

        #region PlatformOperationException utilities

        public static bool IsValidPlatformOperationException(this Type type)
        {
            return typeof(PlatformOperationException).IsAssignableFrom(type);
        }

        #endregion

        #region System.Threading.Tasks.Task global extensions

        public static bool IsTask(this Type type)
        {
            return type == typeof(Task) || (type.IsGeneric() && type.GetGenericTypeDefinition() == typeof(Task<>));
        }

        public static bool IsTaskVoid(this Type type)
        {
            return type == typeof(Task);
        }

        public static bool IsTaskT(this Type type)
        {
            return type.IsGeneric() && type.GetGenericTypeDefinition() == typeof(Task<>);
        }

        public static Type GetResultType(this Task task)
        {
            return task.GetType().GetProperty("Result")?.PropertyType ?? typeof(void);
        }

        public static object GetResultValue(this Task task)
        {
            return task.GetType().GetProperty("Result")?.GetValue(task);
        }

        public static object TaskResult(this Task task)
        {
            if (task.GetResultType() == typeof(void))
            {
                return null;
            }
            else
            {
                return task.GetResultValue();
            }
        }

        #endregion
    }
}