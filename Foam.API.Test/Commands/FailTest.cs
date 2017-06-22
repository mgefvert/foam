using System;
using Foam.API.Commands;
using Foam.API.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Foam.API.Test.Commands
{
    [TestClass]
    public class FailTest
    {
        [TestMethod, ExpectedException(typeof(FoamException))]
        public void Test()
        {
            using (var runner = JobRunner.CreateDebugRunner())
            {
                var cmd = new FailCommand();
                cmd.Execute(runner);
            }
        }
    }
}
