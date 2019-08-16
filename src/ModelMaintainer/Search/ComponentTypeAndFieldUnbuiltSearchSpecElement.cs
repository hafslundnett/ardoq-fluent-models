using System;
using System.Collections.Generic;

namespace ArdoqFluentModels.Search
{
    public class ComponentTypeAndFieldUnbuiltSearchSpecElement<TSource> : IUnbuiltSearchSpecElement
    {
        private Func<TSource, string> _componentTypeGetter;
        private string _hardcodedComponentType;
        private Func<TSource, IDictionary<string, object>> _fieldGetter;
        private Func<TSource, List<string>> _nameGetter;
        private string _hardcodedName;
        private Func<TSource, List<string>> _parentNameGetter;

        public void SetComponentTypeGetter(Func<TSource, string> getter)
        {
            _componentTypeGetter = getter;
        }

        public void SetFieldGetter(Func<TSource, IDictionary<string, object>> getter)
        {
            _fieldGetter = getter;
        }

        public void SetHardcodedComponentType(string compType)
        {
            _hardcodedComponentType = compType;
        }

        public void SetNameGetter(Func<TSource, List<string>> nameLambda)
        {
            _nameGetter = nameLambda;
        }

        public void SetHardcodedName(string name)
        {
            _hardcodedName = name;
        }

        public void SetParentNameGetter(Func<TSource, List<string>> parentNameLambda)
        {
            _parentNameGetter = parentNameLambda;
        }

        public ISearchSpecElement Build(object obj)
        {
            var spec = new ComponentTypeAndFieldSearchSpecElement();

            if (_componentTypeGetter != null)
            {
                spec.ComponentType = _componentTypeGetter.Invoke((TSource)obj);
            }
            else if (!string.IsNullOrWhiteSpace(_hardcodedComponentType))
            {
                spec.ComponentType = _hardcodedComponentType;
            }

            if (_nameGetter != null)
            {
                spec.NameList = _nameGetter.Invoke((TSource)obj);
            }
            else if (!string.IsNullOrWhiteSpace(_hardcodedName))
            {
                spec.Name = _hardcodedName;
            }

            if (_parentNameGetter != null)
            {
                spec.ParentNameList = _parentNameGetter.Invoke((TSource)obj);
            }

            if (_fieldGetter != null)
            {
                foreach (var pair in _fieldGetter.Invoke((TSource)obj))
                {
                    spec.AddFieldFilter(pair.Key, pair.Value);
                }
            }
            
            return spec;
        }
    }
}
