using System;
using System.IO;
using ExifLib;
using Foam.Extensions.AV.Geocode;

namespace Foam.Extensions.AV
{
    public class ExifImage
    {
        private string _gpsLatitudeRef;
        private string _gpsLongitudeRef;
        private double[] _gpsLatitude;
        private double[] _gpsLongitude;
        private string _cameraModel;
        private LatLong _latLong;

        public LatLong LatLong => _latLong;
        public string CameraModel => _cameraModel;

        public ExifImage(FileInfo file)
        {
            LoadValues(file.FullName);
        }

        private void LoadValues(string filename)
        {
            try
            {
                using (var reader = new ExifReader(filename))
                {
                    _gpsLatitudeRef = GetValue<string>(reader, ExifTags.GPSLatitudeRef);
                    _gpsLatitude = GetValue<double[]>(reader, ExifTags.GPSLatitude);
                    _gpsLongitudeRef = GetValue<string>(reader, ExifTags.GPSLongitudeRef);
                    _gpsLongitude = GetValue<double[]>(reader, ExifTags.GPSLongitude);
                    _cameraModel = GetValue<string>(reader, ExifTags.Model);

                    _latLong = new LatLong(_gpsLatitudeRef, _gpsLatitude, _gpsLongitudeRef, _gpsLongitude);
                }
            }
            catch (Exception)
            {
                _latLong = new LatLong();
            }
        }

        private T GetValue<T>(ExifReader reader, ExifTags tag)
        {
            try
            {
                T value;
                reader.GetTagValue(tag, out value);

                return value;
            }
            catch (Exception)
            {
                return default(T);
            }
        }
    }
}
