using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using DotNetCommons;
using Newtonsoft.Json;

namespace Foam.Extensions.AV.Geocode
{
    public class GeocodingResult
    {
        public string ResultCode { get; set; }
        public string StreetNumber { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string County { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string Zip { get; set; }

        [JsonIgnore]
        public bool Success => ResultCode == "OK";
        [JsonIgnore]
        public string Reference => string.Join(", ", new List<string>
        {
            Street,
            City + " " + State,
            Country
        }.TrimAndFilter());

        public GeocodingResult()
        {
        }

        public static GeocodingResult FromJson(string source)
        {
            return JsonConvert.DeserializeObject<GeocodingResult>(source);
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        public GeocodingResult(XContainer document)
        {
            var response = document.Elements("GeocodeResponse").SingleOrDefault();
            if (response == null)
                return;

            ResultCode = response.Elements("status").Select(x => x.Value).SingleOrDefault();

            foreach (var component in response.Elements("result").Elements("address_component"))
            {
                var shortName = component.Elements("short_name").Select(x => x.Value.Trim()).FirstOrDefault();
                var longName = component.Elements("long_name").Select(x => x.Value.Trim()).FirstOrDefault();
                var type = component.Elements("type").Select(x => x.Value.Trim()).FirstOrDefault();
                if (string.IsNullOrEmpty(longName) || string.IsNullOrEmpty(type))
                    continue;

                switch (type)
                {
                    case "street_number":
                        StreetNumber = shortName;
                        break;

                    case "route":
                        Street = longName;
                        break;

                    case "locality":
                        City = longName;
                        break;

                    case "administrative_area_level_2":
                        County = longName;
                        break;

                    case "administrative_area_level_1":
                        State = shortName;
                        break;

                    case "country":
                        Country = shortName;
                        break;

                    case "postal_code":
                        Zip = longName;
                        break;
                }
            }
        }
    }
}
