using System;
using System.Collections.Generic;

namespace ArdoqFluentModels.Mapping
{
    public class ExternalReferenceSpecification<TSource> : IExternalReferenceSpecification
    {
        private readonly Func<TSource, string> _componentKeyGetter;

        public ExternalReferenceSpecification(
            string componentType, 
            Func<TSource, string> componentKeyGetter, 
            string refName,
            string workspaceName)
        {
            TargetComponentType = componentType;
            _componentKeyGetter = componentKeyGetter;
            ReferenceName = refName;
            WorkspaceName = workspaceName;
        }

        public string TargetComponentType { get; }
        public string WorkspaceName { get; }
        public Type SourceType => typeof(TSource);
        public string ReferenceName { get; }

        public string GetTargetComponentKey(object obj)
        {
            try
            {
                return _componentKeyGetter.Invoke((TSource) obj);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
