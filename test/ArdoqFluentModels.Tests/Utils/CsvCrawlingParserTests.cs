using ArdoqFluentModels.Utils;
using System.IO;
using System.Linq;
using Xunit;

namespace ModelMaintainer.Tests.Utils
{
    public class CsvCrawlingParserTests
    {
        [Fact]
        public void Parse_SingleSimpleLine_ParsesAsExpected()
        {
            // Arrange
            var line = "a;b;c";
            var parser = new CsvCrawlingParser(';');

            // Act
            var lines = parser.Parse(line);

            // Assert
            Assert.Single(lines);
            var l = lines.First();
            Assert.Equal(3, l.Length);
            Assert.Equal("a", l[0]);
            Assert.Equal("b", l[1]);
            Assert.Equal("c", l[2]);
        }

        [Fact]
        public void Parse_TwoSimpleLines_ParsesAsExpected()
        {
            // Arrange
            var line = "a;b;c\r\nxxx;yyy;zzz";
            var parser = new CsvCrawlingParser(';');

            // Act
            var lines = parser.Parse(line);

            // Assert
            Assert.Equal(2, lines.Count());
            var l = lines.First();
            Assert.Equal(3, l.Length);
            Assert.Equal("a", l[0]);
            Assert.Equal("b", l[1]);
            Assert.Equal("c", l[2]);

            l = lines.Single(ln => ln[0] == "xxx");
            Assert.Equal("yyy", l[1]);
            Assert.Equal("zzz", l[2]);

        }


        [Fact]
        public void Parse_EmbeddedStringWithCarriageReturns_ParsesAsExpected()
        {
            // Arrange
            var line = "a;b;c;\"This is a string \r\nwith carriage\r\n\r\n\r\nreturns!\";e";
            var parser = new CsvCrawlingParser(';');

            // Act
            var lines = parser.Parse(line);

            // Assert
            Assert.Single(lines);
            var l = lines.First();
            Assert.Equal(5, l.Length);
        }

        [Fact]
        public void ReadFile()
        {
            //var f = @"c:\temp\servers_june_2018.csv";
            //var text = File.ReadAllText(f);

            //var parser = new CsvCrawlingParser(';');

            //var lines = parser.Parse(text);
        }
    }
}
