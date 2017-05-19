using System;

namespace Foam.API.Commands
{
    public interface ICommand
    {
        void Initialize();
        void Execute(JobRunner runner);
    }
}
