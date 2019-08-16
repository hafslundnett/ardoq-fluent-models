using System.Collections.Generic;
using System.Linq;
using ModelMaintainer.Ardoq;
using ModelMaintainer.Mapping;
using ModelMaintainer.Search;

namespace ModelMaintainer.Maintainence
{
    public class SearchLinkageService
    {
        private readonly IArdoqSearcher _searcher;
        private readonly IArdoqReader _reader;
        private readonly IArdoqWriter _writer;
        private readonly IArdoqSession _sourceWorkspaceSession;
        private readonly IBuiltComponentMapping _mapping;
        private readonly string _ardoqReferenceName;
        private readonly ILogger _logger;

        public SearchLinkageService(
            IArdoqReader reader,
            IArdoqWriter writer,
            IArdoqSession session,
            IArdoqSearcher searcher,
            IBuiltComponentMapping mapping,
            string ardoqReferenceName,
            ILogger logger)
        {
            _reader = reader;
            _writer = writer;
            _sourceWorkspaceSession = session;
            _searcher = searcher;
            _mapping = mapping;
            _ardoqReferenceName = ardoqReferenceName;
            _logger = logger;
        }

        public void LinkBySearch(ISearchBuilder builder, List<ParentChildRelation> relations)
        {
            foreach (var relation in relations)
            {
                var searchSpec = builder.BuildSearch(relation.Child);
                SearchAndLink(relation, searchSpec);
            }
        }

        private void SearchAndLink(ParentChildRelation relation, SearchSpec searchSpec)
        {
            var sourceComponent = _sourceWorkspaceSession.GetChildComponent(relation);
            var components = _searcher.Search(searchSpec).Result;
            if (components == null || !components.Any())
            {
                return;
            }

            var refType = _sourceWorkspaceSession.GetReferenceTypeForName(_ardoqReferenceName);

            var existingReferences = _sourceWorkspaceSession.GetAllSourceReferencesFromChild(relation)
                .Where(r => r.Type == refType)
                .ToList();

            foreach (var targetComponent in components)
            {
                if (existingReferences.Any(r => r.Target == targetComponent.Id))
                {
                    continue;
                }

                _sourceWorkspaceSession.AddReference(refType, sourceComponent, targetComponent);
            }
        }
    }
}
