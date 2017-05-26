using System;
using System.Collections.Generic;
using System.Linq;
using DotNetCommons;
using Foam.API.Configuration;
using Foam.API.Exceptions;
using Foam.API.Files;
using Foam.API.Memory;
using Foam.API.Providers;
using Foam.API.Transactions;

namespace Foam.API
{
    public class JobRunner : IDisposable
    {
        private readonly JobDefinition _definition;
        private readonly ExtensionLibrary _library;
        private readonly List<IProvider> _providers = new List<IProvider>();
        private readonly Stack<FileList> _fileBufferStack = new Stack<FileList>();

        public FileList FileBuffer => _fileBufferStack.Peek();
        public CommitBuffer CommitBuffer { get; } = new CommitBuffer();
        public IMemory Memory { get; }

        public string JobName { get; }

        public JobRunner(JobDefinition definition, ExtensionLibrary library, IMemory memory)
        {
            _definition = definition;
            _library = library;
            Memory = memory;
            JobName = definition.Name;
        }

        ~JobRunner()
        {
            Dispose();
        }

        public void Dispose()
        {
            ShutdownProviders();
        }

        public void Execute()
        {
            // Make sure we're ready to go
            foreach (var cmd in _definition.Commands)
                cmd.Initialize();

            StartProviders();
            try
            {
                // Seed the initial file buffer stack
                _fileBufferStack.Push(new FileList());

                foreach (var cmd in _definition.Commands)
                {
                    var cmdName = ExtensionLibrary.TypeToName(cmd.GetType());

                    Logger.Enter("Executing command: " + cmdName);
                    try
                    {
                        cmd.Execute(this);
                    }
                    finally
                    {
                        Logger.Leave();
                    }
                }

                CommitBuffer.Commit();
            }
            catch (Exception ex)
            {
                Logger.Err($"{ex.GetType().Name} occurred while running job {_definition.Name}: {ex.Message}");
                CommitBuffer.Rollback();
                Logger.Err("Job aborted.");
            }
            finally
            {
                ShutdownProviders();
            }
        }

        public IProvider SelectProvider(Uri source)
        {
            var result = _providers.FirstOrDefault(p => p.CanHandleUri(source));
            if (result != null)
                return result;

            result = _providers.FirstOrDefault(p => p.CanHandleProtocol(source.Scheme));

            return result ?? throw new FoamException("Unable to find a provider for location " + source);
        }

        private void ShutdownProviders()
        {
            foreach (var provider in _providers.ExtractAll(x => true))
            {
                Logger.Debug("Shutting down provider " + provider.GetType().Name);
                provider.Dispose();
            }
        }

        private void StartProviders()
        {
            foreach (var type in _library.LoadedAssemblies.SelectMany(x => x.Providers))
            {
                Logger.Debug("Instantiating provider " + type.Name);
                _providers.Add((IProvider) Activator.CreateInstance(type));
            }
        }

        public void PushFileBuffer()
        {
            var newlist = new FileList(FileBuffer);
            _fileBufferStack.Push(newlist);
        }

        public void PopFileBuffer()
        {
            if (_fileBufferStack.Count <= 1)
                throw new FoamException("File buffer stack is empty.");

            _fileBufferStack.Pop();
        }
    }
}
