using System.Collections.Generic;
using System.IO;
using ModelMaintainer.Ardoq;
using Newtonsoft.Json;
using Xunit;

namespace ModelMaintainer.Tests.Ardoq
{
    public class ArdoqReaderTests
    {
        private readonly string _url;
        private readonly string _org;
        private readonly string _token;

        public ArdoqReaderTests()
        {
            var json = File.ReadAllText(@"secrets.json");
            var map = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

            _url = map["url"];
            _org = map["org"];
            _token = map["token"];
        }

        [Fact(Skip = "Integration")]
        [Trait("Type", "Integration")]
        public void GetWorkspaceNamed_Exists_ReturnsWorkspace()
        {
            // Arrange
            var name = "Blank Workspace";
            var reader = new ArdoqReader(_url, _token, _org, new ConsoleLogger());

            // Act
            var workspace = reader.GetWorkspaceNamed(name).Result;

            // Assert
            Assert.NotNull(workspace);
        }

        [Fact(Skip = "Integration")]
        [Trait("Type", "Integration")]
        public void GetWorkspaceNamed_DoesNotExist_ReturnsWorkspace()
        {
            // Arrange
            var name = "Non-existing workspace name";
            var reader = new ArdoqReader(_url, _token, _org, new ConsoleLogger());

            // Act
            var workspace = reader.GetWorkspaceNamed(name).Result;

            // Assert
            Assert.Null(workspace);
        }

        [Fact(Skip = "Integration")]
        [Trait("Type", "Integration")]
        public void GetFolder_Exists_ReturnsFolder()
        {
            // Arrange
            var name = "1. Prosesser";
            var reader = new ArdoqReader(_url, _token, _org, new ConsoleLogger());

            // Act
            var folder = reader.GetFolder(name).Result;

            // Assert
            Assert.NotNull(folder);
        }

        [Fact(Skip = "Integration")]
        [Trait("Type", "Integration")]
        public void GetFolder_DoesNotExist_ReturnsNull()
        {
            // Arrange
            var name = "Non-existing folder";
            var reader = new ArdoqReader(_url, _token, _org, new ConsoleLogger());

            // Act
            var folder = reader.GetFolder(name).Result;

            // Assert
            Assert.Null(folder);
        }
    }
}
