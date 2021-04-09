using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Flutnet.Utilities
{
    internal static class SignatureTools
    {
        public static string GetSignature(this MethodInfo method, bool invokable)
        {
            StringBuilder sb = new StringBuilder();

            // Add our method accessors if it's not invokable
            if (!invokable)
            {
                sb.Append(GetMethodAccessorSignature(method));
                sb.Append(" ");
            }

            // Add method name
            sb.Append(method.Name);

            // Add method generics
            if (method.IsGenericMethod)
            {
                sb.Append(GetTypeParametersSignature(method));
            }

            // Add method parameters
            sb.Append(GetMethodArgumentsSignature(method, invokable));

            return sb.ToString();
        }

        /// <summary>
        /// Returns the signature of the specified method according to C# language specification.
        /// The signature of a method consists of the name of the method, the number of type parameters and the type and kind
        /// (value, reference, or output) of each of its formal parameters, considered in the order left to right. [...]
        /// The signature of a method specifically does not include the return type,
        /// the params modifier that may be specified for the right-most parameter,
        /// nor the optional type parameter constraints.
        /// For further reading see: https://stackoverflow.com/a/33712878
        /// </summary>
        public static string GetCSharpSignature(this MethodInfo method)
        {
            StringBuilder sb = new StringBuilder();

            // Add method name
            sb.Append(method.Name);

            // Add type parameters if it's a generic method (i.e. <string, string>)
            if (method.IsGenericMethod)
            {
                sb.Append(GetTypeParametersSignature(method));
            }

            // Add formal parameters (i.e. (int, string, DateTime))
            sb.Append(GetFormalParametersSignature(method));

            return sb.ToString();
        }

        public static string GetMethodAccessorSignature(this MethodInfo method)
        {
            StringBuilder sb = new StringBuilder();

            // Access level
            if (method.IsPublic)
            {
                sb.Append("public ");
            }
            else if (method.IsPrivate)
            {
                sb.Append("private ");
            }
            else if (method.IsFamily)
            {
                sb.Append("protected ");
            }
            else if (method.IsAssembly)
            {
                sb.Append("internal ");
            }
            else if (method.IsFamilyOrAssembly)
            {
                sb.Append("protected internal ");
            }

            // Member or static
            if (method.IsStatic)
                sb.Append("static ");

            // Abstract or virtual
            if (method.IsAbstract)
            {
                sb.Append("abstract ");
            }
            else if (method.IsVirtual)
            {
                sb.Append("virtual ");
            }

            // Return type
            sb.Append(method.ReturnType.GetSignature());

            return sb.ToString();
        }

        public static string GetMethodArgumentsSignature(this MethodInfo method, bool invokable)
        {
            var isExtensionMethod = method.IsDefined(typeof(System.Runtime.CompilerServices.ExtensionAttribute), false);
            var methodParameters = method.GetParameters().AsEnumerable();

            // If this signature is designed to be invoked and it's an extension method
            if (isExtensionMethod && invokable)
            {
                // Skip the first argument
                methodParameters = methodParameters.Skip(1);
            }

            var methodParameterSignatures = methodParameters.Select(param => {
                var signature = string.Empty;

                if (param.ParameterType.IsByRef)
                    signature = "ref ";
                else if (param.IsOut)
                    signature = "out ";
                else if (isExtensionMethod && param.Position == 0)
                    signature = "this ";

                if (!invokable)
                {
                    signature += GetSignature(param.ParameterType) + " ";
                }

                signature += param.Name;

                return signature;
            });

            var methodParameterString = "(" + string.Join(", ", methodParameterSignatures) + ")";

            return methodParameterString;
        }

        public static string GetTypeParametersSignature(this MethodInfo method)
        {
            if (method == null) 
                throw new ArgumentNullException(nameof(method));

            if (!method.IsGenericMethod) 
                throw new ArgumentException($"{method.Name} is not generic.");

            return BuildGenericSignature(method.GetGenericArguments());
        }

        public static string GetFormalParametersSignature(this MethodInfo method)
        {
            List<string> signatures = new List<string>();
            foreach (ParameterInfo parameterInfo in method.GetParameters())
            {
                Type parameterType = parameterInfo.IsOut || parameterInfo.ParameterType.IsByRef
                    ? parameterInfo.ParameterType.GetElementType()
                    : parameterInfo.ParameterType;

                if (parameterInfo.IsOut)
                {
                    signatures.Add("out " + GetSignature(parameterType));
                }
                else if (parameterInfo.ParameterType.IsByRef)
                {
                    signatures.Add("ref " + GetSignature(parameterType));
                }
                else
                {
                    signatures.Add(GetSignature(parameterType));
                }
            }

            string signature = "(" + string.Join(", ", signatures) + ")";
            return signature;
        }

        /// <summary>
        /// Takes an <see cref="IEnumerable{T}"/> and creates a generic type signature (&lt;string, string&gt; for example)
        /// </summary>
        /// <param name="genericArgumentTypes"></param>
        /// <returns>Generic type signature like &lt;Type, ...&gt;</returns>
        private static string BuildGenericSignature(IEnumerable<Type> genericArgumentTypes)
        {
            IEnumerable<string> argumentSignatures = genericArgumentTypes.Select(GetSignature);

            return "<" + string.Join(", ", argumentSignatures) + ">";
        }

        /// <summary>
        /// Get a fully qualified signature for <paramref name="type"/>
        /// </summary>
        /// <param name="type">Type. May be generic or <see cref="Nullable{T}"/></param>
        /// <returns>Fully qualified signature</returns>
        public static string GetSignature(this Type type)
        {
            bool isNullableType = type.IsNullable(out Type underlyingNullableType);

            Type signatureType = isNullableType
                ? underlyingNullableType
                : type;

            bool isGenericType = signatureType.IsGeneric();

            string signature = signatureType.GetQualifiedTypeName();

            if (isGenericType)
            {
                // Add the generic arguments
                signature += BuildGenericSignature(signatureType.GetGenericArguments());
            }

            if (isNullableType)
            {
                signature += "?";
            }

            return signature;
        }
    }
}