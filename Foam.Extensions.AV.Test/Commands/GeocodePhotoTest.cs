using System;
using System.Linq;
using System.Reflection;
using DotNetCommons;
using DotNetCommons.Configuration;
using Foam.API;
using Foam.API.Files;
using Foam.Extensions.AV.Commands;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Foam.Extensions.AV.Test.Commands
{
    [TestClass]
    public class GeocodePhotoTest
    {
        [TestMethod]
        public void Execute()
        {
            using (var runner = JobRunner.CreateDebugRunner())
            {
                runner.Memory.Set("geocode", "43.2721315555556,-71.5447146666667", 
                    "{\"ResultCode\":\"OK\",\"City\":\"Concord\",\"County\":\"Merrimack County\",\"State\":\"NH\",\"Country\":\"US\",\"Zip\":\"03301\"}");

                var dt = new DateTime(2016, 1, 1);
                runner.FileBuffer.Add(new FileItem("test.jpg", dt,
                    Assembly.GetExecutingAssembly().GetManifestResourceStream("Foam.Extensions.AV.Test.TestData.test.jpg")));

                var config = new LocalConfigFile(Environment.MachineName);
                var cmd = new GeocodePhotoCommand
                {
                    ApiKey = config.GetOrDefault("Geocode-ApiKey")
                };

                cmd.Initialize();
                cmd.Execute(runner);

                Assert.AreEqual(1, runner.FileBuffer.Count);

                var file = runner.FileBuffer.Single();
                Assert.AreEqual(dt, file.Timestamp);
                Assert.AreEqual("test (Concord NH, US).jpg", file.Name);
            }
        }
    }
}
