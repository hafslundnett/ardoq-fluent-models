using ArdoqFluentModels;
using System.Collections.Generic;
using System.Linq;

namespace ModelMaintainer.Tests.Model
{
    public class TestSourceModelProvider : ISourceModelProvider
    {
        private readonly IEnumerable<object> _objects;

        public TestSourceModelProvider(IEnumerable<object> objs)
        {
            _objects = objs;
        }

        public IEnumerable<object> GetSourceModel()
        {
            return _objects;
        }
    }
}
