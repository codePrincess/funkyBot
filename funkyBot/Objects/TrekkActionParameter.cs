using System;

namespace funkyBot.Objects
{
    [Serializable]
    public class TrekkActionParameter
    {
        public int Id { get; set; }

        public int ActionId { get; set; }

        public string DisplayName { get; set; }

        public string Type { get; set; }

        public int ResourceCategoryId { get; set; }
    }
}