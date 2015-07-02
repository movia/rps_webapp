using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace RpsPositionWebApp.ViewModels
{
    public abstract class VehicleEvent
    {
        [JsonProperty("type")]
        public string Type { get { return this.GetType().Name; } }

        [JsonProperty("vehicleId")]
        public string VehicleId { get; set; }
    }

    public class VehiclePositionEvent : VehicleEvent
    {
        [JsonProperty("timestamp")]
        public ulong Timestamp { get; set; }
        
        [JsonProperty("lat")]
        public float Latitude { get; set; }

        [JsonProperty("lng")]
        public float Longitude { get; set; }

        [JsonProperty("speed")]
        public float Speed { get; set; }

        [JsonProperty("direction")]
        public float Direction { get; set; }

        [JsonProperty("accuracy")]
        public float Accuracy { get; set; }

        public override string ToString()
        {
            return string.Format("[{0}]p({1},{2))", base.VehicleId, this.Latitude, this.Longitude);
        }
    }

    public class VehicleAssignmentEvent : VehicleEvent
    {
        [JsonProperty("journeyId", NullValueHandling=NullValueHandling.Ignore)]
        public string JourneyId { get; set; }

        [JsonProperty("lineDesignation", NullValueHandling = NullValueHandling.Ignore)]
        public string LineDesignation { get; set; }

        [JsonProperty("journeyNumber", NullValueHandling = NullValueHandling.Ignore)]
        public int? JourneyNumber { get; set; }

        [JsonProperty("destination", NullValueHandling = NullValueHandling.Ignore)]
        public string Destination { get; set; }

        [JsonProperty("acceptedDateTime", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? AcceptedDateTime { get; set; }

        [JsonProperty("plannedStartDateTime", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? PlannedStartDateTime { get; set; }

        [JsonProperty("plannedEndDateTime", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? PlannedEndDateTime { get; set; }
    }
}