using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using DotNetCommons;
using Foam.API.Commands;
using Foam.API.Exceptions;

namespace Foam.API.Configuration
{
    public class JobConfiguration
    {
        private const string Ns = "http://gefvert.org/xsd/foam";
        private readonly ExtensionLibrary _library;
        public List<JobDefinition> Jobs { get; } = new List<JobDefinition>();

        public JobConfiguration(ExtensionLibrary library)
        {
            _library = library;
        }

        public JobDefinition FindJob(string name)
        {
            return Jobs.FirstOrDefault(x => x.Name.Like(name));
        }

        public void LoadFromFile(string filename)
        {
            var xdoc = XDocument.Load(filename);
            var xroot = xdoc.Root;
            if (xroot == null)
                throw ReadError(null, "Root node missing");

            foreach (var xjob in xroot.Elements(XName.Get("jobs", Ns)).Elements(XName.Get("job", Ns)))
                Jobs.AddIfNotNull(ParseJobNode(xjob));
        }

        private Exception ReadError(XElement node, string message)
        {
            message += " in element " + (node?.Name ?? "<null>");

            var lineInfo = node as IXmlLineInfo;
            if (lineInfo?.HasLineInfo() ?? false)
                message += " on line " + lineInfo.LineNumber;

            return new FoamConfigurationException(message);
        }

        private JobDefinition ParseJobNode(XElement xjob)
        {
            var result = new JobDefinition
            {
                Name = xjob.Attribute("name")?.Value,
                Description = xjob.Attribute("description")?.Value
            };

            if (string.IsNullOrEmpty(result.Name))
                throw ReadError(xjob, "Job name is missing");

            foreach(var xcommand in xjob.Elements())
                result.Commands.AddIfNotNull(ParseCommandNode(xcommand));

            return result;
        }

        private ICommand ParseCommandNode(XElement xcommand)
        {
            var name = xcommand.Name.LocalName;
            var cmd = _library.FindCommand(name);
            if (cmd.Key == null)
                throw ReadError(xcommand, $"Unable to find command '{name}'");

            var result = Activator.CreateInstance(cmd.Value) as ICommand;
            if (result == null)
                throw ReadError(xcommand, "Unable to instantiate command of type " + cmd.Value.Name);

            foreach (var xattr in xcommand.Attributes())
                SetProperty(xcommand, result, xattr.Name.LocalName, xattr.Value);

            return result;
        }

        private void SetProperty(XElement node, ICommand result, string name, string value)
        {
            var pname = name.Replace("-", "");
            var property = result.GetType().GetProperties().FirstOrDefault(p => p.Name.Like(pname));
            if (property == null)
                throw ReadError(node, $"Undefined attribute '{name}'");

            if (property.PropertyType == typeof(Uri))
                result.SetPropertyValue(property, new Uri(value));
            else
                result.SetPropertyValue(property, value, CultureInfo.InvariantCulture);
        }
    }
}
