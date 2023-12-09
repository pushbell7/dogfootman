using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StrollObjectManager : MonoBehaviour
{
    public GameObject CenterObjectToManage;
    public GameObject HumanPrefab;
    int NumberingIndex = 0;
    Vector3 LastSpawnPosition;
    List<GameObject> SpawnedHuman;
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
        SpawnedHuman = new List<GameObject>();
        Spawn(20, CenterObjectToManage.transform.position);
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

    public void Spawn(int count, Vector3 spawnPoint)
    {
        for(int i = 0; i < count; ++i)
        {
            GameObject spawnedObject = Instantiate(HumanPrefab);
            spawnedObject.name = string.Format("human{0}", NumberingIndex++);
            spawnedObject.transform.position = GenerateRandomPosition(spawnedObject, spawnPoint);

            while (IsOnPath(spawnedObject) == false)
            {
                // there is under water
                if(spawnedObject.transform.position.y < 10.0f)
                {
                    spawnedObject.transform.position = GenerateRandomPosition(spawnedObject, spawnPoint);
                    continue;
                }
                spawnedObject.transform.position = Vector3.Lerp(spawnedObject.transform.position, spawnPoint, 0.5f);
            }
            SpawnedHuman.Add(spawnedObject);
        }
        LastSpawnPosition = CenterObjectToManage.transform.position;
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
