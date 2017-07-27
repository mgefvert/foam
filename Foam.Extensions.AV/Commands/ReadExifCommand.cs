using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using DotNetCommons;
using ExifLib;
using Foam.API;
using Foam.API.Attributes;
using Foam.API.Commands;
using Foam.API.Files;

namespace Foam.Extensions.AV.Commands
{
    [ShortDescription("Read EXIF tags into the file variables.")]
    [LongDescription("Reads all of the EXIF tags for an image and loads it into the variables list for a file. All tags are" +
                     "prefixed with the tag 'exif-' to identify them properly.")]
    public class ReadExifCommand : ICommand
    {
        [PropertyDescription("File mask to use when selecting files from the file buffer.")]
        public string Mask { get; set; }

        public void Initialize()
        {
        }

        public void Execute(JobRunner runner)
        {
            foreach(var file in runner.FileBuffer.SelectFiles(Evaluator.Text(Mask)))
                Logger.Catch(() => ReadExif(file));
        }

        private void ReadExif(FileItem file)
        {
            using (var reader = new ExifReader(file.GetStream(false)))
            {
                foreach (ExifTags value in Enum.GetValues(typeof(ExifTags)))
                {
                    var s = GetValue(reader, value);
                    if (s == null)
                        continue;
                    
                    if (s.GetType().IsArray)
                        file.Variables["exif-" + value] = string.Join(",", ((IEnumerable)s).Cast<object>());
                    else
                        file.Variables["exif-" + value] = Convert.ToString(s, CultureInfo.InvariantCulture);
                }
            }
        }

        private static object GetValue(ExifReader reader, ExifTags tag)
        {
            try
            {
                if (reader.GetTagValue(tag, out object value))
                    return value;

                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
