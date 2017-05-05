using System;

namespace funkyBot.Objects
{
    [Serializable]
    public class TrekkRessources
    {
        public int Id { get; set; }

        public int RessourceCategoryId { get; set; }

        public string DisplayName { get; set; }
    }
}