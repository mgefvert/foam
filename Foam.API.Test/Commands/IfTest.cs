﻿using System;
using DotNetCommons;
using DotNetCommons.Logger;
using Foam.API.Commands;
using Foam.API.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Foam.API.Test.Commands
{
    [TestClass]
    public class IfTest
    {
        private bool _success;
        private JobRunner _runner;

        [TestInitialize]
        public void Setup()
        {
            _success = false;
            _runner = JobRunner.CreateDebugRunner();
            Logger.LogChannel.LogEvent += LogEvent;
        }

        [TestCleanup]
        public void Teardown()
        {
            Logger.LogChannel.LogEvent -= LogEvent;
            _runner.Dispose();
        }

        [TestMethod]
        public void TestSet()
        {
            var cmd = new IfSetCommand();
            cmd.Commands.Add(new EchoCommand { Text = "success" });
            cmd.Var = "testvar";

            _runner.Definition.Commands.Add(cmd);
            _runner.Execute();
            Assert.IsFalse(_success);

            _runner.Constants["testvar"] = "1";
            _runner.Execute();
            Assert.IsTrue(_success);
        }

        private void LogEvent(object channel, LogEntry entry)
        {
            _success |= entry.Message.Contains("success");
        }
    }
}
