using System;

namespace funkyBot.Objects
{
    [Serializable]
    public class TrekkAction
    {
        public int Id { get; set; }

        public int TrekkTemplateId { get; set; }

        public string DisplayName { get; set; }

        public bool StartsTrekk { get; set; }

        public bool StopsTrekk { get; set; }
    }
}