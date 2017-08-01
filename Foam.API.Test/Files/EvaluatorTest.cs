using System;
using Foam.API.Files;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Foam.API.Test.Files
{
    [TestClass]
    public class EvaluatorTest
    {
        [TestMethod]
        public void TestText()
        {
            var file = new FileItem("d:\\temp\\hello-world.txt", new DateTime(2017, 1, 15, 12, 49, 13), "HelloWorld");
            file.Variables["test"] = "gorgonzola";

            Assert.AreEqual("File=hello-world.txt Size=10 " + DateTime.Today.ToString("yyyyMMdd"), 
                Evaluator.Text("File={@filename-noext}{@fileext} Size={@filesize} {@date}", file, null));
        }

        [TestMethod]
        public void TestVariableFile()
        {
            var file = new FileItem("d:\\temp\\hello-world.txt", new DateTime(2017, 1, 15, 12, 49, 13), "HelloWorld");
            file.Variables["test"] = "gorgonzola";

            Assert.AreEqual("hello-world.txt", Evaluator.Variable("filename", file, null));
            Assert.AreEqual("hello-world", Evaluator.Variable("filename-noext", file, null));
            Assert.AreEqual(".txt", Evaluator.Variable("fileext", file, null));
            Assert.AreEqual("10", Evaluator.Variable("filesize", file, null));
            Assert.AreEqual("20170115-124913", Evaluator.Variable("filedate", file, null));
            Assert.AreEqual("2004290681", Evaluator.Variable("filecrc", file, null));
            Assert.AreEqual(DateTime.Now.ToString("yyyyMMdd"), Evaluator.Variable("date", file, null));
            Assert.AreEqual(DateTime.Now.ToString("yyyyMMdd-HHmmss"), Evaluator.Variable("time", file, null));
            Assert.AreEqual(DateTime.UtcNow.ToString("yyyyMMdd"), Evaluator.Variable("utcdate", file, null));
            Assert.AreEqual(DateTime.UtcNow.ToString("yyyyMMdd-HHmmss"), Evaluator.Variable("utctime", file, null));
            Assert.AreEqual("gorgonzola", Evaluator.Variable("test", file, null));
            Assert.AreEqual(null, Evaluator.Variable("dont-exist", file, null));
            Assert.AreEqual(null, Evaluator.Variable("", file, null));
            Assert.AreEqual(null, Evaluator.Variable(null, file, null));
        }

        [TestMethod]
        public void TestVariableNoFile()
        {
            Assert.AreEqual(null, Evaluator.Variable("filename", null, null));
            Assert.AreEqual(null, Evaluator.Variable("filename-noext", null, null));
            Assert.AreEqual(null, Evaluator.Variable("fileext", null, null));
            Assert.AreEqual(null, Evaluator.Variable("filesize", null, null));
            Assert.AreEqual(null, Evaluator.Variable("filedate", null, null));
            Assert.AreEqual(null, Evaluator.Variable("filecrc", null, null));
            Assert.AreEqual(DateTime.Now.ToString("yyyyMMdd"), Evaluator.Variable("date", null, null));
            Assert.AreEqual(DateTime.Now.ToString("yyyyMMdd-HHmmss"), Evaluator.Variable("time", null, null));
            Assert.AreEqual(DateTime.UtcNow.ToString("yyyyMMdd"), Evaluator.Variable("utcdate", null, null));
            Assert.AreEqual(DateTime.UtcNow.ToString("yyyyMMdd-HHmmss"), Evaluator.Variable("utctime", null, null));
            Assert.AreEqual(null, Evaluator.Variable("test", null, null));
            Assert.AreEqual(null, Evaluator.Variable("dont-exist", null, null));
            Assert.AreEqual(null, Evaluator.Variable("", null, null));
            Assert.AreEqual(null, Evaluator.Variable(null, null, null));
        }
    }
}
