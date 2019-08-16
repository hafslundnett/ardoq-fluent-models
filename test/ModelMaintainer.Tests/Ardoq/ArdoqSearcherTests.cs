using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Ardoq;
using Ardoq.Models;
using Ardoq.Service.Interface;
using ArdoqFluentModels;
using ArdoqFluentModels.Ardoq;
using ArdoqFluentModels.Search;
using Moq;
using Xunit;

namespace ModelMaintainer.Tests.Ardoq
{
    public class ArdoqSearcherTests
    {
        private readonly Mock<IArdoqClient> _clientMock;
        private readonly Mock<ITagService> _tagServiceMock;
        private readonly Mock<IComponentService> _componentServiceMock;

        public ArdoqSearcherTests()
        {
            _clientMock = new Mock<IArdoqClient>();
            _tagServiceMock = new Mock<ITagService>();
            _componentServiceMock = new Mock<IComponentService>();
        }

        [Fact]
        public void Search_OneSearchTerm_FindsExpectedComponents()
        {
            // Arrange
            var comp = new Component("my-comp", null, null) {Id = "my-id", Type = "MyType"};
            var spec = new SearchSpec(null);
            spec.AddElement(new ComponentTypeAndFieldSearchSpecElement{ComponentType = "MyType"});

            _tagServiceMock.Setup(ts => ts.GetAllTags(null))
                .Returns(Task.FromResult(new List<Tag>()));
            _componentServiceMock.Setup(cs => cs.GetAllComponents(null))
                .Returns(Task.FromResult(new List<Component> {comp}));

            var searcher = GetSearcher();

            // Act
            var found = searcher.Search(spec).Result;

            // Assert
            Assert.Single(found);
        }

        [Fact]
        public void Search_Hierarchy_FindsExpectedComponents()
        {
            // Arrange
            var parentId = "parent-1";
            var component = new Component("myName", null, null)
            {
                Id = parentId,
                Type = "myType",
                Fields = new Dictionary<string, object> { ["k1"] = 99 }
            };
            

            _tagServiceMock.Setup(ts => ts.GetAllTags(null))
                .Returns(Task.FromResult(new List<Tag>{new Tag("tag1", null, null){Components = new List<string>{parentId}}}));
            _componentServiceMock.Setup(cs => cs.GetAllComponents(null))
                .Returns(Task.FromResult(new List<Component> { component }));

            var spec = new SearchSpec(null);
            var tagSearch = new TagSearchSpecElement();
            tagSearch.AddTags(new List<string> { "tag1" });
            spec.AddElement(tagSearch);

            var typeAndFieldSearchSpec = new ComponentTypeAndFieldSearchSpecElement { ComponentType = "myType" };
            typeAndFieldSearchSpec.AddFieldFilter("k1", 99);

            spec.AddElement(typeAndFieldSearchSpec);
            var searcher = GetSearcher();

            // Act
            var found = searcher.Search(spec).Result;

            // Assert
            Assert.Single(found);
            Assert.Equal("myName", found.Single().Name);
        }

        [Fact]
        public void Search_HierarchyNoHits_ReturnsEmptyList()
        {
            // Arrange
            var parentId = "parent-1";
            var childId = "child-1";
            var parentComponent = new Component("parent", null, null)
            {
                Id = parentId,
                Type = "ParentType",
                Children = new List<string> { childId }
            };

            var childComponent = new Component("child", null, null)
            {
                Id = childId,
                Type = "ChildType",
                Fields = new Dictionary<string, object> { ["k1"] = 66 }
            };


            _tagServiceMock.Setup(ts => ts.GetAllTags(null))
                .Returns(Task.FromResult(new List<Tag> { new Tag("tag1", null, null) { Components = new List<string> { parentId } } }));
            _componentServiceMock.Setup(cs => cs.GetAllComponents(null))
                .Returns(Task.FromResult(new List<Component> { parentComponent, childComponent }));

            var spec = new SearchSpec(null);
            var tagSpec = new TagSearchSpecElement();
            tagSpec.AddTags(new List<string>{"tag1"});
            spec.AddElement(tagSpec);

            var typeAndFieldSearchSpec = new ComponentTypeAndFieldSearchSpecElement { ComponentType = "ChildType" };
            typeAndFieldSearchSpec.AddFieldFilter("k1", 99);

            spec.AddElement(typeAndFieldSearchSpec);
            var searcher = GetSearcher();

            // Act
            var found = searcher.Search(spec).Result;

            // Assert
            Assert.Empty(found);
        }

        [Fact(Skip = "Integration")]
        [Trait("Type", "Integration")]
        private void Search_IntegrationTest()
        {
            // Arrange
            var url = "";
            var token = "";
            var organization = "";
            var client = new ArdoqClient(new HttpClient(), url, token, organization);

            var searcher = new ArdoqSearcher(client, new ConsoleLogger());

            var tag = "integration-test";

            var spec = new SearchSpec(null);
            var tagElement = new TagSearchSpecElement();
            tagElement.AddTags(new List<string>{tag});
            spec.AddElement(tagElement);

            // Act
            var results = searcher.Search(spec).Result;
        }

        private ArdoqSearcher GetSearcher()
        {
            _clientMock.Setup(c => c.TagService).Returns(_tagServiceMock.Object);
            _clientMock.Setup(c => c.ComponentService).Returns(_componentServiceMock.Object);

            return new ArdoqSearcher(_clientMock.Object, new ConsoleLogger());
        }
    }
}
