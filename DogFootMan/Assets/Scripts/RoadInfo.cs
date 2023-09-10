using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Junction
{
    public GameObject NextRoad;
    public Vector3 WaitingPoint;
}

public class RoadInfo : MonoBehaviour
{
    // Forward Land is z direction of road
    public int ForwardLaneCount;
    public int BackwardLaneCount;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }
    void OnDrawGizmos()
    {
        //Gizmos.color = Color.red;

        //var collider = GetComponent<BoxCollider>();
        //Vector3 size = new Vector3(collider.size.x * transform.localScale.x, collider.size.y * transform.localScale.y, collider.size.z * transform.localScale.z);
        //Vector3 position = transform.position;
        //Quaternion rotation = transform.rotation;
        //Gizmos.matrix = Matrix4x4.TRS(position, rotation, size);
        //Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
    }

    public Vector3 GetCenterOfLanePosition(int lane, Vector3 currentPositionOfRunningObject)
    {
        var result = GetCenterOfLanePosition(lane, new Vector2(currentPositionOfRunningObject.x, currentPositionOfRunningObject.z));
        return new Vector3(result.x, currentPositionOfRunningObject.y, result.y);
    }

    Vector2 GetCenterOfLanePosition(int lane, Vector2 currentPositionOfRunningObject)
    {
        Vector2 myPosition = new Vector2(gameObject.transform.position.x, gameObject.transform.position.z);
        currentPositionOfRunningObject -= myPosition;

        Quaternion inverseRotation = Quaternion.Euler(0f, 0f, gameObject.transform.rotation.eulerAngles.y);
        currentPositionOfRunningObject = inverseRotation * currentPositionOfRunningObject;

        var boxCollider = gameObject.GetComponent<BoxCollider>();
        float laneWidth = boxCollider.size.x * gameObject.transform.localScale.x / (ForwardLaneCount + BackwardLaneCount);
        float centerOfLane = laneWidth * lane - (lane > 0 ? 1 : -1) * laneWidth / 2;
        currentPositionOfRunningObject.x = centerOfLane;

        currentPositionOfRunningObject = Quaternion.Euler(0f, 0f, -gameObject.transform.rotation.eulerAngles.y) * currentPositionOfRunningObject;
        return currentPositionOfRunningObject += myPosition;
    }

    public Vector3 GetDestinationOfLane(int lane)
    {
        var destination = GetDestinationOfLaneImpl(lane);
        return new Vector3(destination.x, 20f, destination.y);
    }

    Vector2 GetDestinationOfLaneImpl(int lane)
    {
        var boxCollider = GetComponent<BoxCollider>();
        var halfSizeZ = boxCollider.size.z * gameObject.transform.localScale.z / 2;
        float laneWidth = boxCollider.size.x * gameObject.transform.localScale.x / (ForwardLaneCount + BackwardLaneCount);
        float centerOfLane = laneWidth * lane - (lane > 0 ? 1 : -1) * laneWidth / 2;
        Vector2 destination = new Vector2(centerOfLane, lane > 0 ? halfSizeZ : -halfSizeZ);

        destination = Quaternion.Euler(0f, 0f, -gameObject.transform.rotation.eulerAngles.y) * destination;
        destination += new Vector2(gameObject.transform.position.x, gameObject.transform.position.z);
        return destination;
    }

    public Vector3 GetStartingPointOfLane(int lane)
    {
        var boxCollider = GetComponent<BoxCollider>();
        var halfSizeZ = boxCollider.size.z * gameObject.transform.localScale.z / 2;
        float laneWidth = boxCollider.size.x * gameObject.transform.localScale.x / (ForwardLaneCount + BackwardLaneCount);
        float centerOfLane = laneWidth * lane - (lane > 0 ? 1 : -1) * laneWidth / 2;
        Vector2 destination = new Vector2(centerOfLane, lane > 0 ? -halfSizeZ : halfSizeZ);

        destination = Quaternion.Euler(0f, 0f, -gameObject.transform.rotation.eulerAngles.y) * destination;
        destination += new Vector2(gameObject.transform.position.x, gameObject.transform.position.z);

        return new Vector3(destination.x, 20f, destination.y);
    }

    public int GetTheNumberOfLane(Vector3 currentPositionOfRunningObject)
    {
        currentPositionOfRunningObject -= gameObject.transform.position;

        Quaternion inverseRotation = Quaternion.Euler(0f, 0f, gameObject.transform.rotation.eulerAngles.y);
        currentPositionOfRunningObject = inverseRotation * currentPositionOfRunningObject;

        var boxCollider = gameObject.GetComponent<BoxCollider>();
        float laneWidth = boxCollider.size.x * gameObject.transform.localScale.x / (ForwardLaneCount + BackwardLaneCount);
        bool bIsForward = currentPositionOfRunningObject.x > 0;
        int theNumberOfLane = (int)Mathf.Abs(currentPositionOfRunningObject.x / laneWidth);

        return (bIsForward ? 1 : -1) * (theNumberOfLane + 1);
    }

    public bool IsRoadOn(Vector3 positionToCheck)
    {
        var boxCollider = GetComponent<BoxCollider>();
        var center = transform.position + boxCollider.center;
        float xScale = boxCollider.size.x * transform.localScale.x / 2;
        float zScale = boxCollider.size.z * transform.localScale.z / 2;

        return IsInRotatedRectangle(positionToCheck, center, new Vector3(xScale, 0, zScale), transform.rotation.eulerAngles.y);
    }

    static public bool IsInRotatedRectangle(Vector3 point, Vector3 rectanglePosition, Vector3 halfRectangleSize, float rectangleRotation)
    {
        return IsInRotatedRectangle(new Vector2(point.x, point.z), new Vector2(rectanglePosition.x, rectanglePosition.z), new Vector2(halfRectangleSize.x, halfRectangleSize.z), rectangleRotation);
    }

    static bool IsInRotatedRectangle(Vector2 point, Vector2 rectanglePosition, Vector2 halfRectangleSize, float rectangleRotation)
    {
        // Translate the point to the rectangle's local coordinate space
        Vector2 center = rectanglePosition;
        Vector2 translatedPoint = point - center;

        // Apply the inverse rotation of the rectangle to the translated point
        Quaternion inverseRotation = Quaternion.Euler(0f, 0f, rectangleRotation);
        Vector2 rotatedPoint = inverseRotation * translatedPoint;

        return (Mathf.Abs(rotatedPoint.x) <= halfRectangleSize.x) && (Mathf.Abs(rotatedPoint.y) <= halfRectangleSize.y);
    }
}
