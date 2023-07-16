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

        DrawDebugBounds();
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
            var boxCollider = road.GetComponent<BoxCollider>();
            var center = road.transform.position + boxCollider.center;
            float xScale = boxCollider.size.x * road.transform.localScale.x / 2;
            float zScale = boxCollider.size.z * road.transform.localScale.z / 2;
            Vector3 scaleVector = new Vector3(xScale, 0, zScale);
            scaleVector = road.transform.rotation * scaleVector;
            Vector3 right = road.transform.rotation * Vector3.right * xScale * 2;

            Vector3 topLeft = center + scaleVector;
            Vector3 topRight = topLeft - right;
            Vector3 bottomRight = center - scaleVector;
            Vector3 bottomLeft = bottomRight + right;

            if (PointInRectangle(topLeft, topRight, bottomRight, bottomLeft, originPosition))
            {
                candidate = originPosition;
                nameOfBoundsForDebugging = road.name;
                break;
            }

            float distanceFromOrigin = GetDistanceFromRotatedRectangle(new Vector2(originPosition.x, originPosition.z), new Vector2(center.x, center.z), new Vector2(xScale * 2, zScale * 2), road.transform.rotation.eulerAngles.y);
            if (distanceFromOrigin < minimum)
            {
                candidate = GetClosestPoint(new Vector2(originPosition.x, originPosition.z), new Vector2(center.x, center.z), new Vector2(xScale * 2, zScale * 2), road.transform.rotation.eulerAngles.y);
                candidate = new Vector3(candidate.x, 20, candidate.y);
                minimum = distanceFromOrigin;
                nameOfBoundsForDebugging = road.name;
            }
        }
        Debug.Log(string.Format("origin {0}, result {1} from {2}", originPosition, candidate, nameOfBoundsForDebugging));
        Debug.DrawLine(originPosition, candidate, Color.red, 60.0f);
        return candidate;
    }

    void DrawDebugBounds()
    {
        foreach(var road in BatchedRoads)
        {
            var boxCollider = road.GetComponent<BoxCollider>();
            var center = road.transform.position + boxCollider.center;
            float xScale = boxCollider.size.x * road.transform.localScale.x / 2;
            float zScale = boxCollider.size.z * road.transform.localScale.z / 2;
            Vector3 scaleVector = new Vector3(xScale, 0, zScale);
            scaleVector = road.transform.rotation * scaleVector;
            Vector3 right = road.transform.rotation * Vector3.right * xScale * 2;

            Vector3 topLeft = center + scaleVector;
            Vector3 topRight = topLeft - right;
            Vector3 bottomRight = center - scaleVector;
            Vector3 bottomLeft = bottomRight + right;

            DrawDebugRect(topLeft, topRight, bottomRight, bottomLeft);
        }
    }

    void DrawDebugRect(Vector3 topLeft, Vector3 topRight, Vector3 bottomRight, Vector3 bottomLeft)
    {
        const float DURATION = 60 * 10.0f;
        Debug.DrawLine(topLeft, topRight, Color.red, DURATION);
        Debug.DrawLine(topRight, bottomRight, Color.red, DURATION);
        Debug.DrawLine(bottomRight, bottomLeft, Color.red, DURATION);
        Debug.DrawLine(bottomLeft, topLeft, Color.red, DURATION);
    }

    // copied from https://gamedev.stackexchange.com/questions/110229/how-do-i-efficiently-check-if-a-point-is-inside-a-rotated-rectangle
    float isLeft(Vector3 P0, Vector3 P1, Vector3 P2)
    {
        return ((P1.x - P0.x) * (P2.z - P0.z) - (P2.x - P0.x) * (P1.z - P0.z));
    }
    bool PointInRectangle(Vector3 X, Vector3 Y, Vector3 Z, Vector3 W, Vector3 P)
    {
        return (isLeft(X, Y, P) > 0 && isLeft(Y, Z, P) > 0 && isLeft(Z, W, P) > 0 && isLeft(W, X, P) > 0);
    }

    float GetDistanceFromRotatedRectangle(Vector2 point, Vector2 rectanglePosition, Vector2 rectangleSize, float rectangleRotation)
    {
        Vector2 finalPoint = GetClosestPoint(point, rectanglePosition, rectangleSize, rectangleRotation);
        float distance = Vector2.Distance(finalPoint, point);
        return distance;
    }
    Vector2 GetClosestPoint(Vector2 point, Vector2 rectanglePosition, Vector2 rectangleSize, float rectangleRotation)
    {
        Vector2 center = rectanglePosition;
        Vector2 translatedPoint = point - center;

        Quaternion inverseRotation = Quaternion.Euler(0f, 0f, -rectangleRotation);
        Vector2 rotatedPoint = inverseRotation * translatedPoint;

        Vector2 halfSize = rectangleSize / 2f;
        Vector2 clampedPoint = new Vector2(
            Mathf.Clamp(rotatedPoint.x, -halfSize.x, halfSize.x),
            Mathf.Clamp(rotatedPoint.y, -halfSize.y, halfSize.y)
        );

        Vector2 clampedRotatedPoint = inverseRotation * clampedPoint;
        return clampedRotatedPoint + center;
    }
}
