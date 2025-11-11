using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Object pooling system roughly based on Unity's Introduction to Object Pooling.
/// https://learn.unity.com/tutorial/introduction-to-object-pooling
/// </summary>
[DefaultExecutionOrder(-1000)]



public class ObjectPool : MonoBehaviour
{
    public static ObjectPool SharedInstance;
    public List<PoolableObject> objectsToPool;
    public Dictionary<GameObject, List<GameObject>> poolDictionary;

    private void Awake()
    {
        if (SharedInstance == null)
        {
            SharedInstance = this;
        }
        else if (SharedInstance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Initialize pool dictionary with lists of objects to pool.
        poolDictionary = new Dictionary<GameObject, List<GameObject>>();
        foreach (PoolableObject item in objectsToPool)
        {
            List<GameObject> objects = new List<GameObject>();

            // Pool a specified amount of objects.
            for (int i = 0; i < item.amountToPool; i++)
            {
                GameObject obj = Instantiate(item.objectToPool);
                obj.SetActive(false);
                objects.Add(obj);
            }
            // Key is the object, value is a list of pooled instances.
            poolDictionary.Add(item.objectToPool, objects);
        }
    }

    /// <summary>
    /// Retrive a pooled object, used when rendering map.
    /// </summary>
    /// <param name="prefab">The instance to compare with pool dictionary.</param>
    /// <returns></returns>
    public GameObject GetPooledObject(GameObject prefab)
    {
        if (!poolDictionary.ContainsKey(prefab))
        {
            Debug.LogWarning("Object not found in pool: " + prefab.name);
            return null;
        }
        List<GameObject> objects = poolDictionary[prefab];

        foreach (GameObject obj in objects)
        {
            // Sanity check, since the player can break bricks.
            if (obj != null && !obj.activeInHierarchy)
            {
                return obj;
            }
        }
        return null;
    }
}

/// <summary>
/// Poolable object defines an object to be pooled and an amount to pool.
/// </summary>
[System.Serializable]
public class PoolableObject
{
    public GameObject objectToPool;
    public int amountToPool;
}
