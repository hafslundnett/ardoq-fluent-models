using System.Collections.Generic;
using System.Linq;
using ModelMaintainer.Ardoq;
using ModelMaintainer.Mapping;

namespace ModelMaintainer.Maintainence
{
    public class TagLinkageService
    {
        private readonly IArdoqSession _session;
        private readonly IArdoqReader _reader;
        private readonly IArdoqWriter _writer;

        public TagLinkageService(IArdoqSession session, IArdoqReader reader, IArdoqWriter writer)
        {
            _session = session;
            _reader = reader;
            _writer = writer;
        }

        public void Link(
            ParentChildRelation relation, 
            IEnumerable<string> tags, 
            IBuiltComponentMapping mapping,
            string ardoqReferenceName,
            string searchFolder = null)
        {
            var component = _session.GetChildComponent(relation);

            List<global::Ardoq.Models.Tag> allTags = _reader.GetAllTagsInFolder(searchFolder).Result;
            var componentDict = new Dictionary<string, List<global::Ardoq.Models.Tag>>();
            foreach(var tag in allTags)
            {
                foreach(var compId in tag.Components)
                {
                    if (!componentDict.ContainsKey(compId))
                    {
                        componentDict[compId] = new List<global::Ardoq.Models.Tag>();
                    }
                    componentDict[compId].Add(tag);
                }
            }

            var componentIds = componentDict.Select(pair => pair.Key);
            foreach (var tag in tags)
            {
                var compsMatchingTag = componentDict
                    .Where(pair => pair.Value.Select(t => t.Name).Contains(tag))
                    .Select(pair => pair.Key);
                componentIds = componentIds.Intersect(compsMatchingTag);
            }

            var refType = _session.GetReferenceTypeForName(ardoqReferenceName);
            foreach (var targetComponentId in componentIds)
            {
                var refs = _session.GetAllSourceReferencesFromChild(relation);
                if (!refs.Any(r => r.Target == targetComponentId && r.Type == refType))
                {
                    _session.AddReference(refType, component.Id, targetComponentId);
                }
            }
        }
    }
}
