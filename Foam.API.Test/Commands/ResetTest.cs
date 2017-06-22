using System;
using Foam.API.Commands;
using Foam.API.Files;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Foam.API.Test.Commands
{
    [TestClass]
    public class ResetTest
    {
        [TestMethod]
        public void Test()
        {
            using (var runner = JobRunner.CreateDebugRunner())
            {
                for (int i = 0; i < 10; i++)
                    runner.FileBuffer.Add(new FileItem($"test{i}.txt", DateTimeOffset.Now, new byte[0]));

                var cmd = new ResetCommand();
                cmd.Execute(runner);

                Assert.AreEqual(0, runner.FileBuffer.Count);
            }
        }
    }
}
