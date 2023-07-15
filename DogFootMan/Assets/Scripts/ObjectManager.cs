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

    private List<GameObject> BatchedRoads;

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

        BatchedRoads = new List<GameObject>(GameObject.FindGameObjectsWithTag("Road"));
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
                GameObject spawnedObject = Instantiate(PrefabsToManage[spawnedObjects.Key]);
                spawnedObject.transform.position = GeneratePosition(spawnedObject);
                spawnedObjects.Value.Add(spawnedObject);
            }
        }
    }

    Vector3 GeneratePosition(GameObject spawnedObject)
    {
        Vector3 direction = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
        int size = Random.Range(25, 75);
        Vector3 positionToSpawn = MoveOnTheRoad(CenterObjectToManage.transform.position + (direction * size));
        positionToSpawn.y = Terrain.activeTerrain.SampleHeight(positionToSpawn) + (spawnedObject.GetComponent<Collider>().bounds.size.y / 2);

        return positionToSpawn;
    }

    Vector3 MoveOnTheRoad(Vector3 originPosition)
    {
        float minimum = float.MaxValue;
        Vector3 candidate = Vector3.zero;
        string nameOfBoundsForDebugging = "";
        foreach(var road in BatchedRoads)
        {
            var bounds = road.GetComponent<Renderer>().bounds;
            originPosition.y = bounds.center.y;

            if (bounds.Contains(originPosition))
            {
                return originPosition;
            }

            float distanceFromOrigin = bounds.SqrDistance(originPosition);
            if (distanceFromOrigin < minimum)
            {
                candidate = bounds.ClosestPoint(originPosition);
                minimum = distanceFromOrigin;
                nameOfBoundsForDebugging = road.name;
            }
        }
        Debug.Log(string.Format("origin {0}, result {1} from {2}", originPosition, candidate, nameOfBoundsForDebugging));
        return candidate;
    }
}
