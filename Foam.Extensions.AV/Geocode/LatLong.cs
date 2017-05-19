using System;
using System.Globalization;

namespace Foam.Extensions.AV.Geocode
{
    public class LatLong
    {
        private readonly double _latitude;
        private readonly double _longitude;
        private readonly bool _hasValue;

        public bool HasValue => _hasValue;
        public double Latitude => _latitude;
        public double Longitude => _longitude;

        public string PositionString => _hasValue
          ? _latitude.ToString(CultureInfo.InvariantCulture) + "," + _longitude.ToString(CultureInfo.InvariantCulture)
          : null;

        public LatLong()
        {
            _hasValue = false;
        }

        public LatLong(string latitudeReference, double[] latitude, string longitudeReference, double[] longitude)
          : this(DmsToDecimal(latitudeReference, latitude), DmsToDecimal(longitudeReference, longitude))
        {
        }

        public LatLong(double? latitude, double? longitude)
        {
            _hasValue = latitude.HasValue && longitude.HasValue;
            if (!_hasValue)
                return;

            _latitude = latitude.GetValueOrDefault();
            _longitude = longitude.GetValueOrDefault();
        }

        public static double? DmsToDecimal(string reference, double[] position)
        {
            if (string.IsNullOrEmpty(reference) || position == null || position.Length < 3)
                return null;

            var sign = (reference == "S" || reference == "W") ? -1 : 1;

            return sign * (position[0] + position[1] / 60 + position[2] / 3600);
        }
    }
}
