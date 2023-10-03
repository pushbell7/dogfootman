using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrafficControlTrigger : MonoBehaviour
{
    Dictionary<GameObject, List<GameObject>> WaitingObjectUnderControl;
    int CurrentTrafficIndex;

    private List<GameObject> ConnectedRoads;
    private Dictionary<GameObject, List<Vector3>> CandidatePointMap;

    Vector3[] Edges = new Vector3[4];
    void MakeSpans()
    {
        var boxCollider = GetComponent<BoxCollider>();
        var halfSizeX = boxCollider.size.x * transform.localScale.x / 2;
        var halfSizeZ = boxCollider.size.z * transform.localScale.z / 2;
        Edges[0] = transform.position + transform.rotation * (new Vector3(-halfSizeX, 0, -halfSizeZ));
        Edges[1] = transform.position + transform.rotation * (new Vector3(halfSizeX, 0, -halfSizeZ));
        Edges[2] = transform.position + transform.rotation * (new Vector3(halfSizeX, 0, halfSizeZ));
        Edges[3] = transform.position + transform.rotation * (new Vector3(-halfSizeX, 0, halfSizeZ));
    }

    Vector3 GetPointInSpan(Vector3 target, Vector3 forward)
    {
        int count = Edges.Length;
        for (int i = 0; i < count; ++i)
        {
            Vector3 edge1 = Edges[i % count];
            Vector3 edge2 = Edges[(i + 1) % count];
            var intersectionPoint = GetIntersectionPoint(edge1, edge2, target, target + forward);

            if(IsInSpan(edge1, edge2, intersectionPoint) && IsSameDirection(forward, intersectionPoint - target))
            {
                return intersectionPoint;
            }
        }
        return transform.position;
    }
    bool IsSameDirection(Vector3 direction1, Vector3 direction2)
    {
        return Vector3.Dot(direction1, direction2) > 0;
    }
    bool IsInSpan(Vector3 spanVector1, Vector3 spanVector2, Vector3 pointToCheck)
    {
        Vector3 dir1 = spanVector2 - pointToCheck;
        Vector3 dir2 = spanVector1 - pointToCheck;

        return Vector3.Dot(dir1, dir2) < 0;
    }

    Vector3 GetIntersectionPoint(Vector3 p1, Vector3 p2, Vector3 q1, Vector3 q2)
    {
        Vector3 d1 = p2 - p1;
        Vector3 d2 = q2 - q1;

        Vector3 n = Vector3.Cross(d1, d2);

        if(n.sqrMagnitude < float.Epsilon)
        {
            return Vector3.zero;
        }
        var newNormal1 = Vector3.Cross(q1 - p1, d2);
        float t = Vector3.Dot(newNormal1, n) / Vector3.Dot(n, n);
        Vector3 result1 = p1 + t * d1;

        // to check if result1 == result2
        //var newNormal2 = Vector3.Cross(q1 - p1, d1);
        //float s = Vector3.Dot(newNormal2, n) / Vector3.Dot(n, n);
        //Vector3 result2 = q1 + s * d2;

        return result1;
    }

    // Start is called before the first frame update
    void Start()
    {
        WaitingObjectUnderControl = new Dictionary<GameObject, List<GameObject>>();

        MakeSpans();

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
                    var startingPoint = roadInfo.GetStartingPointOfLane(i);
                    candidatePoint.Add(GetPointInSpan(startingPoint, road.transform.forward));
                }
            }
            for(int i = -1; i >= -roadInfo.BackwardLaneCount; --i)
            {
                if (RoadInfo.IsInRotatedRectangle(roadInfo.GetStartingPointOfLane(i), transform.position, size / 2, transform.rotation.eulerAngles.y))
                {
                    var startingPoint = roadInfo.GetStartingPointOfLane(i);
                    candidatePoint.Add(GetPointInSpan(startingPoint, -road.transform.forward));
                }
            }

            CandidatePointMap.Add(road, candidatePoint);
        }
    }
    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;

        int count = Edges.Length;
        for (int i = 0; i < count; ++i)
        {
            Vector3 edge1 = Edges[i % count];
            Vector3 edge2 = Edges[(i + 1) % count];
            Gizmos.DrawLine(edge1, edge2);
        }
        //var collider = GetComponent<BoxCollider>();
        //Vector3 size = new Vector3(collider.size.x * transform.localScale.x, collider.size.y * transform.localScale.y, collider.size.z * transform.localScale.z);
        //Vector3 position = transform.position;
        //Quaternion rotation = transform.rotation;
        //Gizmos.matrix = Matrix4x4.TRS(position, rotation, size);
        //Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
        if (CandidatePointMap == null) return;

        foreach(var pointPair in CandidatePointMap)
        {
            foreach(var point in pointPair.Value)
            {
                Gizmos.DrawSphere(point, 5);
            }
        }
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
            int randomIndex = Random.Range(0, leftRoads.Count);
            if(leftRoads.Count <= randomIndex)
            {
                Debug.Log(string.Format("{0} : count:{1}", currentRoad.name, leftRoads.Count));
            }
            var targetRoad = leftRoads[randomIndex];
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
