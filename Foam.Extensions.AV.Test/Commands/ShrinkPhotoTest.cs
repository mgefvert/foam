using System;
using System.Linq;
using System.Reflection;
using ExifLib;
using Foam.API;
using Foam.API.Files;
using Foam.Extensions.AV.Commands;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Foam.Extensions.AV.Test.Commands
{
    [TestClass]
    public class ShrinkPhotoTest
    {
        [TestMethod]
        public void Execute()
        {
            using (var runner = JobRunner.CreateDebugRunner())
            {
                var dt = new DateTime(2016, 1, 1);
                runner.FileBuffer.Add(new FileItem("test.jpg", dt,
                    Assembly.GetExecutingAssembly().GetManifestResourceStream("Foam.Extensions.AV.Test.TestData.test.jpg")));

                AssertHasExifData(runner.FileBuffer[0].Data);

                var cmd = new ShrinkPhotoCommand { PixelLimit = 1048576 };
                cmd.Initialize();
                cmd.Execute(runner);

                Assert.AreEqual(1, runner.FileBuffer.Count);

                var file = runner.FileBuffer.Single();
                Assert.AreEqual(dt, file.Timestamp);
                Assert.IsTrue(file.Length < 131072);
                AssertHasExifData(file.Data);
            }
        }

        private void AssertHasExifData(ReadOnlyByteBuffer data)
        {
            using (var reader = new ExifReader(data.GetReadOnlyStream()))
            {
                reader.GetTagValue<string>(ExifTags.Model, out var model);
                Assert.IsFalse(string.IsNullOrEmpty(model));
            }
        }
    }
}
