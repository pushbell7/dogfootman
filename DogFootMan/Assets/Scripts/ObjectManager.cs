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
        const float DistanceToRemove = 120;
        Vector3 MainCharacterPosition = CenterObjectToManage.transform.position;

        foreach(var spawnedObjects in SpawnedObjectMap)
        {
            spawnedObjects.Value.RemoveAll(element => Vector3.Distance(element.transform.position, MainCharacterPosition) > DistanceToRemove);
        }
    }

    void SpawnObjects()
    {
        foreach (var spawnedObjects in SpawnedObjectMap)
        {
            if (spawnedObjects.Value.Count < MaxCountForObjectMap[spawnedObjects.Key])
            {
                GameObject SpawnedObject = Instantiate(PrefabsToManage[spawnedObjects.Key]);
                // how do I decide y poistion on terrain?
                // is it possible to spawn object far from main character slightly? I want to make object not to appear suddenly near main character.
                SpawnedObject.transform.position = CenterObjectToManage.transform.position + new Vector3(Random.Range(-50, 50), 0, Random.Range(-50, 50));
                spawnedObjects.Value.Add(SpawnedObject);
            }
        }
    }
}
