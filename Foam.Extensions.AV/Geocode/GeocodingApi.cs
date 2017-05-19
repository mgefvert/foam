using System;

namespace Foam.Extensions.AV.Geocode
{
    public class GeocodingApi
    {
        /*private readonly string _key = ConfigurationManager.AppSettings["Google-Geocoding-API"];
        private DateTime _rateLimiter = DateTime.MinValue;
        private static readonly MicroDataStore Cache = new MicroDataStore("geolookup.dat");

        public void SaveCache()
        {
            Cache.Save();
        }

        private XDocument Request(IDictionary<string, string> parameters)
        {
            while ((DateTime.Now - _rateLimiter).TotalMilliseconds < 250)
                Thread.Sleep(10);

            _rateLimiter = DateTime.Now;

            var uri = new UriBuilder("https://maps.googleapis.com/maps/api/geocode/xml")
            {
                Query = string.Join("&", parameters.Select(x => x.Key + "=" + Uri.EscapeDataString(x.Value)))
            };

            var request = WebRequest.CreateHttp(uri.Uri);
            using (var response = request.GetResponse())
            using (var stream = response.GetResponseStream())
                return XDocument.Load(stream);
        }

        public GeocodingResult ReverseGeocodeLookup(LatLong latlong)
        {
            var position = latlong.PositionString;

            if (Cache.Exists(position))
                return (GeocodingResult)Cache.AsObject[position];

            var result = new GeocodingResult(Request(new Dictionary<string, string>
            {
                { "latlng", position },
                { "key", _key },
                { "result_type", "street_address" }
            }));

            Cache.AsObject[position] = result;
            return result;
        }*/
    }
}
