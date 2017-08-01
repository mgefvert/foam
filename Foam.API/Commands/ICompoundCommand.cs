using System;
using System.Collections.Generic;
using Foam.API.Files;

namespace Foam.API.Commands
{
    public interface ICompoundCommand : ICommand
    {
        FileList Filter(FileList files, JobRunner runner);
        ICollection<ICommand> Commands { get; }
    }
}
