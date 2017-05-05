using System;

namespace funkyBot.Objects
{
    [Serializable]
    public class Trekk
    {
        public string Id { get; set; }

        public string Type { get; set; }

        public string DisplayName { get; set; }

        public DateTime StartTimeStamp { get; set; }

        public DateTime StopTimeStamp { get; set; }
    }
}