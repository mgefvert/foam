using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Foam.API.Attributes;
using Foam.API.Exceptions;
using Foam.API.Files;

namespace Foam.API.Commands
{
    public enum FilterOperation
    {
        Eq,
        Neq,
        Any,
        Like
    }

    public abstract class SelectBaseCommand : ICompoundCommand
    {
        [PropertyDescription("Optional file mask specifies what files to test.")]
        public string Mask { get; set; }

        private string _var;
        private FilterOperation _op;
        private string _value;

        public ICollection<ICommand> Commands { get; } = new List<ICommand>();

        protected SelectBaseCommand(string var, FilterOperation op, string value)
        {
            _var = var;
            _op = op;
            _value = value;
        }

        public void Initialize()
        {
            if (string.IsNullOrEmpty(_var))
                throw new FoamConfigurationException("No variable defined.");
        }

        public void Execute(JobRunner runner)
        {
        }

        private static bool SelectAny(string value) => !string.IsNullOrEmpty(value);
        private static bool SelectEq(string value, string matchAgainst) => string.Equals(value, matchAgainst, StringComparison.CurrentCultureIgnoreCase);
        private static bool SelectLike(string value, Regex regex) => regex.IsMatch(value);
        private static bool SelectNeq(string value, string matchAgainst) => !SelectEq(value, matchAgainst);

        public FileList Filter(FileList files, JobRunner runner)
        {
            var filelist = files.SelectFiles(Evaluator.Text(Mask, null, runner.Constants));

            switch (_op)
            {
                case FilterOperation.Any:
                    filelist = filelist.Where(f => SelectAny(Evaluator.Variable(_var, f, runner.Constants)));
                    break;
                case FilterOperation.Eq:
                    filelist = filelist.Where(f => SelectEq(Evaluator.Variable(_var, f, runner.Constants), _value));
                    break;
                case FilterOperation.Neq:
                    filelist = filelist.Where(f => SelectNeq(Evaluator.Variable(_var, f, runner.Constants), _value));
                    break;
                case FilterOperation.Like:
                    var regex = new Regex(_value, RegexOptions.IgnoreCase | RegexOptions.Singleline);
                    filelist = filelist.Where(f => SelectLike(Evaluator.Variable(_var, f, runner.Constants), regex));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(_op));
            }

            return new FileList(filelist);
        }
    }
}
