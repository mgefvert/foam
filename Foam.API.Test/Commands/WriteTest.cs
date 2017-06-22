using System;
using System.IO;
using System.Text;
using Foam.API.Commands;
using Foam.API.Files;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Foam.API.Test.Commands
{
    [TestClass]
    public class WriteTest 
    {
        [TestMethod]
        public void Test()
        {
            using (var runner = JobRunner.CreateDebugRunner())
            {
                var path = Path.GetTempPath();
                var filetxt = Path.Combine(path, "foam-test.txt");
                var filebak = Path.Combine(path, "foam-test.bak");

                if (File.Exists(filetxt))
                    File.Delete(filetxt);
                if (File.Exists(filebak))
                    File.Delete(filebak);

                runner.FileBuffer.Add(new FileItem("foam-test.txt", DateTimeOffset.Now, "This is a test."));
                runner.FileBuffer.Add(new FileItem("foam-test.bak", DateTimeOffset.Now, "This is a test."));

                var cmd = new WriteCommand
                {
                    Target = new Uri(Path.GetTempPath()),
                    Mask = "*.txt"
                };

                cmd.Execute(runner);

                Assert.IsTrue(File.Exists(filetxt));
                Assert.IsFalse(File.Exists(filebak));
                Assert.AreEqual(15, new FileInfo(filetxt).Length);
            }
        }
    }
}
