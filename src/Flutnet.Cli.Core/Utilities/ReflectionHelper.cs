using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Flutnet.Cli.Core.Utilities
{
    internal static class ReflectionHelper
    {
        /// <summary>
        /// Recursively gets all generic type params in any given type definition.
        /// So if you had a List{Dictionary{Dictionary{string, int}, Dictionary{string, int}}}, it would get all of the type params.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        public static Type[] GetAllTypeParams(Type type, List<Type> list = null)
        {
            if (list == null)
                list = new List<Type>();

            if (type.IsGenericType)
            {
                Type[] typeParams = type.GetGenericArguments();
                list.AddRange(typeParams);

                // Recursively process all types
                foreach (Type t in typeParams)
                    GetAllTypeParams(t, list);
            }
            return list.ToArray();
        }

        /// <summary>
        /// Find all the nested method in a class.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="includeInterface"></param>
        /// <returns></returns>
        public static MethodInfo[] GetNestedMethods(this Type type, bool includeInterface = false)
        {

            List<MethodInfo> methods = new List<MethodInfo>();

            if (type.BaseType != null)
            {
                methods.AddRange(GetNestedMethods(type.BaseType, includeInterface));
            }

            if (includeInterface)
            {
                Type[] interfaces = type.GetInterfaces() ?? new Type[0];

                foreach (var i in interfaces)
                {
                    methods.AddRange(GetNestedMethods(i, true));
                }
            }

            methods.AddRange(type.GetMethods());

            return methods.ToArray();

        }

        /// <summary>
        /// Find all the nested method implemented by this class.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static MethodInfo[] GetInterfaceMethods(this Type type)
        {

            List<MethodInfo> methods = new List<MethodInfo>();

            if (type.IsInterface && type.BaseType != null)
            {
                methods.AddRange(GetInterfaceMethods(type.BaseType));
            }

            Type[] interfaces = type.GetInterfaces() ?? new Type[0];

            foreach (var i in interfaces)
            {
                methods.AddRange(GetInterfaceMethods(i));
            }

            if (type.IsInterface)
            {
                methods.AddRange(type.GetMethods());
            }

            return methods.ToArray();

        }

        /// <summary>
        /// Find all the nested method implemented by this class.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Type[] GetNestedInterface(this Type type)
        {

            List<Type> nestedInterfaces = new List<Type>();

            if (type.IsInterface && type.BaseType != null)
            {
                nestedInterfaces.AddRange(GetNestedInterface(type.BaseType));
            }

            Type[] interfaces = type.GetInterfaces() ?? new Type[0];

            foreach (var i in interfaces)
            {
                nestedInterfaces.AddRange(GetNestedInterface(i));
            }

            if (type.IsInterface)
            {
                nestedInterfaces.Add(type);
            }

            return nestedInterfaces.Distinct().ToArray();

        }

        // TODO DA TESTARE
        public static bool ImplementInterface(this MethodInfo method)
        {
            Type[] interfaces = method?.ReflectedType?.GetNestedInterface() ?? new Type[0];

            foreach (var @interface in interfaces)
            {
                if (method.Implements(@interface))
                {
                    return true;
                }
            }

            return false;
        }

        // TODO DA TESTARE
        public static bool Implements(this MethodInfo method, Type iface)
        {
            return method.ReflectedType != null && method.ReflectedType.GetInterfaceMap(iface).TargetMethods.Contains(method);
        }


        class PropertyInfoComparer : IEqualityComparer<PropertyInfo>
        {
            public bool Equals(PropertyInfo x, PropertyInfo y)
            {
                if (x == null || y == null)
                    return false;

                return x.PropertyType == y.PropertyType &&
                       x.Name == y.Name &&
                       x.DeclaringType == y.DeclaringType;
            }

            public int GetHashCode(PropertyInfo obj)
            {
                return obj.GetHashCode();
            }
        }

        public static IEnumerable<PropertyInfo> GetProperties(this Type t, BindingFlags flags, Type baseHidingType = null)
        {
            if (baseHidingType != null && baseHidingType.IsAssignableFrom(t))
            {
                var all = t.GetProperties(flags);
                var filtered = all.Where(p => baseHidingType.IsAssignableFrom(p.DeclaringType));
                return filtered;
            }
            else
            {
                return t.GetProperties(flags);
            }
        }

        public static IEnumerable<FieldInfo> GetFields(this Type t, BindingFlags flags, Type baseHidingType = null)
        {
            if (baseHidingType != null && baseHidingType.IsAssignableFrom(t))
            {
                var all = t.GetFields(flags);
                var filtered = all.Where(f => baseHidingType.IsAssignableFrom(f.DeclaringType));
                return filtered;
            }
            else
            {
                return t.GetFields(flags);
            }
        }

        public static bool IsInherited(this PropertyInfo propertyInfo)
        {
            Type relatedType = propertyInfo.DeclaringType;

            IEnumerable<PropertyInfo> declaredProps = relatedType?.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public) ?? new PropertyInfo[0];
            IEnumerable<PropertyInfo> inheritedProps = relatedType?.BaseType?.GetProperties(BindingFlags.Public | BindingFlags.Instance) ?? new PropertyInfo[0];

            bool isDeclared = declaredProps.Contains(propertyInfo);
            bool isInherited = inheritedProps.Contains(propertyInfo);

            return isInherited && !isDeclared;
        }

        public static bool IsDeclared(this PropertyInfo propertyInfo)
        {
            Type relatedType = propertyInfo.DeclaringType;

            IEnumerable<PropertyInfo> declaredProps = relatedType?.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public) ?? new PropertyInfo[0];
            //IEnumerable<PropertyInfo> inheritedProps = relatedType?.BaseType?.GetProperties(BindingFlags.Public | BindingFlags.Instance) ?? new PropertyInfo[0];

            bool isDeclared = declaredProps.Contains(propertyInfo);
            //bool isInherited = inheritedProps.Contains(propertyInfo);

            return isDeclared;
        }

        public static bool IsInherited(this FieldInfo fieldInfo)
        {
            Type relatedType = fieldInfo.DeclaringType;
                                 
            IEnumerable<FieldInfo> declaredFields = relatedType?.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public) ?? new FieldInfo[0];
            IEnumerable<FieldInfo> inheritedFields = relatedType?.BaseType?.GetFields(BindingFlags.Public | BindingFlags.Instance) ?? new FieldInfo[0];

            bool isDeclared = declaredFields.Contains(fieldInfo);
            bool isInherited = inheritedFields.Contains(fieldInfo);

            return isInherited && !isDeclared;
        }

        public static bool IsDeclared(this FieldInfo fieldInfo)
        {
            Type relatedType = fieldInfo.DeclaringType;

            IEnumerable<FieldInfo> declaredFields = relatedType?.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public) ?? new FieldInfo[0];
            //IEnumerable<FieldInfo> inheritedFields = relatedType?.BaseType?.GetFields(BindingFlags.Public | BindingFlags.Instance) ?? new FieldInfo[0];

            bool isDeclared = declaredFields.Contains(fieldInfo);
            //bool isInherited = inheritedFields.Contains(fieldInfo);

            return isDeclared;
        }

        public static IEnumerable<FieldInfo> GetDeclaredPublicField(this Type type)
        {
            IEnumerable<FieldInfo> declaredFields = type.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public) ?? new FieldInfo[0];
            return declaredFields;
        }

        public static IEnumerable<FieldInfo> GetInheritedPublicField(this Type type)
        {
            IEnumerable<FieldInfo> inheritedFields = type.BaseType?.GetFields(BindingFlags.Public | BindingFlags.Instance) ?? new FieldInfo[0];
            return inheritedFields;
        }

        public static IEnumerable<FieldInfo> GetPublicField(this Type type)
        {
            IEnumerable<FieldInfo> declaredFields = type.GetFields(BindingFlags.Instance | BindingFlags.Public) ?? new FieldInfo[0];
            return declaredFields;
        }

        public static IEnumerable<PropertyInfo> GetDeclaredPublicProperties(this Type type)
        {
            IEnumerable<PropertyInfo> declaredProps = type.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public) ?? new PropertyInfo[0];
            return declaredProps;
        }

        public static IEnumerable<PropertyInfo> GetInheritedPublicProperties(this Type type)
        {
            IEnumerable<PropertyInfo> inheritedProps = type.BaseType?.GetProperties(BindingFlags.Public | BindingFlags.Instance) ?? new PropertyInfo[0];
            return inheritedProps;
        }

        public static IEnumerable<PropertyInfo> GetPublicProperties(this Type type)
        {
            IEnumerable<PropertyInfo> declaredProps = type.GetProperties(BindingFlags.Instance | BindingFlags.Public) ?? new PropertyInfo[0];
            return declaredProps;
        }

        public static IEnumerable<Type> GetFieldsTypes(this Type type, BindingFlags flags, Type baseHidingType = null)
        {
            IEnumerable<Type> types = type.GetFields(flags, baseHidingType).Select(f => f.FieldType) ?? new Type[0];
            return types;
        }

        public static IEnumerable<Type> GetPropertiesTypes(this Type type, BindingFlags flags, Type baseHidingType = null)
        {
            IEnumerable<Type> types = type.GetProperties(flags, baseHidingType).Select(p=>p.PropertyType) ?? new Type[0];
            return types;
        }

        public static IEnumerable<Type> GetFieldsAndPropertiesTypes(this Type type, BindingFlags flags, Type baseHidingType = null)
        {
            IEnumerable<Type> types = GetFieldsTypes(type, flags, baseHidingType).Concat(GetPropertiesTypes(type, flags, baseHidingType));
            return types;
        }

        // Return all the custom types that extends type t.
        public static IEnumerable<Type> GetSubTypes(ICollection<Type> customTypes, Type t)
        {
            return customTypes.Where(t.IsAssignableFrom).ToList();
        }

        /// <summary>
        /// Invokes a private method on a given object, using the specified parameters.
        /// </summary>
        /// <param name="obj">The object on which to invoke the method.</param>
        /// <param name="methodName">The name of the method to invoke.</param>
        /// <param name="parameters">An argument list for the invoked method.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the method is not found.</exception>
        public static object Call(object obj, string methodName, params object[] parameters)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            Type t = obj.GetType();
            MethodInfo mi = t.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (mi == null)
                throw new ArgumentOutOfRangeException(nameof(methodName), $"Method {methodName} not found in Type {t.FullName}");

            return mi.Invoke(obj, parameters);
        }

        /// <summary>
        /// Invokes a private, static method on a given type, using the specified parameters.
        /// </summary>
        /// <param name="type">The type on which to invoke the static method.</param>
        /// <param name="methodName">The name of the method to invoke.</param>
        /// <param name="parameters">An argument list for the invoked method.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the method is not found.</exception>
        public static object CallStatic(Type type, string methodName, params object[] parameters)
        {
            Type t = type;
            MethodInfo mi = t.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            if (mi == null)
                throw new ArgumentOutOfRangeException(nameof(methodName), $"Method {methodName} not found in Type {t.FullName}");

            return mi.Invoke(null, parameters);
        }
    }
}
