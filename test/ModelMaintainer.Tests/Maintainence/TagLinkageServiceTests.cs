using System.Collections.Generic;
using System.Threading.Tasks;
using Ardoq.Models;
using ModelMaintainer.Ardoq;
using ModelMaintainer.Maintainence;
using ModelMaintainer.Mapping;
using ModelMaintainer.Tests.Model;
using Moq;
using Xunit;

namespace ModelMaintainer.Tests.Maintainence
{
    public class TagLinkageServiceTests
    {
        private readonly Mock<IArdoqSession> _sessionMock;
        private readonly Mock<IArdoqReader> _readerMock;
        private readonly Mock<IArdoqWriter> _writerMock;

        public TagLinkageServiceTests()
        {
            _sessionMock = new Mock<IArdoqSession>();
            _readerMock = new Mock<IArdoqReader>();
            _writerMock = new Mock<IArdoqWriter>();
        }

        [Fact]
        public void Link_HasComponentToLinkToNoPriorReference_WritesReferenceLink()
        {
            // Arrange
            var ardoqReferenceName = "ref-1";
            var refType = 99;
            var rootWorkspaceId = "root-workspace-1";
            var sourceComponentId = "source-1";
            var sourceComponent = new Component("MDM-RGtest", rootWorkspaceId, null) {Id = sourceComponentId };

            var targetComponentId = "target-1";

            var m = new ComponentMapping<ResourceGroup>("ResourceGroup")
                .WithKey(rg => rg.Name)
                .WithPreexistingHierarchyReference("ResourceGroups");

            var relation = new ParentChildRelation(null, new ResourceGroup {Name = "MDM-RGtest"});
            var tags = new List<string>{"environment-test", "mdm-storage-internal"};

            _sessionMock.Setup(s => s.GetChildComponent(relation))
                .Returns(sourceComponent);
            _sessionMock.Setup(s => s.GetReferenceTypeForName(ardoqReferenceName))
                .Returns(refType);
            _sessionMock.Setup(s => s.GetAllSourceReferencesFromChild(relation))
                .Returns(new List<Reference>());
            _readerMock.Setup(r => r.GetAllTagsInFolder(null))
                .Returns(Task.FromResult(new List<Tag>
                {
                    new Tag("environment-test", null, null){Components = new List<string>{targetComponentId}},
                    new Tag("mdm-storage-internal", null, null){Components = new List<string>{targetComponentId}},
                    new Tag("environment-test", null, null){Components = new List<string>{"some-other-component"}}
                }));

            var service = new TagLinkageService(_sessionMock.Object, _readerMock.Object, _writerMock.Object);

            // Act
            service.Link(relation, tags, m, ardoqReferenceName);

            // Assert
            _sessionMock.Verify(s => s.AddReference(refType, sourceComponentId, targetComponentId));
        }

        [Fact]
        public void Link_ReferenceAlreadyExists_WritesNothing()
        {
            // Arrange
            var ardoqReferenceName = "ref-1";
            var refType = 99;
            var rootWorkspaceId = "root-workspace-1";
            var sourceComponentId = "source-1";
            var sourceComponent = new Component("MDM-RGtest", rootWorkspaceId, null) { Id = sourceComponentId };

            var targetComponentId = "target-1";

            var m = new ComponentMapping<ResourceGroup>("ResourceGroup")
                .WithKey(rg => rg.Name)
                .WithPreexistingHierarchyReference("ResourceGroups");

            var relation = new ParentChildRelation(null, new ResourceGroup { Name = "MDM-RGtest" });
            var tags = new List<string> { "environment-test", "mdm-storage-internal" };

            _sessionMock.Setup(s => s.GetChildComponent(relation))
                .Returns(sourceComponent);
            _sessionMock.Setup(s => s.GetReferenceTypeForName(ardoqReferenceName))
                .Returns(refType);
            _readerMock.Setup(r => r.GetAllTagsInFolder(null))
                .Returns(Task.FromResult(new List<Tag>
                {
                    new Tag("environment-test", null, null){Components = new List<string>{targetComponentId}},
                    new Tag("mdm-storage-internal", null, null){Components = new List<string>{targetComponentId}}
                }));

            var existingReference = new Reference(null, null, sourceComponentId, targetComponentId, refType);
            _sessionMock.Setup(s => s.GetAllSourceReferencesFromChild(relation))
                .Returns(new List<Reference>{ existingReference });

            var service = new TagLinkageService(_sessionMock.Object, _readerMock.Object, _writerMock.Object);

            // Act
            service.Link(relation, tags, m, ardoqReferenceName);

            // Assert
            _sessionMock.Verify(s => s.AddReference(refType, sourceComponentId, targetComponentId), Times.Never);
        }

        [Fact]
        public void Link_NoComponentsMatchTags_WritesNothing()
        {
            // Arrange
            var ardoqReferenceName = "ref-1";
            var refType = 99;
            var rootWorkspaceId = "root-workspace-1";
            var sourceComponentId = "source-1";
            var sourceComponent = new Component("MDM-RGtest", rootWorkspaceId, null) { Id = sourceComponentId };

            var targetComponentId = "target-1";

            var m = new ComponentMapping<ResourceGroup>("ResourceGroup")
                .WithKey(rg => rg.Name)
                .WithPreexistingHierarchyReference("ResourceGroups");

            var relation = new ParentChildRelation(null, new ResourceGroup { Name = "MDM-RGtest" });
            var tags = new List<string> { "environment-test", "mdm-storage-internal" };

            _sessionMock.Setup(s => s.GetChildComponent(relation))
                .Returns(sourceComponent);
            _sessionMock.Setup(s => s.GetReferenceTypeForName(ardoqReferenceName))
                .Returns(refType);
            _sessionMock.Setup(s => s.GetAllSourceReferencesFromChild(relation))
                .Returns(new List<Reference>());
            _readerMock.Setup(r => r.GetAllTagsInFolder(null))
                .Returns(Task.FromResult(new List<Tag>
                {
                    new Tag("environment-test", null, null){Components = new List<string>{targetComponentId}},
                    new Tag("mdm-storage-internal", null, null){Components = new List<string>{"some-other-component"}},
                    new Tag("environment-test", null, null){Components = new List<string>{"some-other-component-2"}}
                }));

            var service = new TagLinkageService(_sessionMock.Object, _readerMock.Object, _writerMock.Object);

            // Act
            service.Link(relation, tags, m, ardoqReferenceName);

            // Assert
            _sessionMock.Verify(s => s.AddReference(refType, sourceComponentId, targetComponentId), Times.Never);
        }
    }
}
