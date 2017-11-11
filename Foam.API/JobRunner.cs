using System;
using System.Collections.Generic;
using System.Linq;
using DotNetCommons;
using DotNetCommons.Logger;
using Foam.API.Commands;
using Foam.API.Configuration;
using Foam.API.Exceptions;
using Foam.API.Files;
using Foam.API.Memory;
using Foam.API.Providers;
using Foam.API.Transactions;

namespace Foam.API
{
    public class CallJobEventArgs : EventArgs
    {
        public string JobName { get; set; }
        public JobDefinition Result { get; set; }
    }

    public delegate void CallJob(object sender, CallJobEventArgs args);

    public class JobRunner : IDisposable
    {
        private readonly Stack<FileList> _fileBufferStack = new Stack<FileList>();

        public string JobName { get; }
        public CommitBuffer CommitBuffer { get; } = new CommitBuffer();
        public Variables Constants { get; } = new Variables();
        public FileList FileBuffer => _fileBufferStack.Peek();
        public JobDefinition Definition { get; }
        public ExtensionLibrary Library { get; }
        public Dictionary<string, Map> Maps { get; }
        public IMemory Memory { get; }
        public List<IProvider> Providers { get; } = new List<IProvider>();

        public event CallJob CallJob;

        public static JobRunner CreateDebugRunner()
        {
            var definition = new JobDefinition { Name = "debug" };

            var library = new ExtensionLibrary();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(x => !x.FullName.StartsWith("System"))
                .ToList();

            foreach (var assembly in assemblies)
                library.ScanAssembly(assembly);

            return new JobRunner(new JobRunnerSettings {
                Definition = definition,
                Library = library,
                Memory = new InternalMemory()
            });
        }

        public JobRunner(JobRunnerSettings settings)
        {
            Definition = settings.Definition;
            Library = settings.Library;

            Maps = settings.Maps?.ToDictionary(x => x.Name, StringComparer.CurrentCultureIgnoreCase)
                   ?? new Dictionary<string, Map>();

            Memory = settings.Memory;
            JobName = Definition.Name;

            StartProviders();
            ReinitFileBuffer();
        }

        ~JobRunner()
        {
            Dispose();
        }

        public void Dispose()
        {
            ShutdownProviders();
        }

        public void Call(string jobname)
        {
            if (CallJob == null)
                throw new NullReferenceException("CallJob handler is not assigned.");

            var args = new CallJobEventArgs { JobName = jobname };
            CallJob(this, args);

            if (args.Result == null)
                throw new FoamException($"Job '{jobname}' not found.");

            ExecuteCommands(args.Result.Commands);
        }

        public void Execute()
        {
            // Make sure we're ready to go
            foreach (var cmd in Definition.Commands)
                cmd.Initialize();

            try
            {
                ReinitFileBuffer();
                try
                {
                    ExecuteCommands(Definition.Commands);
                }
                catch (FoamStopJobException)
                {
                    Logger.Log("Job ended through Stop command.");
                }
                CommitBuffer.Commit();
            }
            catch (Exception ex)
            {
                Logger.Error($"{ex.GetType().Name} occurred while running job {Definition.Name}: {ex.Message}");
                CommitBuffer.Rollback();
                Logger.Error("Job aborted.");
            }
            finally
            {
                ReinitFileBuffer();
            }
        }

        private void ExecuteCommands(IEnumerable<ICommand> commands)
        {
            foreach (var cmd in commands)
            {
                var cmdName = ExtensionLibrary.TypeToName(cmd.GetType());

                Logger.Enter("Executing command: " + cmdName);
                try
                {
                    cmd.Execute(this);

                    if (cmd is ICompoundCommand compound)
                    {
                        var filelist = compound.Filter(FileBuffer, this);
                        if (filelist == null)
                            continue;

                        PushFileBuffer(filelist);
                        ExecuteCommands(compound.Commands);
                        PopFileBuffer();
                    }
                }
                finally
                {
                    Logger.Leave();
                }
            }
        }

        public void PopFileBuffer()
        {
            if (_fileBufferStack.Count <= 1)
                throw new FoamException("File buffer stack is empty.");

            _fileBufferStack.Pop();
        }

        public void PushFileBuffer()
        {
            PushFileBuffer(new FileList(FileBuffer));
        }

        public void PushFileBuffer(FileList newlist)
        {
            _fileBufferStack.Push(newlist);
        }

        private void ReinitFileBuffer()
        {
            // Seed the initial file buffer stack
            _fileBufferStack.Clear();
            _fileBufferStack.Push(new FileList());
        }

        public IProvider SelectProvider(Uri source)
        {
            var result = Providers.FirstOrDefault(p => p.CanHandleUri(source));
            if (result != null)
                return result;

            result = Providers.FirstOrDefault(p => p.CanHandleProtocol(source.Scheme));

            return result ?? throw new FoamException("Unable to find a provider for location " + source);
        }

        private void ShutdownProviders()
        {
            foreach (var provider in Providers.ExtractAll(x => true))
            {
                Logger.Debug("Shutting down provider " + provider.GetType().Name);
                provider.Dispose();
            }
        }

        private void StartProviders()
        {
            foreach (var type in Library.LoadedAssemblies.SelectMany(x => x.Providers))
            {
                Logger.Debug("Instantiating provider " + type.Name);
                Providers.Add((IProvider) Activator.CreateInstance(type));
            }
        }
    }
}
