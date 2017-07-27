using System;
using System.Linq;
using DotNetCommons;
using Foam.API.Attributes;
using Foam.API.Exceptions;
using Foam.API.Files;

namespace Foam.API.Commands
{
    [ShortDescription("Map a variable to a different value.")]
    [LongDescription("The map-var command takes a variable and maps it to a different value using the map definitions in the job file. " +
                     "It is very useful in translating for instance keywords into folders names or similar data. You can use either " +
                     "variables (var=\"variable\") or variable-expanded text (text=\"{@variable}\") to match the map keys.")]
    public class MapVarCommand : ICommand
    {
        [PropertyDescription("Optional file mask specifies what files to use.")]
        public string Mask { get; set; }
        [PropertyDescription("Variable to match.")]
        public string Var { get; set; }
        [PropertyDescription("Text to match.")]
        public string Text { get; set; }
        [PropertyDescription("Which map to use.")]
        public string Map { get; set; }
        [PropertyDescription("Which variable that receives the mapping value.")]
        public string To { get; set; }

        public void Initialize()
        {
            if (string.IsNullOrEmpty(Var) && Text == null)
                throw new FoamConfigurationException("Either 'var' or 'text' must be defined.");
            if (string.IsNullOrEmpty(Map))
                throw new FoamConfigurationException("No 'map' is defined.");
            if (string.IsNullOrEmpty(To))
                throw new FoamConfigurationException("No target variable 'to' is defined.");
        }

        public void Execute(JobRunner runner)
        {
            var map = runner.Maps.GetOrDefault(Evaluator.Text(Map));
            if (map == null)
                throw new FoamConfigurationException($"Undefined map '{Map}'.");

            var files = runner.FileBuffer.SelectFiles(Evaluator.Text(Mask)).ToList();
            if (!files.Any())
                return;

            foreach (var file in files)
            {
                var lookup = !string.IsNullOrEmpty(Var) ? Evaluator.Variable(Var, file) : Evaluator.Text(Text, file);
                var result = map.GetOrDefault(lookup);

                Logger.Debug($"map-var({file.Name}): value '{lookup}' maps to '{result}");
                file.Variables[To] = result;
            }
        }
    }
}
