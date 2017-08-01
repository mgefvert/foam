using System;
using Foam.API.Commands;
using Foam.API.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Foam.API.Test.Commands
{
    [TestClass]
    public class SetVarTest
    {
        [TestMethod]
        [ExpectedException(typeof(FoamConfigurationException))]
        public void TestCantSetReservedVariable()
        {
            using (var runner = JobRunner.CreateDebugRunner())
            {
                var cmd = new SetVarCommand
                {
                    Value = "test",
                    To = "date"
                };

                // "date" is a reserved variable and we should blow up here.
                cmd.Initialize();
                cmd.Execute(runner);
            }
        }

        [TestMethod]
        public void TestSetVariable()
        {
            Assert.Inconclusive("Not implemented");
        }
    }
}
