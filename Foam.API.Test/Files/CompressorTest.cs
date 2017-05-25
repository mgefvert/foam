using System;
using System.Reflection;
using System.Text;
using Foam.API.Files;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SevenZip;
using CompressionMode = Foam.API.Files.CompressionMode;

namespace Foam.API.Test.Files
{
    [TestClass]
    public class CompressorTest
    {
        private static string _testData;
        private static byte[] _testBytes;

        public CompressorTest()
        {
            _testData = "";
            for (var i = 0; i < 300; i++)
                _testData += "The quick brown fox jumped over the lazy dog. ";

            _testBytes = Encoding.Default.GetBytes(_testData);
        }

        [TestMethod]
        public void TestCompressSingleZip()
        {
            var list = new FileList
            {
                new FileItem("test.txt", DateTime.Now, _testBytes)
            };

            var compressed = Compressor.Compress(list, "x.zip", CompressionMode.Zip);
            Assert.AreEqual("x.zip", compressed.Name);
            compressed = Compressor.Compress(list, null, CompressionMode.Zip);
            Assert.AreEqual("test.zip", compressed.Name);

            var decompressed = Compressor.Decompress(compressed);
            Assert.AreEqual(1, decompressed.Count);
            Assert.AreEqual("test.txt", decompressed[0].Name);
            Assert.AreEqual(_testBytes.Length, decompressed[0].Length);
            Assert.AreEqual(_testData, decompressed[0].GetString(Encoding.Default));
        }

        [TestMethod]
        public void TestCompressSingleGZip()
        {
            var list = new FileList
            {
                new FileItem("test.txt", DateTime.Now, _testBytes)
            };

            var compressed = Compressor.Compress(list, "x.gz", CompressionMode.GZip);
            Assert.AreEqual("x.gz", compressed.Name);
            compressed = Compressor.Compress(list, null, CompressionMode.GZip);
            Assert.AreEqual("test.txt.gz", compressed.Name);

            var decompressed = Compressor.Decompress(compressed);
            Assert.AreEqual(1, decompressed.Count);
            Assert.AreEqual("test.txt", decompressed[0].Name);
            Assert.AreEqual(_testBytes.Length, decompressed[0].Length);
            Assert.AreEqual(_testData, decompressed[0].GetString(Encoding.Default));
        }

        [TestMethod]
        public void TestCompressMultipleZip()
        {
            var list = new FileList
            {
                new FileItem("test.txt", DateTime.Now, _testBytes),
                new FileItem("test2.txt", DateTime.Now, _testBytes),
                new FileItem("test3.txt", DateTime.Now, _testBytes),
                new FileItem("test4.txt", DateTime.Now, _testBytes)
            };

            var compressed = Compressor.Compress(list, "x.zip", CompressionMode.Zip);
            Assert.AreEqual("x.zip", compressed.Name);
            compressed = Compressor.Compress(list, null, CompressionMode.Zip);
            Assert.AreEqual("test.zip", compressed.Name);

            var decompressed = Compressor.Decompress(compressed);
            Assert.AreEqual(4, decompressed.Count);
            Assert.AreEqual("test.txt", decompressed[0].Name);
            Assert.AreEqual("test2.txt", decompressed[1].Name);
            Assert.AreEqual("test3.txt", decompressed[2].Name);
            Assert.AreEqual("test4.txt", decompressed[3].Name);
            Assert.AreEqual(_testBytes.Length, decompressed[0].Length);
            Assert.AreEqual(_testBytes.Length, decompressed[1].Length);
            Assert.AreEqual(_testBytes.Length, decompressed[2].Length);
            Assert.AreEqual(_testBytes.Length, decompressed[3].Length);
            Assert.AreEqual(_testData, decompressed[0].GetString(Encoding.Default));
            Assert.AreEqual(_testData, decompressed[1].GetString(Encoding.Default));
            Assert.AreEqual(_testData, decompressed[2].GetString(Encoding.Default));
            Assert.AreEqual(_testData, decompressed[3].GetString(Encoding.Default));
        }

        [TestMethod]
        [ExpectedException(typeof(CompressionFailedException))]
        public void TestCompressMultipleGZip()
        {
            var list = new FileList
            {
                new FileItem("test.txt", DateTime.Now, _testBytes),
                new FileItem("test2.txt", DateTime.Now, _testBytes),
                new FileItem("test3.txt", DateTime.Now, _testBytes),
                new FileItem("test4.txt", DateTime.Now, _testBytes)
            };

            Compressor.Compress(list, "x.gz", CompressionMode.GZip);
        }

        [TestMethod]
        public void TestDecompressZip()
        {
            var file = new FileItem { Name = "files.zip" };
            file.SetData(Assembly.GetExecutingAssembly().GetManifestResourceStream("Foam.API.Test.TestData.files.zip"));

            var decompressed = Compressor.Decompress(file);

            Assert.AreEqual(2, decompressed.Count);
            Assert.AreEqual("file1.txt", decompressed[0].Name);
            Assert.AreEqual("file2.txt", decompressed[1].Name);
            Assert.AreEqual(2820, decompressed[0].Length);
            Assert.AreEqual(1620, decompressed[1].Length);
            Assert.IsTrue(decompressed[0].GetString(Encoding.Default).StartsWith("The quick brown fox jumped over the lazy dog."));
            Assert.IsTrue(decompressed[1].GetString(Encoding.Default).StartsWith("I am a little panda bear."));
        }

        [TestMethod]
        public void TestDecompress7Zip()
        {
            var file = new FileItem { Name = "files.7z" };
            file.SetData(Assembly.GetExecutingAssembly().GetManifestResourceStream("Foam.API.Test.TestData.files.7z"));

            var decompressed = Compressor.Decompress(file);

            Assert.AreEqual(2, decompressed.Count);
            Assert.AreEqual("file1.txt", decompressed[0].Name);
            Assert.AreEqual("file2.txt", decompressed[1].Name);
            Assert.AreEqual(2820, decompressed[0].Length);
            Assert.AreEqual(1620, decompressed[1].Length);
            Assert.IsTrue(decompressed[0].GetString(Encoding.Default).StartsWith("The quick brown fox jumped over the lazy dog."));
            Assert.IsTrue(decompressed[1].GetString(Encoding.Default).StartsWith("I am a little panda bear."));
        }

        [TestMethod]
        public void TestDecompressGZip()
        {
            var file = new FileItem { Name = "file1.txt.gz" };
            file.SetData(Assembly.GetExecutingAssembly().GetManifestResourceStream("Foam.API.Test.TestData.file1.txt.gz"));

            var decompressed = Compressor.Decompress(file);

            Assert.AreEqual(1, decompressed.Count);
            Assert.AreEqual("file1.txt", decompressed[0].Name);
            Assert.AreEqual(2820, decompressed[0].Length);
            Assert.IsTrue(decompressed[0].GetString(Encoding.Default).StartsWith("The quick brown fox jumped over the lazy dog."));
        }
    }
}
