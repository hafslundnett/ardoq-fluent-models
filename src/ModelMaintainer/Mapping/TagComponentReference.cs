using System;
using System.Collections.Generic;

namespace ModelMaintainer.Mapping
{
    public class TagComponentReference<TSource> : ITagComponentReferenceSpecification<TSource>
    {
        private readonly List<Func<TSource, string>> _tagReferenceGetters = new List<Func<TSource, string>>();

        public TagComponentReference(string ardoqReferenceName)
        {
            ArdoqReferenceName = ardoqReferenceName;
        }

        public string ArdoqReferenceName { get; }

        private string _folderName;

        public string GetSearchFolder()
        {
            return _folderName;
        }

        private void SetFolderName(string value)
        {
            _folderName = value;
        }

        public ITagComponentReferenceSpecification<TSource> AddReferenceTagGetter(Func<TSource, string> getter)
        {
            _tagReferenceGetters.Add(getter);
            return this;
        }

        public void AddSearchFolder(string folderName) => SetFolderName(folderName);

        public (bool, IEnumerable<string>) GetTags(object sourceObject)
        {
            var tags = new List<string>();
            foreach (var getter in _tagReferenceGetters)
            {
                try
                {
                    var tag = getter.Invoke((TSource) sourceObject);
                    if (string.IsNullOrWhiteSpace(tag))
                    {
                        return (false, null);
                    }

                    tags.Add(tag);
                }
                catch (Exception)
                {
                    return (false, null);
                }
            }

            return (true, tags);
        }
    }
}
