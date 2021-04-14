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
using System.Linq.Expressions;
using System.Reflection;

namespace Flutnet.Utilities
{
    internal static class ExpressionTools
    {
        public static Func<object[], object> CreateLazyMethodWithResult(object instance, MethodInfo method)
        {
            ParameterExpression allParameters;
            Expression methodCall = GenerateCallExpression(instance, method, out allParameters);
            Expression<Func<object[], object>> lambda = Expression.Lambda<Func<object[], object>>(methodCall, allParameters);
            return lambda.Compile();
        }

        public static Action<object[]> CreateLazyMethodWithNoResult(object instance, MethodInfo method)
        {
            ParameterExpression allParameters;
            Expression methodCall = GenerateCallExpression(instance, method, out allParameters);
            Expression<Action<object[]>> lambda = Expression.Lambda<Action<object[]>>(methodCall, allParameters);
            return lambda.Compile();
        }

        public static Func<object[], object> CreateLazyStaticMethodWithResult(MethodInfo method)
        {
            ParameterExpression allParameters;
            Expression methodCall = GenerateCallExpression(null, method, out allParameters);
            Expression<Func<object[], object>> lambda = Expression.Lambda<Func<object[], object>>(methodCall, allParameters);
            return lambda.Compile();
        }

        public static Action<object[]> CreateLazyStaticMethodWithNoResult(MethodInfo method)
        {
            ParameterExpression allParameters;
            Expression methodCall = GenerateCallExpression(null, method, out allParameters);
            Expression<Action<object[]>> lambda = Expression.Lambda<Action<object[]>>(methodCall, allParameters);
            return lambda.Compile();
        }

        /// <summary>
        /// Generate expression call
        /// </summary>
        /// <param name="instance">If instance is NULL the method is treated as static.</param>
        /// <param name="method"></param>
        /// <param name="allParameters"></param>
        private static Expression GenerateCallExpression(object instance, MethodBase method, out ParameterExpression allParameters)
        {
            List<Expression> parameters = GenerateParameters(method, out allParameters);

            MethodInfo methodInfo = (MethodInfo) method;
            Type returnType = methodInfo.ReturnType;
            MethodCallExpression methodCall;

            if (instance != null)
            {
                // it's an instance (non static) method
                UnaryExpression instanceExpr = Expression.Convert(Expression.Constant(instance), instance.GetType());
                methodCall = Expression.Call(instanceExpr, methodInfo, parameters.ToArray());
            }
            else
            {
                // it's a static method
                methodCall = Expression.Call(methodInfo, parameters.ToArray());
            }

            if (returnType.Name != "Void")
            {
                // the method returns a value,
                // need to convert the expression to return an object
                return Expression.TypeAs(methodCall, typeof(object));
            }

            return methodCall;
        }

        public static Func<object[], object> GenerateLazyConstructorCall(ConstructorInfo constructor)
        {
            ParameterExpression allParameters;
            List<Expression> parameters = GenerateParameters(constructor, out allParameters);

            NewExpression newExpr = Expression.New(constructor, parameters.ToArray());
            Expression<Func<object[], object>> lambda = Expression.Lambda<Func<object[], object>>(newExpr, allParameters);

            return lambda.Compile();
        }

        private static List<Expression> GenerateParameters(MethodBase method, out ParameterExpression allParameters)
        {
            allParameters = Expression.Parameter(typeof(object[]), "params");

            ParameterInfo[] methodParameters = method.GetParameters();
            List<Expression> parameters = new List<Expression>();
            for (int i = 0; i < methodParameters.Length; i++)
            {
                ConstantExpression indexExpr = Expression.Constant(i);
                BinaryExpression item = Expression.ArrayIndex(allParameters, indexExpr);

                ParameterInfo parameterInfo = methodParameters[i];
                Type parameterType = parameterInfo.IsOut || parameterInfo.ParameterType.IsByRef
                    ? parameterInfo.ParameterType.GetElementType()
                    : parameterInfo.ParameterType;

                //UnaryExpression converted = Expression.Convert(item, parameterType);
                // item as T or (T) item
                UnaryExpression converted = !parameterType.IsValueType
                    ? Expression.TypeAs(item, parameterType)
                    : Expression.Convert(item, parameterType);
                parameters.Add(converted);
            }
            return parameters;
        }
    }
}
