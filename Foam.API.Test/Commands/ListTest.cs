using System;
using Foam.API.Commands;
using Foam.API.Files;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Foam.API.Test.Commands
{
    [TestClass]
    public class ListTest
    {
        [TestMethod]
        public void Test()
        {
            using (var runner = JobRunner.CreateDebugRunner())
            {
                // Hard to test, just make sure it doesn't blow up.
                runner.FileBuffer.Add(new FileItem("test.dat", DateTimeOffset.Now, "This is a test."));
                new ListCommand().Execute(runner);
            }
        }
    }
}
