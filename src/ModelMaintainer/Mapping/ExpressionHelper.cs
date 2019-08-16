using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace ModelMaintainer.Mapping
{
    public static class ExpressionHelper
    {
        private const string ExpressionCannotBeNullMessage = "The expression cannot be null.";
        private const string InvalidExpressionMessage = "Invalid expression.";

        public static string GetMemberName(Expression expr)
        {
            if (expr == null)
            {
                throw new ArgumentException(ExpressionCannotBeNullMessage);
            }

            var expression = expr is LambdaExpression lambdaExpression
                ? lambdaExpression.Body
                : expr;

            if (expression is MemberExpression memberExpression)
            {
                // Reference type property or field
                return memberExpression.Member.Name;
            }

            if (expression is MethodCallExpression methodCallExpression)
            {
                // Reference type method
                return methodCallExpression.Method.Name;
            }

            if (expression is UnaryExpression unaryExpression)
            {
                // Property, field of method returning value type
                return GetMemberName(unaryExpression);
            }

            throw new ArgumentException(InvalidExpressionMessage);
        }

        private static string GetMemberName(UnaryExpression unaryExpression)
        {
            if (unaryExpression.Operand is MethodCallExpression)
            {
                var methodExpression = (MethodCallExpression) unaryExpression.Operand;
                return methodExpression.Method.Name;
            }

            return ((MemberExpression) unaryExpression.Operand).Member.Name;
        }

        public static IEnumerable<object> GetReferencedObjects(object obj, MethodInfo getter)
        {
            var val = getter.Invoke(obj, new object[] { });
            if (val == null)
            {
                return null;
            }

            if (typeof(IEnumerable).IsInstanceOfType(val))
            {
                var res = new List<object>();
                foreach (var o in (IEnumerable)val)
                {
                    res.Add(o);
                }

                return res;
            }

            return new List<object>{val};
        }
    }
}
