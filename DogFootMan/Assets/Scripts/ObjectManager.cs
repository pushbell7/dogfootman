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
    private Dictionary<int, int> CurrentIndexMap;

    private List<GameObject> BatchedRoads;

    // Start is called before the first frame update
    void Start()
    {
        SpawnedObjectMap = new Dictionary<int, List<GameObject>>();
        MaxCountForObjectMap = new Dictionary<int, int>();
        CurrentIndexMap = new Dictionary<int, int>();

        int index = 0;
        int[] temporaryMaxCounts = { 20, 10, 5, };
        foreach (var prefab in PrefabsToManage) 
        {
            SpawnedObjectMap.Add(index, new List<GameObject>());
            MaxCountForObjectMap.Add(index, temporaryMaxCounts[index]);
            CurrentIndexMap.Add(index, 0);
            index++;
        }

        BatchedRoads = new List<GameObject>(GameObject.FindGameObjectsWithTag("Road"));

        DrawDebugBounds();
    }

    // Update is called once per frame
    void Update()
    {
        if (CenterObjectToManage == null) return;

        //DestroyObjects();
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
        string[] temporaryName = { "car", "human", "item" };
        int index = 0;
        foreach (var spawnedObjects in SpawnedObjectMap)
        {
            string curruentName = temporaryName[index];
            while (spawnedObjects.Value.Count < MaxCountForObjectMap[spawnedObjects.Key])
            {
                GameObject spawnedObject = Instantiate(PrefabsToManage[spawnedObjects.Key]);
                spawnedObject.transform.position = GeneratePosition(spawnedObject);

                int indexOfCurrentType = CurrentIndexMap[index];
                spawnedObject.name = string.Format("{0}{1}", curruentName, indexOfCurrentType);
                CurrentIndexMap[index] = ++indexOfCurrentType;

                spawnedObject.tag = "Obstacles";

                spawnedObjects.Value.Add(spawnedObject);
            }
            index++;
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
        //string nameOfBoundsForDebugging = "";
        foreach(var road in BatchedRoads)
        {
            var boxCollider = road.GetComponent<BoxCollider>();
            var center = road.transform.position + boxCollider.center;
            float xScale = boxCollider.size.x * road.transform.localScale.x / 2;
            float zScale = boxCollider.size.z * road.transform.localScale.z / 2;

            Vector2 tempNearestPoint = GetNearestPointOnRectangle(new Vector2(originPosition.x, originPosition.z), new Vector2(center.x, center.z), new Vector2(xScale, zScale), road.transform.rotation.eulerAngles.y);
            Vector3 nearestPoint = new Vector3(tempNearestPoint.x, 20, tempNearestPoint.y);
            float distance = (nearestPoint - originPosition).magnitude;
            if(distance < minimum)
            {
                candidate = nearestPoint;
                minimum = distance;
                //nameOfBoundsForDebugging = road.name;

            }
        }
        //Debug.DrawLine(originPosition, candidate, Color.green, 60.0f);
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

    // generated by chatgpt. some bugs are fixed
    Vector2 GetNearestPointOnRectangle(Vector2 point, Vector2 rectanglePosition, Vector2 halfRectangleSize, float rectangleRotation)
    {
        // Translate the point to the rectangle's local coordinate space
        Vector2 center = rectanglePosition;
        Vector2 translatedPoint = point - center;

        // Apply the inverse rotation of the rectangle to the translated point
        Quaternion inverseRotation = Quaternion.Euler(0f, 0f, rectangleRotation);
        Vector2 rotatedPoint = inverseRotation * translatedPoint;

        const float PADDING = 1.0f;
        halfRectangleSize.x -= PADDING;
        halfRectangleSize.y -= PADDING;

        // Clamp the rotated point to the rectangle's bounds
        Vector2 clampedPoint = new Vector2(
            Mathf.Clamp(rotatedPoint.x, -halfRectangleSize.x, halfRectangleSize.x),
            Mathf.Clamp(rotatedPoint.y, -halfRectangleSize.y, halfRectangleSize.y)
        );

        // Apply the original rotation to the clamped rotated point
        Vector2 clampedRotatedPoint = Quaternion.Euler(0f, 0f, -rectangleRotation) * clampedPoint;

        // Add the center position back to the clamped, rotated point
        Vector2 nearestPoint = clampedRotatedPoint + center;

        return nearestPoint;
    }


    public bool IsOnRoad(Vector3 positionToCheck)
    {
        return FindRoadOn(positionToCheck) != null;
    }

    public GameObject FindRoadOn(Vector3 positionToCheck)
    {
        foreach(var road in BatchedRoads)
        {
            var roadInfo = road.GetComponent<RoadInfo>();
            if (roadInfo.IsRoadOn(positionToCheck))
            {
                return road;
            }
        }
        return null;
    }

}
