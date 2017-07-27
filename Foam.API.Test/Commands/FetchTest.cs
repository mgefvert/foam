using System;
using System.IO;
using System.Linq;
using Foam.API.Commands;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Foam.API.Test.Commands
{
    [TestClass]
    public class FetchTest
    {
        [TestMethod]
        public void Test()
        {
            using (var runner = JobRunner.CreateDebugRunner())
            {
                var path = Path.GetTempPath();
                var testfile = Path.Combine(path, "foam-test.fetchdata");

                File.WriteAllText(testfile, "This is a test.");

                var cmd = new FetchCommand
                {
                    Mask = "*.fetchdata",
                    Source = path
                };
                cmd.Execute(runner);

                Assert.IsTrue(runner.FileBuffer.Count > 0);
                Assert.IsTrue(runner.FileBuffer.Any(x => x.Name == "foam-test.fetchdata"));
            }
        }
    }
}
