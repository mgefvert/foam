using System;
using System.Globalization;

namespace Foam.Extensions.AV.Geocode
{
    public class LatLong
    {
        public bool HasValue { get; }
        public double Latitude { get; }
        public double Longitude { get; }

        public string PositionString => HasValue
          ? Latitude.ToString(CultureInfo.InvariantCulture) + "," + Longitude.ToString(CultureInfo.InvariantCulture)
          : null;

        public LatLong()
        {
            HasValue = false;
        }

        public LatLong(string latitudeReference, double[] latitude, string longitudeReference, double[] longitude)
          : this(DmsToDecimal(latitudeReference, latitude), DmsToDecimal(longitudeReference, longitude))
        {
        }

        public LatLong(double? latitude, double? longitude)
        {
            HasValue = latitude.HasValue && longitude.HasValue;
            if (!HasValue)
                return;

            Latitude = latitude.GetValueOrDefault();
            Longitude = longitude.GetValueOrDefault();
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
