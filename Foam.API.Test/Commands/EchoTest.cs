using System;
using DotNetCommons;
using Foam.API.Commands;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Foam.API.Test.Commands
{
    [TestClass]
    public class EchoTest
    {
        private bool _gotHelloWorld;

        [TestMethod]
        public void Test()
        {
            using (var runner = JobRunner.CreateDebugRunner())
            {
                var cmd = new EchoCommand
                {
                    Text = "Hello, world!"
                };

                Logger.LogEvent += LoggerOnLogEvent;
                try
                {
                    _gotHelloWorld = false;
                    cmd.Execute(runner);
                    Assert.IsTrue(_gotHelloWorld);
                }
                finally
                {
                    Logger.LogEvent -= LoggerOnLogEvent;
                }
            }
        }

        private void LoggerOnLogEvent(LogSeverity severity, string text)
        {
            _gotHelloWorld |= text.ToLower().Contains("hello, world");
        }
    }
}
