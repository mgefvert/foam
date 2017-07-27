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
                Evaluator.Text("File={@filename-noext}{@fileext} Size={@filesize} {@date}", file));
        }

        [TestMethod]
        public void TestVariableFile()
        {
            var file = new FileItem("d:\\temp\\hello-world.txt", new DateTime(2017, 1, 15, 12, 49, 13), "HelloWorld");
            file.Variables["test"] = "gorgonzola";

            Assert.AreEqual("hello-world.txt", Evaluator.Variable("filename", file));
            Assert.AreEqual("hello-world", Evaluator.Variable("filename-noext", file));
            Assert.AreEqual(".txt", Evaluator.Variable("fileext", file));
            Assert.AreEqual("10", Evaluator.Variable("filesize", file));
            Assert.AreEqual("20170115-124913", Evaluator.Variable("filedate", file));
            Assert.AreEqual("2004290681", Evaluator.Variable("filecrc", file));
            Assert.AreEqual(DateTime.Now.ToString("yyyyMMdd"), Evaluator.Variable("date", file));
            Assert.AreEqual(DateTime.Now.ToString("yyyyMMdd-HHmmss"), Evaluator.Variable("time", file));
            Assert.AreEqual(DateTime.UtcNow.ToString("yyyyMMdd"), Evaluator.Variable("utcdate", file));
            Assert.AreEqual(DateTime.UtcNow.ToString("yyyyMMdd-HHmmss"), Evaluator.Variable("utctime", file));
            Assert.AreEqual("gorgonzola", Evaluator.Variable("test", file));
            Assert.AreEqual(null, Evaluator.Variable("dont-exist", file));
            Assert.AreEqual(null, Evaluator.Variable("", file));
            Assert.AreEqual(null, Evaluator.Variable(null, file));
        }

        [TestMethod]
        public void TestVariableNoFile()
        {
            Assert.AreEqual(null, Evaluator.Variable("filename"));
            Assert.AreEqual(null, Evaluator.Variable("filename-noext"));
            Assert.AreEqual(null, Evaluator.Variable("fileext"));
            Assert.AreEqual(null, Evaluator.Variable("filesize"));
            Assert.AreEqual(null, Evaluator.Variable("filedate"));
            Assert.AreEqual(null, Evaluator.Variable("filecrc"));
            Assert.AreEqual(DateTime.Now.ToString("yyyyMMdd"), Evaluator.Variable("date"));
            Assert.AreEqual(DateTime.Now.ToString("yyyyMMdd-HHmmss"), Evaluator.Variable("time"));
            Assert.AreEqual(DateTime.UtcNow.ToString("yyyyMMdd"), Evaluator.Variable("utcdate"));
            Assert.AreEqual(DateTime.UtcNow.ToString("yyyyMMdd-HHmmss"), Evaluator.Variable("utctime"));
            Assert.AreEqual(null, Evaluator.Variable("test"));
            Assert.AreEqual(null, Evaluator.Variable("dont-exist"));
            Assert.AreEqual(null, Evaluator.Variable(""));
            Assert.AreEqual(null, Evaluator.Variable(null));
        }
    }
}
