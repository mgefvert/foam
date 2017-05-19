using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DotNetCommons;

namespace Foam.API.Files
{
    public class FileList : List<FileItem>
    {
        public FileList()
        {
        }

        public FileList(IEnumerable<FileItem> collection) : base(collection)
        {
        }

        public IEnumerable<FileItem> SelectFiles(string mask)
        {
            if (string.IsNullOrEmpty(mask))
                mask = "*";

            var masks = mask.Split(',').TrimAndFilter().ToArray();
            var regexes = masks.Select(WildCardToRegular).ToList();

            foreach (var file in this)
            {
                if (regexes.Any(r => r.IsMatch(file.Name)))
                    yield return file;
            }
        }

        public FileList ExtractFiles(string mask)
        {
            var result = SelectFiles(mask).ToList();
            foreach (var file in SelectFiles(mask))
                Remove(file);

            return new FileList(result);
        }

        private static Regex WildCardToRegular(string value)
        {
            return new Regex("^" + Regex.Escape(value).Replace("\\?", ".").Replace("\\*", ".*?") + "$", 
                RegexOptions.IgnoreCase | RegexOptions.Singleline);
        }
    }
}
