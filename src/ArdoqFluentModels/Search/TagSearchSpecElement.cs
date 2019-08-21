using System.Collections.Generic;
using System.Linq;
using Ardoq.Models;

namespace ArdoqFluentModels.Search
{
    public class TagSearchSpecElement : SearchSpecElementBase
    {
        private readonly List<string> _tags = new List<string>();

        public IEnumerable<string> Tags => _tags;

        public void AddTags(IEnumerable<string> tags)
        {
            _tags.AddRange(tags);
        }

        protected override IEnumerable<Component> SearchCore(IEnumerable<Tag> tags, IEnumerable<Component> allComps)
        {
            var allTags = tags.ToList();
            var allComponents = allComps.ToList();

            List<Component> remaining = null;
            foreach (var tag in _tags)
            {
                var currentComponents = GetComponentsForSingleTag(tag, allTags, allComponents).ToList();
                if (!currentComponents.Any())
                {
                    return new List<Component>();
                }

                if (remaining == null)
                {
                    remaining = currentComponents.ToList();
                    continue;
                }

                remaining = remaining.Intersect(currentComponents).ToList();

                if (!remaining.Any())
                {
                    return remaining;
                }
            }

            return remaining;
        }

        private IEnumerable<Component> GetComponentsForSingleTag(string tag, IEnumerable<Tag> allTags, IEnumerable<Component> allComponents)
        {
            return allTags.Where(t => t.Name == tag)
                .Aggregate(
                    new List<Component>(),
                    (accumulator, t) =>
                    {
                        var taggedComponents = t.Components.Select(cid => allComponents.Single(c => c.Id == cid));
                        accumulator.AddRange(taggedComponents);

                        return accumulator;
                    })
                .GroupBy(c => c.Id)
                .Select(g => g.First());
        }
    }
}
