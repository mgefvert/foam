using System;
using System.Collections.Generic;
using Foam.API.Files;

namespace Foam.API.Commands
{
    public interface ICompoundCommand : ICommand
    {
        /// <summary>
        /// Filter runs a filter on the file list and returns it for execution. If NULL is returned,
        /// the compound commands will not be executed.
        /// </summary>
        /// <param name="files">Current file buffer</param>
        /// <param name="runner">The current job runner</param>
        /// <returns>A FileList that is forwarded to the subcommands, or NULL if the subcommands should be skipped.</returns>
        FileList Filter(FileList files, JobRunner runner);

        /// <summary>
        /// A list of subcommands belonging to this command, for IF queries, etc.
        /// </summary>
        ICollection<ICommand> Commands { get; }
    }
}
