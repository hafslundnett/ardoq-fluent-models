using System;
using System.Collections.Generic;
using System.Linq;
using Ardoq.Models;
using ArdoqFluentModels.Search;
using Xunit;

namespace ModelMaintainer.Tests.Search
{
    public class ComponentTypeAndFieldSearchSpecElementTests
    {
        [Fact]
        public void Search_HasHits_ReturnsExpectedComponents()
        {
            // Arrange
            var component = new Component("my-comp", null, null)
            {
                Fields = new Dictionary<string, object>
                {
                    ["k1"] = "v1",
                    ["k2"] = new DateTime(2018, 11, 8, 11, 11, 11),
                    ["k3"] = 111,
                    ["k4"] = long.MaxValue
                }
            };

            var spec = new ComponentTypeAndFieldSearchSpecElement();
            spec.AddFieldFilter("k1", "v1");
            spec.AddFieldFilter("k2", new DateTime(2018, 11, 8, 11, 11, 11));
            spec.AddFieldFilter("k3", 111);
            spec.AddFieldFilter("k4", long.MaxValue);

            // Act
            var found = spec.Search(null, new List<Component> {component});

            // Assert
            Assert.Single(found);
        }

        [Fact]
        public void Search_NoHits_ReturnsEmptyList()
        {
            // Arrange
            var component = new Component("my-comp", null, null)
            {
                Fields = new Dictionary<string, object>
                {
                    ["k1"] = "v1",
                    ["k2"] = new DateTime(2018, 11, 8, 11, 11, 11),
                    ["k3"] = 111,
                    ["k4"] = long.MaxValue
                }
            };

            var spec = new ComponentTypeAndFieldSearchSpecElement();
            spec.AddFieldFilter("k1", "v1");
            spec.AddFieldFilter("k2", new DateTime(2018, 11, 8, 11, 11, 11));
            spec.AddFieldFilter("k3", 111);
            spec.AddFieldFilter("k4", 90000099);

            // Act
            var found = spec.Search(null, new List<Component> { component });

            // Assert
            Assert.Empty(found);
        }

        [Fact]
        public void Search_WithNameFilter_ReturnsExpectedComponents()
        {
            // Arrange
            var name = "the-name-we-are-looking-for";
            var components = new List<Component>
            {
                new Component("my-comp", null, null)
                {
                    Type = "SomeType",
                    Fields = new Dictionary<string, object>
                    {
                        ["k1"] = "v1",
                    }
                },
                new Component(name, null, null)
                {
                    Type = "SomeOtherType",
                    Fields = new Dictionary<string, object>
                    {
                        ["k1"] = "v1",
                    }
                }
            };

            var spec = new ComponentTypeAndFieldSearchSpecElement { Name = name };

            // Act
            var found = spec.Search(null, components);

            // Assert
            Assert.Single(found);
            Assert.Equal(name, found.Single().Name);
        }

        [Fact]
        public void Search_WithTypeFilter_ReturnsExpectedComponents()
        {
            // Arrange
            var typeInFilter = "TypeInFilter";
            var components = new List<Component>
            {
                new Component("my-comp", null, null)
                {
                    Type = typeInFilter,
                    Fields = new Dictionary<string, object>
                    {
                        ["k1"] = "v1",
                    }
                },
                new Component("my-other-comp", null, null)
                {
                    Type = "SomeOtherType",
                    Fields = new Dictionary<string, object>
                    {
                        ["k1"] = "v1",
                    }
                }
            };

            var spec = new ComponentTypeAndFieldSearchSpecElement{ComponentType = typeInFilter};
            spec.AddFieldFilter("k1", "v1");

            // Act
            var found = spec.Search(null, components);

            // Assert
            Assert.Single(found);
            Assert.Equal("my-comp", found.Single().Name);
        }

        [Fact]
        public void Search_WithJustTypeFilter_ReturnsExpectedComponents()
        {
            // Arrange
            var typeInFilter = "TypeInFilter";
            var components = new List<Component>
            {
                new Component("my-comp", null, null)
                {
                    Type = typeInFilter
                },
                new Component("my-other-comp", null, null)
                {
                    Type = "SomeOtherType"
                }
            };
            var spec = new ComponentTypeAndFieldSearchSpecElement { ComponentType = typeInFilter };

            // Act
            var found = spec.Search(null, components);

            // Assert
            Assert.Single(found);
            Assert.Equal("my-comp", found.Single().Name);
        }

        [Fact]
        public void Search_WithTypeFilterAndInSearchSpace_ReturnsExpectedComponents()
        {
            // Arrange
            var typeInFilter = "TypeInFilter";
            var compId = "my-comp-1";
            Component targetComponent = new Component("my-comp", null, null)
            {
                Id = compId,
                Type = typeInFilter,
                Fields = new Dictionary<string, object>
                {
                    ["k1"] = "v1",
                }
            };
            Component otherComponent = new Component("my-other-comp", null, null)
            {
                Type = "SomeOtherType",
                Fields = new Dictionary<string, object>
                {
                    ["k1"] = "v1",
                }
            };
            var components = new List<Component>
            {
                targetComponent,
                otherComponent
            };

            var spec = new ComponentTypeAndFieldSearchSpecElement { ComponentType = typeInFilter };
            spec.AddFieldFilter("k1", "v1");

            // Act
            var found = spec.Search(null, components, new List<Component> { targetComponent });

            // Assert
            Assert.Single(found);
            Assert.Equal("my-comp", found.Single().Name);
        }

        [Fact]
        public void Search_WithTypeFilterNotInSearchSpace_ReturnsEmptyLists()
        {
            // Arrange
            var typeInFilter = "TypeInFilter";
            var compId = "my-comp-1";
            var components = new List<Component>
            {
                new Component("my-comp", null, null)
                {
                    Id = compId,
                    Type = typeInFilter,
                    Fields = new Dictionary<string, object>
                    {
                        ["k1"] = "v1",
                    }
                },
                new Component("my-other-comp", null, null)
                {
                    Type = "SomeOtherType",
                    Fields = new Dictionary<string, object>
                    {
                        ["k1"] = "v1",
                    }
                }
            };

            var spec = new ComponentTypeAndFieldSearchSpecElement { ComponentType = typeInFilter };
            spec.AddFieldFilter("k1", "v1");

            // Act
            var found = spec.Search(null, components, new List<Component> { new Component(null, null, null) { Id = "some-other-component" } });

            // Assert
            Assert.Empty(found);
        }

        [Fact]
        public void Search_WithNameAndParentFilter_ReturnsExpectedComponents()
        {
            // Arrange
            var name = "the-name-we-are-looking-for";
            var parentName = "the-parentName-we-are-looking-for";

            var parentId = "parentId";
            var components = new List<Component>
            {
                new Component(parentName, null, null)
                {
                    Id = parentId,
                    Type = "SomeType",
                    Fields = new Dictionary<string, object>
                    {
                        ["k1"] = "v1",
                    }
                },
                new Component(name, null, null)
                {
                    Parent = parentId,
                    Type = "SomeType",
                    Fields = new Dictionary<string, object>
                    {
                        ["k1"] = "v1",
                    }
                },
                new Component(name, null, null)
                {
                    Type = "SomeOtherType",
                    Fields = new Dictionary<string, object>
                    {
                        ["k1"] = "v1",
                    }
                }
            };

            var spec = new ComponentTypeAndFieldSearchSpecElement { Name = name, ParentNameList = new List<string> { parentName } };

            // Act
            var found = spec.Search(null, components);

            // Assert
            Assert.Single(found);
            Assert.Equal(name, found.Single().Name);
            Assert.Equal(parentId, found.Single().Parent);
        }

        [Fact]
        public void Search_EmptySearchSpec_ReturnsNothing()
        {
            // Arrange
            var components = new List<Component>
            {
                new Component("some name", null, null)
                {
                    Type = "SomeOtherType",
                    Fields = new Dictionary<string, object>
                    {
                        ["k1"] = "v1",
                    }
                }
            };

            var spec = new ComponentTypeAndFieldSearchSpecElement { };

            // Act
            var found = spec.Search(null, components);

            // Assert
            Assert.Empty(found);
        }

        [Fact]
        public void Search_WithNameAndParentFilter_MultipleTargets_ReturnsExpectedComponents()
        {
            // Arrange
            var name1 = "the-name-we-are-looking-for";
            var name2 = "the-other-name-we-are-looking-for";
            var parentName = "the-parentName-we-are-looking-for";

            var parentId = "parentId";
            var components = new List<Component>
            {
                new Component(parentName, null, null)
                {
                    Id = parentId,
                    Type = "SomeType",
                    Fields = new Dictionary<string, object>
                    {
                        ["k1"] = "v1",
                    }
                },
                new Component(name1, null, null)
                {
                    Parent = parentId,
                    Type = "SomeType",
                    Fields = new Dictionary<string, object>
                    {
                        ["k1"] = "v1",
                    }
                },
                new Component(name2, null, null)
                {
                    Parent = parentId,
                    Type = "SomeOtherType",
                    Fields = new Dictionary<string, object>
                    {
                        ["k1"] = "v1",
                    }
                }
            };

            var spec = new ComponentTypeAndFieldSearchSpecElement
            {
                NameList = new List<string> { name1, name2 },
                ParentNameList = new List<string> { parentName }
            };

            // Act
            var found = spec.Search(null, components).ToList();

            // Assert
            Assert.Equal(2, found.Count());
            Assert.Equal(name1, found[0].Name);
            Assert.Equal(name2, found[1].Name);
        }
    }
}
