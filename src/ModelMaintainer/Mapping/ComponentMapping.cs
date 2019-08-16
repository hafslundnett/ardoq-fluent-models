using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using ModelMaintainer.Mapping.ComponentHierarchy;
using ModelMaintainer.Search;

namespace ModelMaintainer.Mapping
{
    public class ComponentMapping<TSource> : IComponentMapping<TSource>
    {
        private readonly Dictionary<string, PropertyInfo> _fieldGetters = new Dictionary<string, PropertyInfo>();
        private readonly Dictionary<(string, string), PropertyInfo> _refGetters = new Dictionary<(string, string), PropertyInfo>();
        private readonly List<ExternalReferenceSpecification<TSource>> _externalReferences = new List<ExternalReferenceSpecification<TSource>>();
        private readonly List<NamedReferenceAccessSpecification> _namedReferenceAccessors = new List<NamedReferenceAccessSpecification>();
        private readonly List<ModelledReferenceAccessSpecification<TSource>> _modelledReferenceAccessors = new List<ModelledReferenceAccessSpecification<TSource>>();
        private readonly List<ITagComponentReference> _tagReferenceGetters = new List<ITagComponentReference>();
        private readonly List<(string, SearchBuilder<TSource>)> _searchReferenceBuilders = new List<(string, SearchBuilder<TSource>)>();
        private PropertyInfo _propertyInfoTagGetter;
        private ITagGetter<TSource> _tagGetter;
        private IBuiltComponentMapping _parent;

        public ComponentMapping(string ardoqComponentTypeName)
        {
            ArdoqComponentTypeName = ardoqComponentTypeName;
        }

        public string ArdoqComponentTypeName { get; }
        public Dictionary<(string, string), PropertyInfo> RefGetters => _refGetters;

        public IEnumerable<IExternalReferenceSpecification> ExternalReferenceSpecifications => _externalReferences;
        public IEnumerable<NamedReferenceAccessSpecification> NamedHierarchyParentAccessSpecifications => _namedReferenceAccessors;
        public IEnumerable<IHierarchyReferenceAccessSpecification> ModelledHierarchyAccessSpecifications => _modelledReferenceAccessors;
        public IEnumerable<ITagComponentReference> TagReferenceGetters => _tagReferenceGetters;

        public IComponentMapping<TSource> WithSearchBasedReferences(SearchBuilder<TSource> builder, string ardoqReferenceName)
        {
            _searchReferenceBuilders.Add((ardoqReferenceName, builder));
            return this;
        }

        public IComponentMapping<TSource> WithPreexistingHierarchyReference(string name)
        {
            _namedReferenceAccessors.Add(new NamedReferenceAccessSpecification(name));
            CheckHierarchyConsistency();
            return this;
        }

        public IComponentMapping<TSource> WithModelledHierarchyReference(Expression<Func<TSource, object>> expression, ModelledReferenceDirection direction)
        {
            _modelledReferenceAccessors.Add(new ModelledReferenceAccessSpecification<TSource>(expression, direction));
            CheckHierarchyConsistency();
            return this;
        }

        public IComponentMapping<TSource> WithKey(Expression<Func<TSource, string>> lambda)
        {
            var memberName = ExpressionHelper.GetMemberName(lambda);
            KeyGetter = typeof(TSource).GetProperty(memberName, BindingFlags.Instance | BindingFlags.Public);
            return this;
        }

        public IComponentMapping<TSource> WithField(Expression<Func<TSource, object>> lambda, string ardoqFieldName)
        {
            var memberName = ExpressionHelper.GetMemberName(lambda);
            var getter = typeof(TSource).GetProperty(memberName, BindingFlags.Instance | BindingFlags.Public);
            _fieldGetters[ardoqFieldName] = getter;
            return this;
        }

        public IComponentMapping<TSource> WithReference(Expression<Func<TSource, object>> lambda, string ardoqRefName)
        {
            var memberName = ExpressionHelper.GetMemberName(lambda);
            var getter = typeof(TSource).GetProperty(memberName, BindingFlags.Instance | BindingFlags.Public);

            var source = getter.DeclaringType.Name + "_" + getter.Name;
            _refGetters[(source, ardoqRefName)] = getter;
            return this;
        }

        public IComponentMapping<TSource> WithTagReference(ITagComponentReference tagReference)
        {
            _tagReferenceGetters.Add(tagReference);
            return this;
        }

        public IComponentMapping<TSource> WithTags(Expression<Func<TSource, IEnumerable<string>>> lambda)
        {
            if (_tagGetter != null)
            {
                throw new ArgumentException("Tag getter already defined.");
            }

            var memberName = ExpressionHelper.GetMemberName(lambda);
            _propertyInfoTagGetter = typeof(TSource).GetProperty(memberName, BindingFlags.Instance | BindingFlags.Public);
            return this;
        }

        public IComponentMapping<TSource> WithTags(ITagGetter<TSource> tagGetter)
        {
            if (_propertyInfoTagGetter != null)
            {
                throw new ArgumentException("Tag getter already defined.");
            }

            _tagGetter = tagGetter;
            return this;
        }

        public IComponentMapping<TSource> WithTaggedReference(
            Func<TSource, IEnumerable<string>> tagGetter,
            string ardoqRefName)
        {
            return this;
        }

        public IComponentMapping<TSource> WithExternalReference(
            string targetComponentType, 
            Func<TSource, string> componentNameGetter,
            string ardoqRefName,
            string externalWorkspaceName)
        {
            var spec = new ExternalReferenceSpecification<TSource>(targetComponentType, componentNameGetter,
                ardoqRefName, externalWorkspaceName);

            _externalReferences.Add(spec);
            return this;
        }

        public (string, IDictionary<string, object>) GetComponentInfo(object obj)
        {
            var key = KeyGetter.GetMethod.Invoke(obj, new object[]{}).ToString();

            var fieldValues = new Dictionary<string, object>();
            foreach (var pair in _fieldGetters)
            {
                var val = pair.Value.GetMethod.Invoke(obj, new object[] { });
                if (val != null)
                {
                    fieldValues[pair.Key] = val;
                }
            }

            return (key, fieldValues);
        }

        public string GetKeyForInstance(object obj)
        {
            if (KeyGetter == null)
            {
                throw new Exception($"KeyGetter is null for object of type {obj.GetType().FullName}. " +
                    "All elements in the model must have a key, use the extension method IComponentMapping.WithKey");
            }
            if (KeyGetter.GetMethod.Invoke(obj, new object[] { }) == null)
            {
                throw new Exception($"The property {KeyGetter.GetMethod.Name} of the object of type {obj.GetType().FullName} is marked as the key but evaluates to null. " +
                    "All key properties must be not null.");
            }

            return KeyGetter.GetMethod.Invoke(obj, new object[] { }).ToString();
        }

        public IEnumerable<string> GetTags(object obj)
        {
            if (_tagGetter != null)
            {
                return _tagGetter.GetTags((TSource)obj);
            }

            if (_propertyInfoTagGetter == null)
            {
                return null;
            }

            return (IEnumerable<string>)_propertyInfoTagGetter.GetMethod.Invoke(obj, new object[] { });
        }

        public IEnumerable<(string, ISearchBuilder)> SearchReferenceBuilders
        {
            get { return _searchReferenceBuilders.Select(srb => (srb.Item1, (ISearchBuilder)srb.Item2)); }
        }

        public Type SourceType => typeof(TSource);
        public PropertyInfo KeyGetter { get; private set; }

        private void CheckHierarchyConsistency()
        {
            if (_modelledReferenceAccessors.Count(p => p.IsChild) + _namedReferenceAccessors.Count(p => p.IsChild) > 1)
            {
                throw new InvalidOperationException("Can only configure a single reference where the class is the child.");
            }
        }

        public IEnumerable<Type> GetChildrenTypes()
        {
            return ModelledHierarchyAccessSpecifications.Where(m => !m.IsChild).Select(m => m.GetRelatedType());
        }

        public Type GetParentType()
        {
            return ModelledHierarchyAccessSpecifications.SingleOrDefault(m => m.IsChild)?.GetRelatedType();
        }

        public IBuiltComponentMapping GetParent()
        {
            return _parent;
        }

        public void SetParent(IBuiltComponentMapping p)
        {
            _parent = p;
        }
    }
}
