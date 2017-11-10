using System;
using System.Linq;
using Foam.API.Commands;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Foam.API.Test.Commands
{
    [TestClass]
    public class GenerateRandomTest
    {
        [TestMethod]
        public void Test()
        {
            using (var runner = JobRunner.CreateDebugRunner())
            {
                new GenerateRandomCommand { Len=1024, Name="bork" }.Execute(runner);
                Assert.AreEqual(1, runner.FileBuffer.Count);
                var file = runner.FileBuffer.Single();
                Assert.AreEqual("bork", file.Name);
                Assert.AreEqual(1024, file.Length);
            }
        }
    }
}
