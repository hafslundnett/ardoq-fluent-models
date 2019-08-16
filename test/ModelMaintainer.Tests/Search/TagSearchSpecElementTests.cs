using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ardoq.Models;
using ModelMaintainer.Search;
using Xunit;

namespace ModelMaintainer.Tests.Search
{
    public class TagSearchSpecElementTests
    {
        [Fact]
        public void Search_HasHits_ReturnsExpectedComponents()
        {
            // Assert
            var searchTag = "tag1";
            var componentId = "comp1";
            var tags = new List<Tag>
            {
                new Tag(searchTag, null, null) {Components = new List<string>{componentId}}
            };

            var components = new List<Component>{new Component("n1", null, null){Id = componentId}};

            var tagSearch = new TagSearchSpecElement();
            tagSearch.AddTags(new List<string>{searchTag});

            // Act
            var found = tagSearch.Search(tags, components);

            // Assert
            Assert.Single(found);
            Assert.Equal("n1", found.First().Name);
        }

        [Fact]
        public void Search_HasHitsAndIsInSearchSpace_ReturnsExpectedComponents()
        {
            // Assert
            var searchTag = "tag1";
            var componentId = "comp1";
            var tags = new List<Tag>
            {
                new Tag(searchTag, null, null) {Components = new List<string>{componentId}}
            };

            var components = new List<Component> { new Component("n1", null, null) { Id = componentId } };
            var searchSpace = new List<Component> { new Component("n1", null, null) { Id = componentId } };

            var tagSearch = new TagSearchSpecElement();
            tagSearch.AddTags(new List<string> { searchTag });

            // Act
            var found = tagSearch.Search(tags, components, searchSpace);

            // Assert
            Assert.Single(found);
            Assert.Equal("n1", found.First().Name);
        }

        [Fact]
        public void Search_SeveralTagsHasHits_ReturnsExpectedComponents()
        {
            // Assert
            var searchTag = "tag1";
            var componentId = "comp1";
            var otherComponent = "other-comp";
            var c2 = "c-2";
            var c3 = "c-3";
            var c4 = "c-4";

            var tags = new List<Tag>
            {
                new Tag(searchTag, null, null) {Components = new List<string>{componentId, c2, c3, c4, otherComponent}},
                new Tag("tag2", null, null) {Components = new List<string>{componentId, c2, c3, otherComponent}},
                new Tag("tag3", null, null) {Components = new List<string>{componentId, c2, c4, otherComponent}},
                new Tag("tag4", null, null) {Components = new List<string>{componentId, c3, c4, otherComponent}}
            };

            var components = new List<Component>
            {
                new Component("n1", null, null) { Id = componentId },
                new Component("n2", null, null) { Id = c2 },
                new Component("n3", null, null) { Id = c3 },
                new Component("n4", null, null) { Id = c4 },
                new Component("other", null, null) { Id = otherComponent },
            };


            var tagSearch = new TagSearchSpecElement();
            tagSearch.AddTags(new List<string> { searchTag, "tag2", "tag3", "tag4" });

            // Act
            var found = tagSearch.Search(tags, components);

            // Assert
            Assert.Equal(2, found.Count());
            Assert.Contains(found, c => c.Id == componentId);
            Assert.Contains(found, c => c.Id == otherComponent);
        }

        [Fact]
        public void Search_HasHitsButNotInSearchSpace_ReturnsExpectedComponents()
        {
            // Assert
            var searchTag = "tag1";
            var componentId = "comp1";
            var tags = new List<Tag>
            {
                new Tag(searchTag, null, null) {Components = new List<string>{componentId}}
            };

            var components = new List<Component> { new Component("n1", null, null) { Id = componentId } };
            var searchSpace = new List<Component> { new Component("n1", null, null) { Id = "some-other-component-id" } };

            var tagSearch = new TagSearchSpecElement();
            tagSearch.AddTags(new List<string> { searchTag });

            // Act
            var found = tagSearch.Search(tags, components, searchSpace);

            // Assert
            Assert.Empty(found);
        }

        [Fact]
        public void Search_NoHits_ReturnsEmptyList()
        {
            // Assert
            var searchTag = "tag1";
            var componentId = "comp1";
            var tags = new List<Tag>
            {
                new Tag(searchTag, null, null) {Components = new List<string>{componentId}}
            };

            var components = new List<Component> { new Component("n1", null, null) { Id = componentId } };

            var tagSearch = new TagSearchSpecElement();
            tagSearch.AddTags(new List<string>{ "some-other-tag" });

            // Act
            var found = tagSearch.Search(tags, components);

            // Assert
            Assert.Empty(found);

        }
    }
}
