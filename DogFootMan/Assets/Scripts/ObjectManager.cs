using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class PrefabToManage
{
    public int Key;
    public string Name;
    public GameObject Prefab;
    public int MaxCountToMake;
    private int CurrentCount;

    static int DEFAULT_KEY = 0;
    public PrefabToManage()
    {
        Key = DEFAULT_KEY++;
    }

    public int GetCurrentCount() { return CurrentCount; }
    public void IncreaseCurrentCount() { CurrentCount++; }
    public void DecreaseCurrentCount() { CurrentCount--; }
}

public class ObjectManager : MonoBehaviour
{
    public enum ObjectType
    {
        Car,
        Human,
        ItemToRide,
        MyCharacter,
        None
    }
    public List<PrefabToManage> PrefabsToManage;
    public GameObject CenterObjectToManage;

    private Dictionary<ObjectType, List<GameObject>> SpawnedObjectMap;

    private List<GameObject> BatchedRoads;

    static public ObjectManager Get()
    {
        return GameObject.Find("ObjectManager").GetComponent<ObjectManager>();
    }

    // Start is called before the first frame update
    void Start()
    {
        SpawnedObjectMap = new Dictionary<ObjectType, List<GameObject>>();

        int index = 0;
        foreach (var prefab in PrefabsToManage) 
        {
            SpawnedObjectMap.Add((ObjectType)(index++), new List<GameObject>());
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
                if (element)
                {
                    if (Vector3.Distance(element.transform.position, MainCharacterPosition) > DistanceToRemove)
                    {
                        spawnedObjects.Value.RemoveAt(i);
                        Destroy(element);
                        var CurrentPrefab = PrefabsToManage[(int)spawnedObjects.Key];
                        CurrentPrefab.DecreaseCurrentCount();
                    }
                }
            }
        }
    }

    void SpawnObjects()
    {
        foreach (var spawnedObjects in SpawnedObjectMap)
        {
            var CurrentPrefab = PrefabsToManage[(int)spawnedObjects.Key];
            while (spawnedObjects.Value.Count < CurrentPrefab.MaxCountToMake)
            {
                GameObject spawnedObject = Instantiate(CurrentPrefab.Prefab);
                spawnedObject.name = string.Format("{0}{1}", CurrentPrefab.Name, CurrentPrefab.GetCurrentCount());
                spawnedObject.tag = "Obstacles";

                if (CurrentPrefab.Name.Equals("Car"))
                {
                    spawnedObject.transform.position = GeneratePositionOnRoad(spawnedObject);
                    var road = FindRoadOn(spawnedObject.transform.position);
                    if (road != null)
                    {
                        bool bIsForward = spawnedObject.GetComponent<CarController>().IsForwardDirectionInRotatedRectangle(road);

                        spawnedObject.transform.rotation = Quaternion.LookRotation((bIsForward ? 1 : -1) * road.transform.forward);
                    }
                }
                else
                {
                    spawnedObject.transform.position = GeneratePositionOutOfRoad(spawnedObject);
                }
                CurrentPrefab.IncreaseCurrentCount();
                spawnedObjects.Value.Add(spawnedObject);
            }
        }
    }

    Vector3 GeneratePositionOnRoad(GameObject spawnedObject)
    {
        return MoveOnTheRoad(GenerateRandomPosition(spawnedObject));
    }
    Vector3 GeneratePositionOutOfRoad(GameObject spawnedObject)
    {
        var positionToSpawn = GenerateRandomPosition(spawnedObject);
        var currentRoad = FindRoadOn(positionToSpawn);
        if(currentRoad != null)
        {
            var boxCollider = currentRoad.GetComponent<BoxCollider>();
            float xScale = boxCollider.size.x * currentRoad.transform.localScale.x / 2;
            var diff = positionToSpawn - currentRoad.transform.position;
            Vector3 newPosition = Vector3.zero;
            if (Vector3.Dot(diff, currentRoad.transform.right) < 0)
            {
                newPosition = positionToSpawn - currentRoad.transform.right * xScale;
            }
            else
            {
                newPosition = positionToSpawn + currentRoad.transform.right * xScale;
            }
            if(IsOnRoad(newPosition))
            {
                return GeneratePositionOutOfRoad(spawnedObject);
            }
            return newPosition;
        }
        return positionToSpawn;
    }

    Vector3 GenerateRandomPosition(GameObject spawnedObject)
    {
        Vector3 direction = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
        int size = Random.Range(25, 75);
        var positionToSpawn = CenterObjectToManage.transform.position + (direction * size);
        positionToSpawn.y = Terrain.activeTerrain.SampleHeight(positionToSpawn) + (spawnedObject.GetComponent<Collider>().bounds.size.y / 2);
        return positionToSpawn;
    }

    Vector3 MoveOnTheRoad(Vector3 originPosition)
    {
        float minimum = float.MaxValue;
        Vector3 candidate = Vector3.zero;
        foreach(var road in BatchedRoads)
        {
            var boxCollider = road.GetComponent<BoxCollider>();
            var center = road.transform.position + boxCollider.center;
            float xScale = boxCollider.size.x * road.transform.localScale.x / 2;
            float zScale = boxCollider.size.z * road.transform.localScale.z / 2;

            Vector2 tempNearestPoint = GetNearestPointOnRectangle(new Vector2(originPosition.x, originPosition.z), new Vector2(center.x, center.z), new Vector2(xScale, zScale), road.transform.rotation.eulerAngles.y);
            Vector3 nearestPoint = new Vector3(tempNearestPoint.x, originPosition.y, tempNearestPoint.y);
            float distance = (nearestPoint - originPosition).magnitude;
            if(distance < minimum)
            {
                candidate = nearestPoint;
                minimum = distance;
            }
        }
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
        var ray = new Ray(positionToCheck, new Vector3(0f, -1f, 0f));
        const float SafeDistance = 10.0f;
        RaycastHit hitResult;
        if (Physics.Raycast(ray, out hitResult, SafeDistance, LayerMask.GetMask("Road")))
        {
            return hitResult.collider.gameObject;
        }
        return null;
    }

    public bool IsMyCharacter(GameObject other)
    {
        return other == CenterObjectToManage;
    }

    public static ObjectType GetType(GameObject gameObjectToGet)
    {
        string name = gameObjectToGet.name;
        if (name.Contains("Car")) { return ObjectType.Car; }
        else if (name.Contains("Human")) { return ObjectType.Human; }
        else if (name.Contains("MainCharacter")) { return ObjectType.MyCharacter; }
        else if (name.Contains("ItemToRide")) { return ObjectType.ItemToRide; }
        else { return ObjectType.None; }
    }
}
