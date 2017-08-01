using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DotNetCommons;
using Foam.API;
using Foam.API.Attributes;
using Foam.API.Configuration;

namespace Foam
{
    internal class Program
    {
        private static int Main()
        {
            Logger.Configuration.EchoToConsole = true;
            Logger.Configuration.FileNaming = LogFileNaming.Monthly;
            Logger.Notice("File Organizer And Mover " + Assembly.GetExecutingAssembly().GetName().Version);

            var result = 0;

            try
            {
                CommandLine.DisplayHelpOnEmpty = false;
                var opts = CommandLine.Parse<Options>();
                if (opts.Debug)
                    Logger.Configuration.Severity = LogSeverity.Debug;

                var library = new ExtensionLibrary();
                library.ScanAndLoad();

                Logger.Log("Loading configuration file " + opts.ConfigFile);
                var jobconfig = new JobConfiguration(library);
                new JobXmlParser(jobconfig).Parse(opts.ConfigFile);

                if (opts.ListCommands)
                {
                    if (opts.Arguments.Any())
                    {
                        foreach(var command in opts.Arguments)
                            ListCommands(library, command);
                    }
                    else
                        ListCommands(library);
                }
                else if (opts.Arguments.Any())
                    RunJobs(jobconfig, library, opts.Arguments);
                else
                    ListJobs(jobconfig);
            }
            catch (CommandLineDisplayHelpException ex)
            {
                Logger.Warn("No parameters on command line; displaying help.");
                Console.Error.WriteLine();
                Console.Error.WriteLine(ex.Message);
                result = 1;
            }
            catch (Exception ex)
            {
                using (new SetConsoleColor(ConsoleColor.Red))
                {
                    Logger.Err("An unhandled exception occurred:");
                    Logger.Err(ex);
                }
                result = 1;
            }

            Logger.Log($"End run, exit code={result}.");
            return result;
        }

        private static void ListCommands(ExtensionLibrary library)
        {
            Console.WriteLine();
            Console.WriteLine("List of commands:");
            Console.WriteLine();

            foreach (var assembly in library.LoadedAssemblies)
            {
                Console.WriteLine(assembly.Assembly.GetName().Name);

                if (!assembly.Commands.Any())
                {
                    Console.WriteLine("  No commands defined.");
                    continue;
                }

                var maxlen = assembly.Commands.Max(x => x.Key.Length);
                foreach (var command in assembly.Commands.OrderBy(x => x.Key))
                {
                    using (new SetConsoleColor(ConsoleColor.White))
                        Console.Write("  " + command.Key.PadRight(maxlen) + "  ");

                    var help = command.Value.GetCustomAttribute<ShortDescriptionAttribute>();
                    Console.WriteLine(help?.Text);
                }

                Console.WriteLine();
            }
        }

        private static void ListCommands(ExtensionLibrary library, string command)
        {
            var cmd = library.FindCommand(command);
            if (cmd.Key == null)
            {
                Console.WriteLine(command + ": No such command.");
                return;
            }

            var type = cmd.Value;

            Console.WriteLine();
            Console.WriteLine("Command  : " + cmd.Key);
            Console.WriteLine("Assembly : " + type.Assembly.GetName().Name);
            Console.WriteLine();

            var help = type.GetCustomAttribute<LongDescriptionAttribute>()?.Text ??
                       type.GetCustomAttribute<ShortDescriptionAttribute>()?.Text;
            if (!string.IsNullOrEmpty(help))
                Console.WriteLine(help);

            Console.WriteLine();
            Console.WriteLine("Parameters:");

            var props = type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Select(x => new Tuple<string, string>(ExtensionLibrary.TypeToName(x.Name), x.GetCustomAttribute<PropertyDescriptionAttribute>()?.Text))
                .ToList();

            var maxlen = props.Max(x => x.Item1.Length);
            foreach (var prop in props)
            {
                using (new SetConsoleColor(ConsoleColor.White))
                    Console.Write("  " + prop.Item1.PadRight(maxlen) + "  ");

                Console.WriteLine(prop.Item2);
            }

            Console.WriteLine();
        }

        private static void ListJobs(JobConfiguration jobconfig)
        {
            if (!jobconfig.Jobs.Any())
            {
                Console.WriteLine("No jobs defined.");
                return;
            }

            Console.WriteLine();
            Console.WriteLine("List of available jobs:");

            var maxlen = jobconfig.Jobs.Max(x => x.Name.Length);

            foreach(var job in jobconfig.Jobs.OrderBy(x => x.Name))
                Console.WriteLine(job.Name.PadRight(maxlen) + "  " + job.Description);
        }

        private static void RunJobs(JobConfiguration jobconfig, ExtensionLibrary library, List<string> jobs)
        {
            var memory = jobconfig.CreateMemoryStorage();
            try
            {
                foreach (var jobname in jobs)
                {
                    var jobdefinition = jobconfig.FindJob(jobname);
                    if (jobdefinition == null)
                    {
                        Logger.Err($"Unable to find job '{jobname}' in job list");
                        continue;
                    }

                    Logger.Enter("Starting job " + jobdefinition.Name, LogSeverity.Notice);
                    try
                    {
                        var settings = new JobRunnerSettings
                        {
                            Definition = jobdefinition,
                            Library = library,
                            Memory = memory,
                            Maps = jobconfig.Maps
                        };

                        using (var runner = new JobRunner(settings))
                        {
                            runner.CallJob += (sender, args) => args.Result = jobconfig.FindJob(args.JobName);
                            runner.Execute();
                        }
                    }
                    finally
                    {
                        Logger.Leave("Job finished");
                    }
                }
            }
            finally
            {
                memory.Dispose();
            }
        }
    }
}
