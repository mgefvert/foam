using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Xml.Linq;

namespace Foam.Extensions.AV.Geocode
{
    public class GeocodingApi
    {
        private static DateTime _rateLimiter = DateTime.MinValue;

        public static GeocodingResult ReverseGeocodeLookup(string apikey, LatLong latlong)
        {
            while ((DateTime.Now - _rateLimiter).TotalMilliseconds < 250)
                Thread.Sleep(10);

            _rateLimiter = DateTime.Now;

            var parameters = new Dictionary<string, string>
            {
                { "latlng", latlong.PositionString },
                { "key", apikey },
                { "result_type", "street_address" }
            };

            var uri = "https://maps.googleapis.com/maps/api/geocode/xml?" + 
                string.Join("&", parameters.Select(x => x.Key + "=" + Uri.EscapeDataString(x.Value)));

            var request = WebRequest.CreateHttp(uri);

            using (var response = request.GetResponse())
            using (var stream = response.GetResponseStream())
            {
                var result = XDocument.Load(stream);
                return new GeocodingResult(result);
            }
        }
    }
}
