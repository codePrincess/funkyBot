using System;

namespace funkyBot.Objects
{
    [Serializable]
    public class StartObject
    {
        public string DisplayText { get; set; }

        public string LicenseNumber { get; set; }

        public int Km { get; set; }
    }
}