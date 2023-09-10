using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrafficControlTrigger : MonoBehaviour
{
    List<GameObject> WaitingObjectUnderControl;

    enum TrafficState
    {
        Horizontal,
        Vertical,
        MAX
    }
    private TrafficState CurrentTrafficState;
    private List<GameObject> ConnectedRoads;
    private Dictionary<GameObject, List<Vector3>> CandidatePointMap;

    // Start is called before the first frame update
    void Start()
    {
        WaitingObjectUnderControl = new List<GameObject>();

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
        CurrentTrafficState = (TrafficState)(Time.time / INTERVAL % (int)TrafficState.MAX);

        switch(CurrentTrafficState)
        {
            case TrafficState.Horizontal:
                WaitingObjectUnderControl.ForEach(obj => obj.GetComponent<CarController>()?.SetWait(false));
                break;
            case TrafficState.Vertical:
                WaitingObjectUnderControl.ForEach(obj => obj.GetComponent<CarController>()?.SetWait(true));
                break;
            default: break;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        WaitingObjectUnderControl.Add(other.gameObject);

        MakeDestination(other.gameObject);
    }

    private void OnTriggerExit(Collider other)
    {
        WaitingObjectUnderControl.Remove(other.gameObject);

        other.gameObject.GetComponent<CarController>()?.SetWait(false);
    }

    void MakeDestination(GameObject InObject)
    {
        // @TODO: flow control 
        // 1. define connected road count
        //ConnectedRoads.Count;
        // 2. which road is InObject on?
        // 3. which lane does InObject use?
        // 4. find road to use and decide point from CandidatePointMap
    }
}
