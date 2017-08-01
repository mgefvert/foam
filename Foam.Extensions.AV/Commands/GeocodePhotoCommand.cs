using System;
using System.IO;
using System.Linq;
using DotNetCommons;
using Foam.API;
using Foam.API.Attributes;
using Foam.API.Commands;
using Foam.API.Exceptions;
using Foam.API.Files;
using Foam.API.Memory;
using Foam.Extensions.AV.Classes;
using Foam.Extensions.AV.Geocode;

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
            if (string.IsNullOrEmpty(ApiKey))
                throw new FoamConfigurationException("geocode-photo must have an API key.");
        }

        public void Execute(JobRunner runner)
        {
            foreach(var file in runner.FileBuffer.SelectFiles(Mask))
                Logger.Catch(() => ProcessFile(file, runner.Memory));
        }

        private void ProcessFile(FileItem file, IMemory memory)
        {
            var exif = new ExifImage(file);
            if (!exif.LatLong.HasValue)
                return;

            var geoinfo = GeoLookup(exif.LatLong, memory);
            if (!geoinfo.Success)
                return;

            var name = Path.GetFileNameWithoutExtension(file.Name)?.Split('(').First().Trim();
            if (string.IsNullOrEmpty(name))
                return;

            var ext = Path.GetExtension(file.Name);
            name += " (" + geoinfo.Reference + ")";

            file.Name = name + ext;
        }

        private GeocodingResult GeoLookup(LatLong latlong, IMemory memory)
        {
            if (memory.Exists("geocode", latlong.PositionString))
                return GeocodingResult.FromJson(memory.Get("geocode", latlong.PositionString));

            var result = GeocodingApi.ReverseGeocodeLookup(ApiKey, latlong);
            if (result.ResultCode != "REQUEST_DENIED")
                memory.Set("geocode", latlong.PositionString, result.ToJson());

            return result;
        }
    }
}
