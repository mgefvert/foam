using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DotNetCommons;
using Foam.API.Commands;
using Foam.API.Exceptions;
using Foam.API.Memory;

namespace Foam.API.Configuration
{
    public class JobConfiguration
    {
        public ExtensionLibrary Library { get; }
        public List<JobDefinition> Jobs { get; } = new List<JobDefinition>();
        public List<Map> Maps { get; } = new List<Map>();

        public string MemoryType { get; set; }
        public string MemoryConnectionString { get; set; }
        public string MemoryFilename { get; set; }

        public JobConfiguration(ExtensionLibrary library)
        {
            Library = library;
        }

        public IMemory CreateMemoryStorage()
        {
            switch (MemoryType)
            {
                case "file":
                    return new FileMemory(MemoryFilename);

                case "mysql":
                    return new MySqlMemory(MemoryConnectionString);

                default:
                    throw new FoamException($"Unrecognized memory backend type: '{MemoryType}', valid options are 'file' or 'mysql'.");
            }
        }

        public JobDefinition FindJob(string name)
        {
            return Jobs.FirstOrDefault(x => x.Name.Like(name));
        }

        public void SetProperty(ICommand result, string name, string value)
        {
            string Multiply(string numvalue, double multiplier)
            {
                var number = double.Parse(numvalue.Substring(0, value.Length - 1), CultureInfo.InvariantCulture);
                return (number * multiplier).ToString(CultureInfo.InvariantCulture);
            }

            var pname = name.Replace("-", "");
            var property = result.GetType().GetProperties().FirstOrDefault(p => p.Name.Like(pname));
            if (property == null)
                throw new FoamConfigurationException($"Undefined command attribute '{name}'");

            if (property.PropertyType == typeof(Uri))
            {
                result.SetPropertyValue(property, new Uri(value));
                return;
            }

            if (property.PropertyType.IsNumeric())
            {
                if (value.EndsWith("G"))
                    value = Multiply(value, 1024 * 1024 * 1024);
                else if (value.EndsWith("M"))
                    value = Multiply(value, 1024 * 1024);
                else if (value.EndsWith("k"))
                    value = Multiply(value, 1024);
            }

            result.SetPropertyValue(property, value, CultureInfo.InvariantCulture);
        }
    }
}
