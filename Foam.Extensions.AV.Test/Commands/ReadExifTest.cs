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
    public class ReadExifTest
    {
        [TestMethod]
        public void Execute()
        {
            using (var runner = JobRunner.CreateDebugRunner())
            {
                runner.FileBuffer.Add(new FileItem("test.jpg", DateTime.Now,
                    Assembly.GetExecutingAssembly().GetManifestResourceStream("Foam.Extensions.AV.Test.TestData.test.jpg")));

                var cmd = new ReadExifCommand();
                cmd.Initialize();
                cmd.Execute(runner);

                Assert.AreEqual(1, runner.FileBuffer.Count);

                var vars = runner.FileBuffer.Single().Variables;
                Assert.IsTrue(vars.Count > 0);
                Assert.IsTrue(vars.All(x => x.Key.StartsWith("exif-")));

                foreach(var var in vars)
                    Console.WriteLine(var.Key + "=" + var.Value);
            }
        }
    }
}
