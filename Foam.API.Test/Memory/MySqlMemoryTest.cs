using System;
using System.Configuration;
using Foam.API.Memory;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Foam.API.Test.Memory
{
    [TestClass]
    public class MySqlMemoryTest
    {
        private const string Facility = "test-facility";

        [TestMethod]
        public void Test()
        {
            var mem = new MySqlMemory(ConfigurationManager.ConnectionStrings["memory"].ConnectionString);

            mem.Set(Facility, "test", "abc123");

            Assert.IsFalse(mem.Exists(Facility, "no"));
            Assert.IsTrue(mem.Exists(Facility, "test"));

            Assert.AreEqual(null, mem.Get(Facility, "no"));
            Assert.AreEqual("abc123", mem.Get(Facility, "test"));

            mem.Delete(Facility, "test");
            Assert.IsFalse(mem.Exists(Facility, "test"));
            Assert.AreEqual(null, mem.Get(Facility, "test"));
        }
    }
}
