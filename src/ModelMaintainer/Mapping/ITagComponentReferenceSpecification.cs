using System;
using System.Collections.Generic;
using System.Text;

namespace ModelMaintainer.Mapping
{
    public interface ITagComponentReferenceSpecification<TSource> : ITagComponentReference
    {
        ITagComponentReferenceSpecification<TSource> AddReferenceTagGetter(Func<TSource, string> getter);
        void AddSearchFolder(string folderName);
    }
}
