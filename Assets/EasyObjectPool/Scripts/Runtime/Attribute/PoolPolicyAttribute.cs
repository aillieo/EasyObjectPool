using System;

namespace AillieoUtils
{
    [AttributeUsage(validOn: AttributeTargets.Class, AllowMultiple = false)]
    public class PoolPolicyAttribute : Attribute
    {
        public int capacity { get; set; } = 0;

        public int sizeMax { get; set; } = 8;
    }
}
