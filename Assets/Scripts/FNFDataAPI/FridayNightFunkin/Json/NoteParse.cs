using System.Collections.Generic;
using Newtonsoft.Json;

namespace FridayNightFunkin.Json
{
    public class NoteParse {
        [JsonProperty("mustHitSection")]
        public bool MustHitSection { get; set; }

        [JsonProperty("typeOfSection")]
        public long TypeOfSection { get; set; }

        [JsonProperty("lengthInSteps")]
        public long LengthInSteps { get; set; }

        [JsonProperty("sectionNotes")]
        public List<List<decimal>> sectionNotes { get; set; } 

        [JsonProperty("bpm", NullValueHandling = NullValueHandling.Ignore)]
        public long? Bpm { get; set; }

        [JsonProperty("changeBPM", NullValueHandling = NullValueHandling.Ignore)]
        public bool? ChangeBpm { get; set; }
    }
}