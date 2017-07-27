using System;
using System.Linq;
using System.Reflection;
using Foam.API.Commands;
using Foam.API.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Foam.API.Test.Configuration
{
    [TestClass]
    public class JobXmlParserTest
    {
        [TestMethod]
        public void TestGroupDefaults()
        {
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Foam.API.Test.TestData.test.xml"))
            {
                Assert.IsNotNull(stream);

                var config = new JobConfiguration(new ExtensionLibrary());
                new JobXmlParser(config).Parse(stream);

                var job = config.FindJob("test-random-implicit");
                Assert.IsNotNull(job);
                Assert.AreEqual(10, ((GenerateRandomCommand)job.Commands.First()).Len);

                job = config.FindJob("test-random-explicit");
                Assert.IsNotNull(job);
                Assert.AreEqual(20, ((GenerateRandomCommand)job.Commands.First()).Len);

                job = config.FindJob("test-random-sub");
                Assert.IsNotNull(job);
                Assert.AreEqual(1024, ((GenerateRandomCommand)job.Commands.First()).Len);
            }
        }
    }
}
