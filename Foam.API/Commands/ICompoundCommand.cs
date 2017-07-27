using System;
using System.Collections.Generic;
using Foam.API.Files;

namespace Foam.API.Commands
{
    public interface ICompoundCommand : ICommand
    {
        FileList Filter(FileList files);
        ICollection<ICommand> Commands { get; }
    }
}
