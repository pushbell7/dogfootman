using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectManager : MonoBehaviour
{
    // can i link fields together for customization purpose?
    // like Prefabs matched with maximum count to control and spawning position
    public List<GameObject> PrefabsToManage;
    public GameObject CenterObjectToManage;

    private Dictionary<int, List<GameObject>> SpawnedObjectMap;
    private Dictionary<int, int> MaxCountForObjectMap;
    // Start is called before the first frame update
    void Start()
    {
        SpawnedObjectMap = new Dictionary<int, List<GameObject>>();
        MaxCountForObjectMap = new Dictionary<int, int>();

        int index = 0;
        int[] temporaryMaxCounts = { 20, 10, 5, };
        foreach (var prefab in PrefabsToManage) 
        {
            SpawnedObjectMap.Add(index, new List<GameObject>());
            MaxCountForObjectMap.Add(index, temporaryMaxCounts[index]);
            index++;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (CenterObjectToManage == null) return;

        DestroyObjects();
        SpawnObjects();
    }

    void DestroyObjects()
    {
        const float DistanceToRemove = 120f;
        Vector3 MainCharacterPosition = CenterObjectToManage.transform.position;

        foreach (var spawnedObjects in SpawnedObjectMap)
        {
            for(int i = spawnedObjects.Value.Count - 1; i >=0; --i)
            {
                var element = spawnedObjects.Value[i];
                if (Vector3.Distance(element.transform.position, MainCharacterPosition) > DistanceToRemove)
                {
                    spawnedObjects.Value.RemoveAt(i);
                    Destroy(element);
                }
            }
        }
    }

    void SpawnObjects()
    {
        foreach (var spawnedObjects in SpawnedObjectMap)
        {
            while (spawnedObjects.Value.Count < MaxCountForObjectMap[spawnedObjects.Key])
            {
                GameObject SpawnedObject = Instantiate(PrefabsToManage[spawnedObjects.Key]);
                // how do I decide y poistion on terrain?
                Vector3 direction = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
                int size = Random.Range(25, 75);
                SpawnedObject.transform.position = CenterObjectToManage.transform.position + (direction * size);
                spawnedObjects.Value.Add(SpawnedObject);
            }
        }
    }
}
