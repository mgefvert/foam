using System;
using System.IO;
using ExifLib;
using Foam.API.Files;
using Foam.Extensions.AV.Geocode;

namespace Foam.Extensions.AV.Classes
{
    public class ExifImage
    {
        private string _gpsLatitudeRef;
        private string _gpsLongitudeRef;
        private double[] _gpsLatitude;
        private double[] _gpsLongitude;

        public LatLong LatLong { get; private set; }
        public string CameraModel { get; private set; }

        public ExifImage(FileItem file)
        {
            using (var stream = file.GetStream(false))
                LoadValues(stream);
        }

        private void LoadValues(Stream stream)
        {
            try
            {
                using (var reader = new ExifReader(stream))
                {
                    CameraModel = GetValue<string>(reader, ExifTags.Model);

                    _gpsLatitudeRef = GetValue<string>(reader, ExifTags.GPSLatitudeRef);
                    _gpsLatitude = GetValue<double[]>(reader, ExifTags.GPSLatitude);
                    _gpsLongitudeRef = GetValue<string>(reader, ExifTags.GPSLongitudeRef);
                    _gpsLongitude = GetValue<double[]>(reader, ExifTags.GPSLongitude);
                    LatLong = new LatLong(_gpsLatitudeRef, _gpsLatitude, _gpsLongitudeRef, _gpsLongitude);
                }
            }
            catch (Exception)
            {
                LatLong = new LatLong();
            }
        }

        private static T GetValue<T>(ExifReader reader, ExifTags tag)
        {
            try
            {
                reader.GetTagValue(tag, out T value);
                return value;
            }
            catch (Exception)
            {
                return default(T);
            }
        }
    }
}
