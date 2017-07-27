using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using DotNetCommons;
using Foam.API.Commands;
using Foam.API.Exceptions;

namespace Foam.API.Configuration
{
    public class JobXmlParser
    {
        public class DefaultItem
        {
            public string Command { get; set; }
            public string Property { get; set; }
            public string Value { get; set; }
        }

        public class Defaults
        {
            public List<DefaultItem> Items { get; } = new List<DefaultItem>();

            public Defaults()
            {
            }

            public Defaults(Defaults defaults)
            {
                Items.AddRange(defaults.Items);
            }

            public void Add(string command, string property, string value)
            {
                Items.Add(new DefaultItem { Command = command, Property = property, Value = value });
            }

            public string Find(string command, string property)
            {
                return Items.FirstOrDefault(x => command.Like(x.Command) && property.Like(x.Property))?.Value;
            }

            public IEnumerable<KeyValuePair<string, string>> Find(string command)
            {
                return Items
                    .Where(x => command.Like(x.Command))
                    .Select(x => new KeyValuePair<string, string>(x.Property, x.Value));
            }
        }

        private const string Ns = "http://gefvert.org/xsd/foam";
        private static readonly XName ConfigName = XName.Get("config", Ns);
        private static readonly XName DefaultsName = XName.Get("defaults", Ns);
        private static readonly XName GroupName = XName.Get("group", Ns);
        private static readonly XName JobName = XName.Get("job", Ns);
        private static readonly XName MapName = XName.Get("map", Ns);

        private readonly JobConfiguration _config;

        public JobXmlParser(JobConfiguration configuration)
        {
            _config = configuration;
        }

        public void Parse(Stream stream)
        {
            var xdoc = XDocument.Load(stream, LoadOptions.SetLineInfo);
            var xroot = xdoc.Root;
            if (xroot == null)
                throw ReadError(null, "Root node missing");

            Parse(xroot, new Defaults());
        }

        public void Parse(string filename)
        {
            using (var fs = new FileStream(filename, FileMode.Open))
                Parse(fs);
        }

        private void Parse(XElement xroot, Defaults defaults)
        {
            foreach (var xnode in xroot.Elements(DefaultsName))
                ParseDefaultsNode(xnode, defaults);

            foreach (var xnode in xroot.Elements())
            {
                if (xnode.Name == MapName)
                    _config.Maps.AddIfNotNull(ParseMapNode(xnode));
                else if (xnode.Name == JobName)
                    _config.Jobs.AddIfNotNull(ParseJobNode(xnode, defaults));
                else if (xnode.Name == ConfigName)
                    ParseConfigNode(xnode);
                else if (xnode.Name == GroupName)
                    Parse(xnode, new Defaults(defaults));
                else if (xnode.Name != DefaultsName)
                {
                    var lineInfo = (IXmlLineInfo)xnode;
                    Logger.Warn($"Unrecognized element {xnode.Name.LocalName} at {lineInfo.LineNumber}:{lineInfo.LinePosition}");
                }
            }
        }

        private void ParseDefaultsNode(XElement xdefaults, Defaults defaults)
        {
            foreach(var xnode in xdefaults.Elements())
                foreach(var xattr in xnode.Attributes())
                    defaults.Add(xnode.Name.LocalName, xattr.Name.LocalName, xattr.Value);
        }

        private void ParseConfigNode(XElement xconfig)
        {
            if (xconfig == null)
                throw ReadError(null, "config node is missing.");

            var memory = xconfig.Element(XName.Get("memory", Ns));
            if (memory != null)
            {
                _config.MemoryType = memory.Attribute("type")?.Value;
                _config.MemoryConnectionString = memory.Attribute("connectionString")?.Value;
                _config.MemoryFilename = memory.Attribute("filename")?.Value;
            }
            else
                throw ReadError(xconfig, "memory node is missing.");
        }

        private JobDefinition ParseJobNode(XElement xjob, Defaults defaults)
        {
            var result = new JobDefinition
            {
                Name = xjob.Attribute("name")?.Value,
                Description = xjob.Attribute("description")?.Value
            };

            if (string.IsNullOrEmpty(result.Name))
                throw ReadError(xjob, "Job name is missing");

            foreach(var xcommand in xjob.Elements())
                result.Commands.AddIfNotNull(ParseJobCommandNode(xcommand, defaults));

            return result;
        }

        private ICommand ParseJobCommandNode(XElement xcommand, Defaults defaults)
        {
            var name = xcommand.Name.LocalName;
            var cmd = _config.Library.FindCommand(name);
            if (cmd.Key == null)
                throw ReadError(xcommand, $"Unable to find command '{name}'");

            var result = Activator.CreateInstance(cmd.Value) as ICommand;
            if (result == null)
                throw ReadError(xcommand, "Unable to instantiate command of type " + cmd.Value.Name);

            foreach(var attr in defaults.Find(name))
                _config.SetProperty(result, attr.Key, attr.Value);

            foreach (var xattr in xcommand.Attributes())
                _config.SetProperty(result, xattr.Name.LocalName, xattr.Value);

            if (xcommand.HasElements && result is ICompoundCommand compound)
            {
                foreach (var xsub in xcommand.Elements())
                    compound.Commands.AddIfNotNull(ParseJobCommandNode(xsub, defaults));
            }

            return result;
        }

        private Map ParseMapNode(XElement xmap)
        {
            var result = new Map
            {
                Name = xmap.Attribute("name")?.Value
            };

            if (string.IsNullOrEmpty(result.Name))
                throw ReadError(xmap, "Map name is missing");

            foreach (var xadd in xmap.Elements(XName.Get("add", Ns)))
            {
                var key = xadd.Attribute("key")?.Value ?? "";
                var value = xadd.Attribute("value")?.Value ?? "";

                result.Add(key, value);
            }

            return result;
        }

        private static Exception ReadError(XElement xnode, string message)
        {
            message += " in element " + (xnode?.Name ?? "<null>");

            var lineInfo = xnode as IXmlLineInfo;
            if (lineInfo?.HasLineInfo() ?? false)
                message += $" at {lineInfo.LineNumber}:{lineInfo.LinePosition}";

            return new FoamConfigurationException(message);
        }
    }
}
