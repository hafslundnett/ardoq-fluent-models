using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ardoq;
using Ardoq.Models;
using ArdoqFluentModels.Search;

namespace ArdoqFluentModels.Ardoq
{
    public class ArdoqSearcher : IArdoqSearcher
    {
        private readonly IArdoqClient _client;
        private readonly ILogger _logger;

        public ArdoqSearcher(IArdoqClient client, ILogger logger)
        {
            _client = client;
            _logger = logger;
        }

        public async Task<IEnumerable<Component>> Search(SearchSpec spec)
        {
            List<Component> allComponents = await GetTargetComponentsFromArdoq(spec);

            var allTags = await _client.TagService.GetAllTags();

            bool started = false;
            return spec.Elements
                .Aggregate(
                    new List<Component>(),
                    (remaining, element) =>
                    {
                        IEnumerable<Component> comps;
                        if (started)
                        {
                            comps = element.Search(allTags, allComponents, remaining);
                        }
                        else
                        {
                            comps = element.Search(allTags, allComponents);
                        }

                        started = true;

                        return comps.ToList();
                    });
        }

        private async Task<List<Component>> GetTargetComponentsFromArdoq(SearchSpec spec)
        {
            List<Component> allComponents;
            if (string.IsNullOrEmpty(spec.SearchFolder))
            {
                allComponents = await _client.ComponentService.GetAllComponents();
            }
            else
            {
                var folder = await _client.FolderService.GetFolderByName(spec.SearchFolder);
                var componentCollectionTask = folder.Workspaces
                    .Select(workspaceId => _client.ComponentService.GetComponentsByWorkspace(workspaceId));
                var componentCollection = await Task.WhenAll(componentCollectionTask);

                allComponents = componentCollection.Aggregate(
                    new List<Component>(),
                    (list, components) =>
                    {
                        list.AddRange(components);
                        return list;
                    }
                );
            }

            return allComponents;
        }
    }
}
