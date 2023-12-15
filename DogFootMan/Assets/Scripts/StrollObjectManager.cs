using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StrollObjectManager : MonoBehaviour
{
    public GameObject CenterObjectToManage;
    public GameObject HumanPrefab;
    int NumberingIndex = 0;
    List<GameObject> SpawnedHuman;
    CheckPointManager CheckPointManagerRef;
    static public StrollObjectManager Get()
    {
        return GameObject.Find("ObjectManager").GetComponent<StrollObjectManager>();
    }

    public bool IsMyCharacter(GameObject obj)
    {
        return CenterObjectToManage == obj;
    }

    // Start is called before the first frame update
    void Start()
    {
        CheckPointManagerRef = GameObject.Find("CheckPoints").GetComponent<CheckPointManager>();
        SpawnedHuman = new List<GameObject>();
        Spawn(20, 0);
    }

    // Update is called once per frame
    void Update()
    {
        RemoveFarHumans();
    }

    void RemoveFarHumans()
    {
        for (int i = SpawnedHuman.Count - 1; i >= 0; --i)
        {
            GameObject human = SpawnedHuman[i];
            if (Vector3.Distance(human.transform.position, CenterObjectToManage.transform.position) > 500.0f)
            {
                SpawnedHuman.RemoveAt(i);
                Destroy(human);
            }
        }
    }

    public void Spawn(int count, int checkPointIndexToSpawn)
    {
        Vector3 spawnPoint = CheckPointManagerRef.GetPositionFrom(checkPointIndexToSpawn);
        for(int i = 0; i < count; ++i)
        {
            GameObject spawnedObject = Instantiate(HumanPrefab);
            spawnedObject.name = string.Format("Human{0}", NumberingIndex++);
            spawnedObject.transform.position = GenerateRandomPosition(spawnedObject, spawnPoint);

            while (IsOnPath(spawnedObject) == false)
            {
                bool bIsUnderWater = spawnedObject.transform.position.y < 10.0f;
                bool bIsTooNear = Vector3.Distance(spawnedObject.transform.position, spawnPoint) < 10.0f;
                if (bIsUnderWater || bIsTooNear)
                {
                    spawnedObject.transform.position = GenerateRandomPosition(spawnedObject, spawnPoint);
                    continue;
                }
                
                spawnedObject.transform.position = Vector3.Lerp(spawnedObject.transform.position, spawnPoint, 0.5f);
            }
            spawnedObject.GetComponent<StrollHumanController>().SetSpawnedCheckPointIndex(checkPointIndexToSpawn);
            SpawnedHuman.Add(spawnedObject);
        }
        Debug.Log("spawned");
    }

    Vector3 GenerateRandomPosition(GameObject spawnedObject, Vector3 spawnPoint)
    {
        Vector3 direction = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
        int size = Random.Range(0, 100);
        var positionToSpawn = spawnPoint + (direction * size);
        positionToSpawn.y = GetActiveTerrain(positionToSpawn).SampleHeight(positionToSpawn) + (spawnedObject.GetComponent<Collider>().bounds.size.y / 2);
        return positionToSpawn;
    }

    public static bool IsOnPath(GameObject target)
    {
        if (target == null) return false;

        var collider = target.GetComponent<CapsuleCollider>();
        var ray = new Ray(target.transform.position, -target.transform.up * collider.height);
        return Physics.Raycast(ray, collider.height, LayerMask.GetMask("Road"));
    }

    static Terrain GetActiveTerrain(Vector3 position)
    {
        Terrain result = null;
        float minimum = float.MaxValue;
        foreach(Terrain activeTerrain in Terrain.activeTerrains)
        {
            float distance = (activeTerrain.GetPosition() - position).sqrMagnitude;
            if (minimum > distance)
            {
                minimum = distance;
                result = activeTerrain;
            }
        }
        return result;
    }
}
