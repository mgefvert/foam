using System;
using System.Linq;
using System.Reflection;
using Foam.API;
using Foam.API.Files;
using Foam.Extensions.AV.Commands;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Foam.Extensions.AV.Test.Commands
{
    [TestClass]
    public class FixMp4CreationDateTest
    {
        [TestMethod]
        public void Test()
        {
            using (var runner = JobRunner.CreateDebugRunner())
            {
                var dt = new DateTime(2016, 1, 1);
                runner.FileBuffer.Add(new FileItem("test.mp4", dt,
                    Assembly.GetExecutingAssembly().GetManifestResourceStream("Foam.Extensions.AV.Test.TestData.test.mp4")));
                var crc = runner.FileBuffer[0].Crc32;

                var cmd = new FixMp4CreationDateCommand();
                cmd.Initialize();
                cmd.Execute(runner);

                var file = runner.FileBuffer.Single();
                Assert.AreNotEqual(crc, file.Crc32);
            }
        }
    }
}
