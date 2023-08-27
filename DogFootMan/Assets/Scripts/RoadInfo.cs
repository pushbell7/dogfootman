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
}
