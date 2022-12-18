using AillieoUtils;
using UnityEngine;

[PoolPolicy(sizeMax = 4, capacity = 2)]
public class CubeDefault : IPoolable
{
    private static int sid = 0;
    public CubeDefault()
    {
        id = sid++;
    }

    public readonly int id;
    public void OnGet()
    {
        Debug.Log($"CubeDefault OnGet {id}");
    }

    public void OnRecycle()
    {
        Debug.Log($"CubeDefault OnRecycle {id}");
    }
}
