using System;
using Foam.API.Commands;
using Foam.API.Files;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Foam.API.Test.Commands
{
    [TestClass]
    public class PushPopTest
    {
        [TestMethod]
        public void Test()
        {
            using (var runner = JobRunner.CreateDebugRunner())
            {
                runner.FileBuffer.Add(new FileItem("file.txt", DateTime.Now, "this is a test"));

                Assert.AreEqual(1, runner.FileBuffer.Count);

                new PushCommand().Execute(runner);
                Assert.AreEqual(1, runner.FileBuffer.Count);

                new ResetCommand().Execute(runner);
                Assert.AreEqual(0, runner.FileBuffer.Count);

                new PopCommand().Execute(runner);
                Assert.AreEqual(1, runner.FileBuffer.Count);
            }
        }
    }
}
