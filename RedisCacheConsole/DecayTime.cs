using System;

namespace RedisCacheConsole
{
    public class DecayTime : Attribute
    {
        public int Hours { get; set; }
        public int Minutes { get; set; }
        public int Seconds { get; set; }

        public TimeSpan Time()
        {
            return new TimeSpan(Hours, Minutes, Seconds);
        }
    }
}