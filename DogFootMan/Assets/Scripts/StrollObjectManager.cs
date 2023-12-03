using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StrollObjectManager : MonoBehaviour
{
    public GameObject CenterObjectToManage;
    public GameObject HumanPrefab;

    List<GameObject> SpawnedHuman;
    // Start is called before the first frame update
    void Start()
    {
        SpawnedHuman = new List<GameObject>();   
    }

    // Update is called once per frame
    void Update()
    {
        Spawn();
    }

    void Spawn()
    {
        while(SpawnedHuman.Count < 10)
        {
            GameObject spawnedObject = Instantiate(HumanPrefab);
            spawnedObject.transform.position = GenerateRandomPosition(spawnedObject);
            SpawnedHuman.Add(spawnedObject);
        }
    }

    Vector3 GenerateRandomPosition(GameObject spawnedObject)
    {
        Vector3 direction = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
        int size = Random.Range(5, 15);
        var positionToSpawn = CenterObjectToManage.transform.position + (direction * size);
        positionToSpawn.y = Terrain.activeTerrain.SampleHeight(positionToSpawn) + (spawnedObject.GetComponent<Collider>().bounds.size.y / 2);
        return positionToSpawn;
    }

    public static bool IsOnPath(GameObject target)
    {
        if (target == null) return false;

        var collider = target.GetComponent<CapsuleCollider>();
        var ray = new Ray(target.transform.position, -target.transform.up * collider.height);
        return Physics.Raycast(ray, collider.height, LayerMask.GetMask("Road"));
    }
}
