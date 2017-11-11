using System;
using System.Linq;
using System.Text.RegularExpressions;
using DotNetCommons;
using DotNetCommons.Logger;
using Foam.API.Attributes;
using Foam.API.Exceptions;
using Foam.API.Files;

namespace Foam.API.Commands
{
    [ShortDescription("Set a variable to a specific value or regex result.")]
    [LongDescription("Set-var sets a variable to a specific value determined from either 'var' or 'text' (either using a variable " +
                     "as the source directly, or using a variable-expanded text; and optionally applying a regex expression the value " +
                     "before setting the target variable.")]
    public class SetVarCommand : ICommand
    {
        [PropertyDescription("Optional file mask specifies what files to use.")]
        public string Mask { get; set; }
        [PropertyDescription("Variable to match.")]
        public string Var { get; set; }
        [PropertyDescription("Text to match.")]
        public string Text { get; set; }
        [PropertyDescription("Optional regular expression to use (Singleline and IgnoreCase are used); the first matching group is used.")]
        public string Regex { get; set; }
        [PropertyDescription("Optional value to set.")]
        public string Value { get; set; }
        [PropertyDescription("Which variable that receives the mapping value.")]
        public string To { get; set; }

        private Regex _regex;

        public void Initialize()
        {
            if (string.IsNullOrEmpty(Var) && Text == null)
                throw new FoamConfigurationException("Either 'var' or 'text' must be defined.");
            if (string.IsNullOrEmpty(To))
                throw new FoamConfigurationException("No target variable 'to' is defined.");
            if (Evaluator.IsReserved(To))
                throw new FoamConfigurationException($"The variable '{To}' is reserved and may not be assigned.");

            if (!string.IsNullOrEmpty(Regex))
                _regex = new Regex(Regex, RegexOptions.IgnoreCase | RegexOptions.Singleline);
        }

        public void Execute(JobRunner runner)
        {
            var files = runner.FileBuffer.SelectFiles(Evaluator.Text(Mask, null, runner.Constants)).ToList();
            if (!files.Any())
                return;

            foreach (var file in files)
            {
                var source = !string.IsNullOrEmpty(Var) 
                    ? Evaluator.Variable(Var, file, runner.Constants)
                    : Evaluator.Text(Text, file, runner.Constants);

                if (_regex != null)
                {
                    var result = _regex.Match(source);
                    if (result.Success)
                        source = result.Groups.Count >= 1 ? result.Groups[1].Value : result.Groups[0].Value;
                    else
                        source = "";
                }

                Logger.Debug($"set-var({file.Name}): {To} = '{source}'");
                file.Variables[To] = source;
            }
        }
    }
}
