using System.Collections.Generic;
using Newtonsoft.Json;

namespace TorneoPredicciones.Classes
{
    public class DataFacebook
    {
        [JsonProperty(PropertyName = "is_silhouette")]
        public bool IsSilhouette { get; set; }

        [JsonProperty(PropertyName = "url")]
        public string Url { get; set; }
    }

    public class Picture
    {
        [JsonProperty(PropertyName = "data")]
        public DataFacebook Data { get; set; }
    }

    public class Cover
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "offset_y")]
        public int OffsetY { get; set; }

        [JsonProperty(PropertyName = "source")]
        public string Source { get; set; }
    }

    public class AgeRange
    {
        [JsonProperty(PropertyName = "min")]
        public int Min { get; set; }
    }

    public class Device
    {
        [JsonProperty(PropertyName = "hardware")]
        public string Hardware { get; set; }

        [JsonProperty(PropertyName = "os")]
        public string Os { get; set; }
    }

    public class FacebookResponse
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "picture")]
        public Picture Picture { get; set; }

        [JsonProperty(PropertyName = "cover")]
        public Cover Cover { get; set; }

        [JsonProperty(PropertyName = "age_range")]
        public AgeRange AgeRange { get; set; }

        [JsonProperty(PropertyName = "devices")]
        public List<Device> Devices { get; set; }

        [JsonProperty(PropertyName = "gender")]
        public string Gender { get; set; }

        [JsonProperty(PropertyName = "is_verified")]
        public bool IsVerified { get; set; }

        [JsonProperty(PropertyName = "locale")]
        public string Locale { get; set; }

        [JsonProperty(PropertyName = "link")]
        public string Link { get; set; }

        [JsonProperty(PropertyName = "first_name")]
        public string FirstName { get; set; }

        [JsonProperty(PropertyName = "last_name")]
        public string LastName { get; set; }

        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
    }

}
