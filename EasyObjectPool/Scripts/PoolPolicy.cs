using System;

namespace AillieoUtils
{
    [Serializable]
    public class PoolPolicy
    {
        public int reserveOnTrim = 0;
        public int sizeMax = 8;

        public override string ToString()
        {
            return $"RES:{reserveOnTrim}/SIZE:{sizeMax}";
        }
    }
    
    
}
