using System;

namespace AillieoUtils
{
    [AttributeUsage(validOn: AttributeTargets.Class, AllowMultiple = false)]
    public class PoolStrategyAttribute : Attribute
    {
        public int reserveOnTrim { get; set; } = 0;

        public int sizeMax { get; set; } = 8;
    }
}
