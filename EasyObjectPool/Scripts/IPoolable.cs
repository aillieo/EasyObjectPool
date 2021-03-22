using System;
using System.Collections.Generic;

namespace AillieoUtils
{
    public interface IPoolable
    {
        void OnGet();

        void OnRecycle();
    }
}
