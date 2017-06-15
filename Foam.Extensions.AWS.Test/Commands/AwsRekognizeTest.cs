using System;
using System.Reflection;
using DotNetCommons;
using DotNetCommons.Configuration;
using ExifLib;
using Foam.API;
using Foam.API.Files;
using Foam.Extensions.AWS.Commands;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Foam.Extensions.AWS.Test.Commands
{
    [TestClass]
    public class AwsRekognizeTest
    {
        [TestMethod]
        public void Execute()
        {
            using (var runner = JobRunner.CreateDebugRunner())
            {
                runner.FileBuffer.Add(new FileItem("pexels-photo-254069.jpg", DateTime.Now,
                    Assembly.GetExecutingAssembly().GetManifestResourceStream("Foam.Extensions.AWS.Test.TestData.pexels-photo-254069.jpg")));

                var config = new LocalConfigFile(Environment.MachineName);
                var cmd = new AwsRekognizeCommand
                {
                    AccessKey = config.GetOrDefault("AWS-AccessKey"),
                    SecretKey = config.GetOrDefault("AWS-SecretKey")
                };

                cmd.Initialize();
                cmd.Execute(runner);

                Assert.AreEqual(1, runner.FileBuffer.Count);

                using (var exif = new ExifReader(runner.FileBuffer[0].GetStream(false)))
                {
                    Assert.IsTrue(exif.GetTagValue(ExifTags.XPKeywords, out string result));
                    Assert.IsTrue(result.Contains("people"));
                }
            }
        }
    }
}
