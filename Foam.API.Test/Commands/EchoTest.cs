using System;
using DotNetCommons;
using DotNetCommons.Logger;
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

                Logger.LogChannel.LogEvent += LoggerOnLogEvent;
                try
                {
                    _gotHelloWorld = false;
                    cmd.Execute(runner);
                    Assert.IsTrue(_gotHelloWorld);
                }
                finally
                {
                    Logger.LogChannel.LogEvent -= LoggerOnLogEvent;
                }
            }
        }

        private void LoggerOnLogEvent(object channel, LogEntry entry)
        {
            _gotHelloWorld |= entry.Message.ToLower().Contains("hello, world");
        }
    }
}
