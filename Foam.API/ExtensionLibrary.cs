using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using DotNetCommons;
using Foam.API.Commands;
using Foam.API.Providers;

namespace Foam.API
{
    public class ExtensionLibrary
    {
        public class ExtensionAssembly
        {
            public Assembly Assembly { get; set; }
            public List<KeyValuePair<string, Type>> Commands { get; } = new List<KeyValuePair<string, Type>>();
            public List<Type> Providers { get; } = new List<Type>();
        }

        public List<ExtensionAssembly> LoadedAssemblies { get; } = new List<ExtensionAssembly>();

        public ExtensionLibrary()
        {
            ScanAssembly(Assembly.GetExecutingAssembly());
        }

        public KeyValuePair<string, Type> FindCommand(string command)
        {
            foreach (var cmd in LoadedAssemblies.SelectMany(x => x.Commands))
                if (cmd.Key.Like(command))
                    return cmd;

            return new KeyValuePair<string, Type>();
        }

        public void ScanAndLoad()
        {
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (path == null)
                throw new InvalidProgramException("No directory found for main assembly.");

            var dlls = Directory.EnumerateFiles(path, "foam.extensions.*.dll");
            foreach (var dll in dlls)
            {
                Logger.Catch("Loading extension assembly " + dll, () =>
                {
                    var assembly = Assembly.LoadFile(dll);
                    ScanAssembly(assembly);
                }, "Finished loading extension assembly", LogSeverity.Debug);
            }
        }

        protected void ScanAssembly(Assembly assembly)
        {
            var library = new ExtensionAssembly { Assembly = assembly };
            var types = assembly.GetTypes();

            // Find all ICommand types
            foreach (var type in types)
            {
                var interfaces = type.GetInterfaces();

                if (interfaces.Contains(typeof(ICommand)))
                {
                    var name = TypeToName(type);
                    Logger.Debug($"Found command '{name}' ({type.Name})");
                    library.Commands.Add(new KeyValuePair<string, Type>(TypeToName(type), type));
                }

                if (interfaces.Contains(typeof(IProvider)))
                {
                    Logger.Debug($"Found provider {type.Name}");
                    library.Providers.Add(type);
                }
            }

            LoadedAssemblies.Add(library);
        }

        public static string TypeToName(Type type)
        {
            return TypeToName(type.Name);
        }

        public static string TypeToName(string type)
        {
            if (type.EndsWith("Command", true, CultureInfo.InvariantCulture))
                type = type.Substring(0, type.Length - 7);

            var result = new StringBuilder(type.Length + 3);  // Probably.

            for (int i = 0; i < type.Length; i++)
            {
                if (i > 0 && char.IsUpper(type[i]))
                    result.Append('-');

                result.Append(char.ToLower(type[i]));
            }

            return result.ToString();
        }
    }
}
