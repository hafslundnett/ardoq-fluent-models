using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace ModelMaintainer.Mapping.ComponentHierarchy
{
    public class ModelledReferenceAccessSpecification<TSource> : IHierarchyReferenceAccessSpecification
    {
        private readonly MethodInfo _getter;
        private readonly ModelledReferenceDirection _direction;

        public ModelledReferenceAccessSpecification(
            Expression<Func<TSource, object>> expression,
            ModelledReferenceDirection direction)
        {
            _direction = direction;

            var memberName = ExpressionHelper.GetMemberName(expression);
            _getter = typeof(TSource).GetProperty(memberName, BindingFlags.Instance | BindingFlags.Public).GetMethod;
        }

        public Type GetRelatedType()
        {
            if (typeof(IEnumerable).IsAssignableFrom(_getter.ReturnType))
            {
                return _getter.ReturnType.GetGenericArguments()[0];
            }
            else
            {
                return _getter.ReturnType;
            }
        }

        public IEnumerable<(object, ModelledReferenceDirection)> GetRelatedObjects(object obj)
        {
            var related = _getter.Invoke(obj, new object[] { });
            if (related == null)
            {
                return null;
            }

            if (typeof(IEnumerable).IsAssignableFrom(related.GetType()))
            {
                var list = (IEnumerable) related;
                var result = new List<(object, ModelledReferenceDirection)>();
                foreach (var o in list)
                {
                    result.Add((o, _direction));
                }

                return result;
            }

            return new List<(object, ModelledReferenceDirection)>
            {
                (related, _direction)
            };
        }

        public bool IsNamed => false;
        public bool IsChild => _direction == ModelledReferenceDirection.Child;
    }
}
