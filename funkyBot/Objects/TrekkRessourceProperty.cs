using System;

namespace funkyBot.Objects
{
    [Serializable]
    public class TrekkRessourceProperty
    {
        public int Id { get; set; }

        public int RessourceId { get; set; }

        public string DisplayName { get; set; }

        public string Type { get; set; }

        public bool IsChangeable { get; set; }
    }
}