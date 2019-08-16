using System;
using System.Collections.Generic;

namespace ModelMaintainer.Mapping
{
    public static class Tags
    {
        public static ITagGetter<T> FromExpression<T>(Func<T, IEnumerable<string>> func)
        {
            return new TagGetter<T>(func);
        }

        private class TagGetter<T> : ITagGetter<T>
        {
            private readonly Func<T, IEnumerable<string>> _func;

            public TagGetter(Func<T, IEnumerable<string>> func)
            {
                _func = func;
            }

            public IEnumerable<string> GetTags(T obj)
            {
                return _func.Invoke(obj);
            }
        }
    }
}
