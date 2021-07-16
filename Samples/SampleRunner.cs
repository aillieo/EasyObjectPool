using AillieoUtils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SampleRunner : MonoBehaviour
{
    [SerializeField]
    private GameObject cube;
    [SerializeField]
    private CubeObject cubeObject;
    [SerializeField]
    private Transform hideRoot;

    private Pool<CubeDefault> pool1;
    private readonly Queue<CubeDefault> objects1 = new Queue<CubeDefault>();

    private Pool<CubeObject> pool2;
    private readonly Queue<CubeObject> objects2 = new Queue<CubeObject>();
    private Vector3 start2 = new Vector3(-10, 3, 0);
    private int position2 = 0;

    private LocalGameObjectPool pool3;
    private readonly Queue<GameObject> objects3 = new Queue<GameObject>();
    private Vector3 start3 = new Vector3(-10, 1, 0);
    private int position3 = 0;

    private LocalGameObjectPool pool4;
    private readonly Queue<GameObject> objects4 = new Queue<GameObject>();
    private Vector3 start4 = new Vector3(-10, -1, 0);
    private int position4 = 0;

    private readonly Queue<GameObject> objects5 = new Queue<GameObject>();
    private Vector3 start5 = new Vector3(-10, -3, 0);
    private int position5 = 0;

    void Start()
    {
        pool1 = Pool<CubeDefault>.CreateDefault<CubeDefault>();

        pool2 = Pool<CubeObject>.Create()
            .SetCreateFunc(() => GameObject.Instantiate(cubeObject))
            .SetDestroyFunc(o => GameObject.Destroy(o))
            .SetOnGet(o => o.gameObject.SetActive(true))
            .SetOnRecycle(o => o.gameObject.SetActive(false))
            .AsPool();

        pool3 = new LocalGameObjectPool(cube, hideRoot);

        pool4 = new LocalGameObjectPool(cube, o=> o.transform.localScale = Vector3.one, o => o.transform.localScale = Vector3.zero);
    }

    private void OnGUI()
    {
        GUILayoutOption height = GUILayout.Height(50);
        GUILayoutOption width = GUILayout.Width(200);
        if (GUILayout.Button("1 +", width, height))
        {
            CubeDefault o = pool1.Get();
            objects1.Enqueue(o);
        }

        if (GUILayout.Button("1 -", width, height))
        {
            if(objects1.Count > 0)
            {
                CubeDefault o = objects1.Dequeue();
                pool1.Recycle(o);
            }
        }

        if (GUILayout.Button("2 +", width, height))
        {
            CubeObject o = pool2.Get();
            objects2.Enqueue(o);
            Vector3 pos = start2 + Vector3.right * position2++;
            o.transform.position = pos;
        }

        if (GUILayout.Button("2 -", width, height))
        {
            if (objects2.Count > 0)
            {
                CubeObject o = objects2.Dequeue();
                pool2.Recycle(o);
            }
        }

        if (GUILayout.Button("3 +", width, height))
        {
            GameObject o = pool3.Get();
            objects3.Enqueue(o);
            Vector3 pos = start3 + Vector3.right * position3++;
            o.transform.position = pos;
        }

        if (GUILayout.Button("3 -", width, height))
        {
            if (objects3.Count > 0)
            {
                GameObject o = objects3.Dequeue();
                pool3.Recycle(o);
            }
        }

        if (GUILayout.Button("4 +", width, height))
        {
            GameObject o = pool4.Get();
            objects4.Enqueue(o);
            Vector3 pos = start4 + Vector3.right * position4++;
            o.transform.position = pos;
        }

        if (GUILayout.Button("4 -", width, height))
        {
            if (objects4.Count > 0)
            {
                GameObject o = objects4.Dequeue();
                pool4.Recycle(o);
            }
        }

        if (GUILayout.Button("5 +", width, height))
        {
            GameObject o = GameObjectPool.Get(cube);
            objects5.Enqueue(o);
            Vector3 pos = start5 + Vector3.right * position5++;
            o.transform.position = pos;
        }

        if (GUILayout.Button("5 -", width, height))
        {
            if (objects5.Count > 0)
            {
                GameObject o = objects5.Dequeue();
                GameObjectPool.Recycle(o);
            }
        }
    }
}
