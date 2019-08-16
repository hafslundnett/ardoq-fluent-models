using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ardoq.Models;
using ModelMaintainer.Ardoq;
using ModelMaintainer.Mapping;
using ModelMaintainer.Utils;

namespace ModelMaintainer.Maintainence
{
    public class MaintainenceSession : IMaintainenceSession
    {
        private readonly ArdoqModelMappingBuilder _builder;
        private readonly IArdoqReader _reader;
        private readonly IArdoqWriter _writer;
        private readonly IArdoqWorkspaceCreator _workspaceCreator;
        private readonly IExternalLinkageService _externalLinkageService;
        private readonly IArdoqSearcher _searcher;
        private readonly bool _safeMode;
        private readonly ILogger _logger;

        public MaintainenceSession(
            ArdoqModelMappingBuilder builder,
            IArdoqReader reader,
            IArdoqWriter writer,
            IArdoqWorkspaceCreator workspaceCreator,
            IExternalLinkageService externalLinkageService,
            IArdoqSearcher searcher,
            bool safeMode,
            ILogger logger)
        {
            _builder = builder;
            _reader = reader;
            _writer = writer;
            _workspaceCreator = workspaceCreator;
            _externalLinkageService = externalLinkageService;
            _searcher = searcher;
            _safeMode = safeMode;
            _logger = logger;
        }

        public async Task Run(ISourceModelProvider modelProvider)
        {
            //var creator = new ArdoqWorkspaceCreator(_reader, _writer);
            var workspace = await _workspaceCreator.CreateWorkspaceIfMissing(_builder.FolderName, _builder.TemplateName, _builder.WorkspaceName);
            var session = new ArdoqSession(workspace.Id, _reader, _writer);

            var mappingsMap = _builder.ComponentMappings.Aggregate(
                new Dictionary<Type, IBuiltComponentMapping>(),
                (map, m) =>
                {
                    map[m.SourceType] = m;
                    return map;
                });

            var finder = new ParentChildRelationFinder(mappingsMap);
            var hierarchyBuilder = new ParentChildRelationHierarchyBuilder(mappingsMap);

            var allSourceObjects = modelProvider.GetSourceModel().ToList();
            var relations = finder.FindRelations(allSourceObjects);
            var hierarchies = hierarchyBuilder.BuildRelationHierarchies(relations).ToList();

            _logger.LogMessage("Starting component maintanence phase.");

            var created = 0;
            var updated = 0;
            var tagUpdated = 0;
            var deleted = 0;

            foreach (var hierarchy in hierarchies)
            {
                var maintainer = new ComponentHierarchyMaintainer(mappingsMap, session);
                created += maintainer.AddMissingComponents(hierarchy);
                updated += maintainer.UpdateComponents(hierarchy);
                tagUpdated += maintainer.UpdateComponentTags(hierarchy);
            }

            if (!SafeMode)
            {
                deleted = new ComponentHierarchyMaintainer(mappingsMap, session).DeleteComponents(hierarchies);
            }

            _logger.LogMessage($"Component maintanence phase complete. Created: {created} Updated: {updated} Tag updated: {tagUpdated} Deleted: {deleted} .");

            LinkInternally(relations, workspace, session);
            LinkExternally(relations, session);
            LinkByTags(relations, session);
            LinkBySearch(relations, session);
        }


        public string GetComponentType(Type sourceType)
        {
            var mapping = _builder.ComponentMappings.SingleOrDefault(cm => cm.SourceType == sourceType);
            return mapping?.ArdoqComponentTypeName;
        }

        public string GetKeyForInstance(object instance)
        {
            var mapping = _builder.ComponentMappings.SingleOrDefault(cm => cm.SourceType == instance?.GetType());
            if (mapping == null)
            {
                return null;
            }

            return mapping.KeyGetter.GetMethod.Invoke(instance, new object[] { }).ToString();
        }

        public bool IsSafeMode => SafeMode;

        public bool SafeMode => _safeMode;

        private void LinkInternally(IEnumerable<ParentChildRelation> allRelations, Workspace workspace, IArdoqSession session)
        {
            _logger.LogMessage("Starting internal linkage phase.");

            var createdCount = 0;
            var removedCount = 0;

            foreach (var mapping in _builder.ComponentMappings)
            {
                var linker = new ObjectReferenceLinker(mapping, workspace, session, this);
                var tuple = linker.LinkAll(allRelations, allRelations.Where(o => o.Child.GetType() == mapping.SourceType));
                createdCount += tuple.Item1;
                removedCount += tuple.Item2;
            }

            _logger.LogMessage($"Internal linkage phase complete. References added: {createdCount} Removed: {removedCount}");
        }

        private void LinkExternally(IEnumerable<ParentChildRelation> relations, IArdoqSession session)
        {
            foreach (var mapping in _builder.ComponentMappings.Where(cm => cm.ExternalReferenceSpecifications.Any()))
            {
                var rels = relations.Where(obj => obj.Child.GetType() == mapping.SourceType).ToList();
                foreach (var referenceSpecification in mapping.ExternalReferenceSpecifications)
                {
                    _externalLinkageService.LinkAll(referenceSpecification, rels, session, this);
                }
            }
        }

        private void LinkByTags(IEnumerable<ParentChildRelation> relations, IArdoqSession session)
        {
            var linker = new TagLinkageService(session, _reader, _writer);

            foreach (var mapping in _builder.ComponentMappings.Where(cm => cm.TagReferenceGetters.Any()))
            {
                var rels = relations.Where(rel => rel.Child.GetType() == mapping.SourceType).ToList();
                foreach (var tagReferenceGetter in mapping.TagReferenceGetters)
                {
                    foreach (var rel in rels)
                    {
                        var tuple = tagReferenceGetter.GetTags(rel.Child);
                        if (tuple.Item1)
                        {
                            linker.Link(rel, tuple.Item2, mapping, tagReferenceGetter.ArdoqReferenceName, 
                                tagReferenceGetter.GetSearchFolder());
                        }
                    }
                }
            }
        }

        private void LinkBySearch(IEnumerable<ParentChildRelation> relations, IArdoqSession session)
        {
            foreach (var mapping in _builder.ComponentMappings.Where(cm => cm.SearchReferenceBuilders.Any()))
            {
                var rels = relations.Where(obj => obj.Child.GetType() == mapping.SourceType).ToList();
                if (!rels.Any())
                {
                    return;
                }

                foreach (var builder in mapping.SearchReferenceBuilders)
                {
                    var linkageService = new SearchLinkageService(
                        _reader,
                        _writer,
                        session,
                        _searcher,
                        mapping,
                        builder.Item1,
                        _logger);

                    linkageService.LinkBySearch(builder.Item2, rels);
                }
            }
        }
    }
}
