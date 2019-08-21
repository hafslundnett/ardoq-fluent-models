using System;
using System.Collections.Generic;
using System.Linq;
using ArdoqFluentModels.Utils;
using Xunit;

namespace ModelMaintainer.Tests.Utils
{
    public class TagHelperTests
    {
        [Fact]
        public void ToArdoqTags_MapIsNull_ReturnsEmptyList()
        {
            // Arrange
            Dictionary<string, string> map = null;

            // Act
            var tags = map.ToArdoqTags();

            // Assert
            Assert.Empty(tags);
        }

        [Fact]
        public void ToArdoqTags_HappyDays_ReturnsExpectedTags()
        {
            // Arrange
            Dictionary<string, string> map = new Dictionary<string, string>
            {
                ["environment"] = "Test",
                ["configkey"] = "abc-def-xyz"
            };

            // Act
            var tags = map.ToArdoqTags().ToList();

            // Assert
            Assert.Equal(2, tags.Count());
            Assert.Contains("environment-test", tags);
            Assert.Contains("configkey-abc-def-xyz", tags);
        }
    }
}
