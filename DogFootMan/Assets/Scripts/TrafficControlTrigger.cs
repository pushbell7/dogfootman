using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrafficControlTrigger : MonoBehaviour
{
    Dictionary<GameObject, List<GameObject>> WaitingObjectUnderControl;
    int CurrentTrafficIndex;

    private List<GameObject> ConnectedRoads;
    private Dictionary<GameObject, List<Vector3>> CandidatePointMap;

    // Start is called before the first frame update
    void Start()
    {
        WaitingObjectUnderControl = new Dictionary<GameObject, List<GameObject>>();

        // get connected roads in this volume
        ConnectedRoads = new List<GameObject>();
        CandidatePointMap = new Dictionary<GameObject, List<Vector3>>();

        var collider = GetComponent<BoxCollider>();
        Vector3 size = new Vector3(collider.size.x * transform.localScale.x, collider.size.y * transform.localScale.y, collider.size.z * transform.localScale.z);
        Collider[] colliders = Physics.OverlapBox(transform.position, size / 2, transform.rotation);

        foreach(var selectedCollider in colliders)
        {
            if (selectedCollider.CompareTag("Road"))
            {
                ConnectedRoads.Add(selectedCollider.gameObject);
                WaitingObjectUnderControl.Add(selectedCollider.gameObject, new List<GameObject>());
            }
        }

        // get every lane point
        foreach(var road in ConnectedRoads)
        {
            var candidatePoint = new List<Vector3>();
            var roadInfo = road.GetComponent<RoadInfo>();
            for(int i = 1; i <= roadInfo.ForwardLaneCount; ++i)
            {
                if (RoadInfo.IsInRotatedRectangle(roadInfo.GetStartingPointOfLane(i), transform.position, size / 2, transform.rotation.eulerAngles.y))
                {
                    candidatePoint.Add(roadInfo.GetStartingPointOfLane(i));
                }
            }
            for(int i = -1; i >= -roadInfo.BackwardLaneCount; --i)
            {
                if (RoadInfo.IsInRotatedRectangle(roadInfo.GetStartingPointOfLane(i), transform.position, size / 2, transform.rotation.eulerAngles.y))
                {
                    candidatePoint.Add(roadInfo.GetStartingPointOfLane(i));
                }
            }

            CandidatePointMap.Add(road, candidatePoint);
        }
    }
    void OnDrawGizmos()
    {
        //Gizmos.color = Color.blue;

        //var collider = GetComponent<BoxCollider>();
        //Vector3 size = new Vector3(collider.size.x * transform.localScale.x, collider.size.y * transform.localScale.y, collider.size.z * transform.localScale.z);
        //Vector3 position = transform.position;
        //Quaternion rotation = transform.rotation;
        //Gizmos.matrix = Matrix4x4.TRS(position, rotation, size);
        //Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
    }

    private void Update()
    {
        const float INTERVAL = 5;
        CurrentTrafficIndex = (int)(Time.time / INTERVAL) % WaitingObjectUnderControl.Count;

        int index = 0;
        foreach(var waitingList in WaitingObjectUnderControl)
        {
            waitingList.Value.ForEach(obj => obj.GetComponent<CarController>()?.SetWait(index == CurrentTrafficIndex));
            index++;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        var carController = other.gameObject.GetComponent<CarController>();
        if (carController)
        {
            var road = ObjectManager.Get().FindRoadOn(other.transform.position);
            if (road)
            {
                WaitingObjectUnderControl[road].Add(other.gameObject);
                carController.SetTraffic(true);
            }
            MakeDestination(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        var carController = other.gameObject.GetComponent<CarController>();
        if (carController)
        {
            var road = ObjectManager.Get().FindRoadOn(other.transform.position);
            if (road)
            {
                WaitingObjectUnderControl[road].Remove(other.gameObject);
            }
            carController.SetWait(false);
            carController.SetTraffic(false);
        }
    }

    void MakeDestination(GameObject InObject)
    {
        var road = ObjectManager.Get().FindRoadOn(InObject.transform.position);
        if(road)
        {
            var lane = road.GetComponent<RoadInfo>().GetTheNumberOfLane(InObject.transform.position);
            InObject.GetComponent<CarController>()?.SetDestination(GetDestination(road, lane));
        }
    }

    Vector3 GetDestination(GameObject currentRoad, int lane)
    {
        GameObject rightestRoad = null;
        float maximumDotResult = float.MinValue;
        
        foreach(var road in ConnectedRoads)
        {
            if (road == currentRoad) continue;
            float dot = Vector3.Dot((road.transform.position - currentRoad.transform.position).normalized, currentRoad.transform.right);
            if(dot > maximumDotResult)
            {
                maximumDotResult = dot;
                rightestRoad = road;
            }
        }

        if(IsEdgeLane(currentRoad, lane))
        {
            return CandidatePointMap[rightestRoad].FindLast( param =>{ return true; });
        }
        else
        {
            var leftRoads = ConnectedRoads.FindAll(param => { return param != rightestRoad && param != currentRoad; });
            var targetRoad = leftRoads[Random.Range(0, leftRoads.Count)];
            return CandidatePointMap[targetRoad].Find(param => { return true; });
        }
    }
    bool IsEdgeLane(GameObject currentRoad, int lane)
    {
        var roadInfo = currentRoad.GetComponent<RoadInfo>();
        if (lane < 0)
        {
            return -lane == roadInfo.BackwardLaneCount;
        }
        else
        {
            return lane == roadInfo.ForwardLaneCount;
        }
    }
}
