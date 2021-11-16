﻿using Xunit;

namespace NPreprocessor.Tests
{
    public class TextReaderTests
    {
        [Fact]
        public void Scenario0()
        {
            var lineReader = new TextReader("`define data Hello.\r\n`include \"data.txt\"");
            Assert.Null(lineReader.CurrentLine);
            lineReader.MoveNext();
            Assert.Equal("`define data Hello.", lineReader.CurrentLine);
            lineReader.MoveNext();
            Assert.Equal("`include \"data.txt\"", lineReader.CurrentLine);
            lineReader.MoveNext();
            Assert.Null(lineReader.CurrentLine);
        }

        [Fact]
        public void Scenario1()
        {
            var lineReader = new TextReader(@"1
2
3
4");
            Assert.Null(lineReader.CurrentLine);
            lineReader.MoveNext();
            Assert.Equal("1", lineReader.CurrentLine);
            lineReader.MoveNext();
            Assert.Equal("2", lineReader.CurrentLine);
            lineReader.MoveNext();
            Assert.Equal("3", lineReader.CurrentLine);
            lineReader.MoveNext();
            Assert.Equal("4", lineReader.CurrentLine);
            lineReader.MoveNext();
            Assert.Null(lineReader.CurrentLine);
        }

        [Fact]
        public void Scenario2()
        {
            var lineReader = new TextReader("1\\r\\n2\r\n3\\r\\n4");
            Assert.Null(lineReader.CurrentLine);
            lineReader.MoveNext();
            Assert.Equal("1\\r\\n2", lineReader.CurrentLine);
            lineReader.MoveNext();
            Assert.Equal("3\\r\\n4", lineReader.CurrentLine);
            lineReader.MoveNext();
            Assert.Null(lineReader.CurrentLine);
        }

        [Fact]
        public void Scenario3()
        {
            var lineReader = new TextReader(" 1 \\r\\n 2 \r\n 3 \\r\\n 4 ");
            Assert.Null(lineReader.CurrentLine);
            lineReader.MoveNext();
            Assert.Equal(" 1 \\r\\n 2 ", lineReader.CurrentLine);
            lineReader.MoveNext();
            Assert.Equal(" 3 \\r\\n 4 ", lineReader.CurrentLine);
            lineReader.MoveNext();
            Assert.Null(lineReader.CurrentLine);
        }

        [Fact]
        public void Scenario4()
        {
            var lineReader = new TextReader("\r\n\r\n");
            Assert.Null(lineReader.CurrentLine);
            lineReader.MoveNext();
            Assert.Equal("", lineReader.CurrentLine);
            lineReader.MoveNext();
            Assert.Equal("", lineReader.CurrentLine);
            lineReader.MoveNext();
            Assert.Null(lineReader.CurrentLine);
        }

        [Fact]
        public void Scenario5()
        {
            var lineReader = new TextReader("");
            Assert.Null(lineReader.CurrentLine);
            lineReader.MoveNext();
            Assert.Equal(string.Empty, lineReader.CurrentLine);
            lineReader.MoveNext();
            Assert.Null(lineReader.CurrentLine);
        }
    }
}
