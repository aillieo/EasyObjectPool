using UnityEngine;

public class CubeObject : MonoBehaviour
{
    private void OnEnable()
    {
        Debug.Log($"CubeObject OnEnable {GetInstanceID()}");
    }

    private void OnDisable()
    {
        Debug.Log($"CubeObject OnDisable {GetInstanceID()}");
    }
}
