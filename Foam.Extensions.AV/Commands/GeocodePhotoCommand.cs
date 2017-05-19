using System;
using DotNetCommons;
using Foam.API;
using Foam.API.Attributes;
using Foam.API.Commands;
using Foam.API.Files;

namespace Foam.Extensions.AV.Commands
{
    [ShortDescription("Uses the GPS location in photos to tag them with a geolocation.")]
    [LongDescription("Examines the GPS location EXIF tags in photos and uses Google Maps to look up the " +
                     "street address for that location. Uses a cache to remember past locations, and " +
                     "requires a valid Google Maps API key.")]
    public class GeocodePhotoCommand : ICommand
    {
        [PropertyDescription("Valid Google Maps API key. You can get this online from the Google Maps developer pages.")]
        public string ApiKey { get; set; }
        [PropertyDescription("File mask to use when selecting files from the file buffer.")]
        public string Mask { get; set; }

        public void Initialize()
        {
        }

        public void Execute(JobRunner runner)
        {
            foreach(var file in runner.FileBuffer.SelectFiles(Mask))
                Logger.Catch(() => ProcessFile(file));
        }

        private void ProcessFile(FileItem file)
        {
            throw new NotImplementedException();
        }
    }
}
