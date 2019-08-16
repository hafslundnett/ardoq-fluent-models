using System;
using System.Collections.Generic;
using System.Net.Http;
using Ardoq;
using System.Linq;
using ArdoqFluentModels.Mapping;
using ArdoqFluentModels.Ardoq;
using ArdoqFluentModels.Maintainence;

namespace ArdoqFluentModels
{
    public class ArdoqModelMappingBuilder : IModelAccessor
    {
        private readonly string _url;
        private readonly string _token;
        private readonly string _organization;
        private Dictionary<Type, IBuiltComponentMapping> _componentMappings = new Dictionary<Type, IBuiltComponentMapping>();

        private IArdoqReader _reader;
        private IArdoqWriter _writer;
        private IArdoqSearcher _searcher;
        private IArdoqWorkspaceCreator _workspaceCreator;
        private IExternalLinkageService _externalLinkageService;
        private bool _safeMode = false;
        private ILogger _logger;

        public string WorkspaceName { get; private set; }
        public string FolderName { get; private set; }

        public string TemplateName { get; private set; }

        public IEnumerable<IBuiltComponentMapping> ComponentMappings => _componentMappings.Values;

        public ArdoqModelMappingBuilder(string url, string token, string organization)
        {
            _url = url;
            _token = token;
            _organization = organization;
        }

        public ArdoqModelMappingBuilder WithWorkspaceNamed(string workspaceName)
        {
            WorkspaceName = workspaceName;
            return this;
        }

        public ArdoqModelMappingBuilder WithFolderNamed(string folderName)
        {
            FolderName = folderName;
            return this;
        }

        public ArdoqModelMappingBuilder WithTemplate(string componentModel)
        {
            TemplateName = componentModel;
            return this;
        }

        public ArdoqModelMappingBuilder WithReader(IArdoqReader reader)
        {
            _reader = reader;
            return this;
        }

        public ArdoqModelMappingBuilder WithWriter(IArdoqWriter writer)
        {
            _writer = writer;
            return this;
        }

        public ArdoqModelMappingBuilder WithSearcher(IArdoqSearcher searcher)
        {
            _searcher = searcher;
            return this;
        }

        public ArdoqModelMappingBuilder WithWorkspaceCreator(IArdoqWorkspaceCreator workspaceCreator)
        {
            _workspaceCreator = workspaceCreator;
            return this;
        }

        public ArdoqModelMappingBuilder WithSafeMode(bool safeMode)
        {
            _safeMode = safeMode;
            return this;
        }

        public ArdoqModelMappingBuilder WithExternalLinkageService(IExternalLinkageService externalLinkageService)
        {
            _externalLinkageService = externalLinkageService;
            return this;
        }

        public ArdoqModelMappingBuilder WithLogger(ILogger logger)
        {
            _logger = logger;
            return this;
        }

        public IComponentMapping<TSource> AddComponentMapping<TSource>(string ardoqComponentName)
        {
            if (_componentMappings.ContainsKey(typeof(TSource)))
            {
                throw new ArgumentException($"Source type {typeof(TSource).FullName} already registered.");
            }

            var mapping = new ComponentMapping<TSource>(ardoqComponentName);
            _componentMappings.Add(typeof(TSource), mapping);

            return mapping;
        }

        public IBuiltComponentMapping GetBuildableComponentMapping<TSource>()
        {
            return _componentMappings[typeof(TSource)];
        }

        public IMaintainenceSession Build()
        {
            var logger = _logger ?? new ConsoleLogger();
            var reader = _reader ?? new ArdoqReader(_url, _token, _organization, logger);
            var writer = _writer ?? new ArdoqWriter(_url, _token, _organization, logger);
            var searcher = _searcher ?? new ArdoqSearcher(new ArdoqClient(new HttpClient(), _url, _token, _organization), logger);
            var creator = _workspaceCreator ?? new ArdoqWorkspaceCreator(reader, writer);
            var linkageService = _externalLinkageService ?? new ExternalLinkageService(reader, writer);
            SortComponentMappings();
            return new MaintainenceSession(this, reader, writer, creator, linkageService, searcher, _safeMode, logger);
        }

        private void SortComponentMappings()
        {
            ComputeParents();

            var sortedDict = new Dictionary<Type, IBuiltComponentMapping>();
            var topNodes = FindComponentMappingsWithParent(null);
            SortSubTree(sortedDict, topNodes);

            _componentMappings = sortedDict;
        }

        private Dictionary<Type, IBuiltComponentMapping> SortSubTree(Dictionary<Type, IBuiltComponentMapping> sortedDict, 
            IEnumerable<KeyValuePair<Type, IBuiltComponentMapping>> subTreeTopNodes)
        {
            // First add all the topnodes in this subtree to the sorted dict
            foreach (var node in subTreeTopNodes)
            {
                sortedDict.Add(node.Key, node.Value);
            }

            // then find the top level nodes on the next level an recursivly call this method
            foreach (var node in subTreeTopNodes)
            {
                var nextLevelTopNodes = FindComponentMappingsWithParent(node.Value);
                var sortedSubTree = SortSubTree(sortedDict, nextLevelTopNodes);
            }

            return sortedDict;
        }

        private IEnumerable<KeyValuePair<Type, IBuiltComponentMapping>> FindComponentMappingsWithParent(IBuiltComponentMapping parent)
        {
            return _componentMappings.Where(cm => cm.Value.GetParent() == parent).Select(cm => cm);
        }

        private void ComputeParents()
        {
            foreach (var cm in _componentMappings.Values)
            {
                Type parentType = cm.GetParentType();
                if (parentType != null)
                {
                    if (cm.GetParent() != null)
                    {
                        throw new Exception($"Validation Error. The type {cm.ArdoqComponentTypeName} has multiple parent");
                    }
                    cm.SetParent(_componentMappings[parentType]);
                }
                var childrenTypes = cm.GetChildrenTypes();
                foreach (var childType in childrenTypes)
                {
                    var child = _componentMappings[childType];
                    child.SetParent(cm);
                }
            }
        }

        public string GetComponentType(Type type)
        {
            if (!_componentMappings.ContainsKey(type))
            {
                throw new InvalidOperationException($"Type {type.FullName} not registered.");
            }

            return _componentMappings[type].ArdoqComponentTypeName;
        }
    }
}
