using System;
using Foam.API.Commands;
using Foam.API.Files;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Foam.API.Test.Commands
{
    [TestClass]
    public class PackUnpackTest
    {
        [TestMethod]
        public void Test()
        {
            using (var runner = JobRunner.CreateDebugRunner())
            {
                runner.FileBuffer.AddRange(new[]
                {
                    new FileItem("file.txt1", DateTime.Now, "this is a test"),
                    new FileItem("file.txt2", DateTime.Now, "this is a test"),
                    new FileItem("file.txt3", DateTime.Now, "this is a test"),
                });

                Assert.AreEqual(3, runner.FileBuffer.Count);

                ICommand cmd = new PackCommand
                {
                    Format = CompressionMode.Zip,
                    Name = "test.zip"
                };

                cmd.Execute(runner);
                Assert.AreEqual(1, runner.FileBuffer.Count);

                cmd = new UnpackCommand();
                cmd.Execute(runner);

                Assert.AreEqual(3, runner.FileBuffer.Count);
            }
        }
    }
}
