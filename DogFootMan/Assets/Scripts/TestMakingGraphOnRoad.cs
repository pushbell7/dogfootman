using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMakingGraphOnRoad : MonoBehaviour
{
    List<GameObject> BatchedRoads;

    class DestinationPair
    {
        public Vector3 Forward;
        public Vector3 Backward;

       
        public DestinationPair(Vector3 inForward, Vector3 inBackward)
        {
            Forward = inForward;
            Backward = inBackward;
        }
    }
    Dictionary<GameObject, DestinationPair> EndPointOnEachRoad;

    // Start is called before the first frame update
    void Start()
    {
        BatchedRoads = new List<GameObject>(GameObject.FindGameObjectsWithTag("Road"));

        foreach(var road in BatchedRoads)
        {
            var roadInfo = road.GetComponent<RoadInfo>();
            Vector3 forwardDestination = roadInfo.GetDestinationOfLane(1);
            Vector3 backwardDestination = roadInfo.GetDestinationOfLane(-1);

            foreach(var anotherRoad in BatchedRoads)
            {
                if (anotherRoad == road) continue;
                var anotherRoadInfo = anotherRoad.GetComponent<RoadInfo>();

                Vector3 forwardDestinationForAnother = anotherRoadInfo.GetDestinationOfLane(1);
                Vector3 backwardDestinationForAnother = anotherRoadInfo.GetDestinationOfLane(-1);

                const float NEAR_DISTANCE = 50.0f;

                float gapOfFF = (forwardDestination - forwardDestinationForAnother).magnitude;
                float gapOfFB = (forwardDestination - backwardDestinationForAnother).magnitude;
                if (gapOfFF < gapOfFB)
                {
                    if (gapOfFF < NEAR_DISTANCE)
                        Debug.DrawLine(forwardDestination, forwardDestinationForAnother, Color.yellow, 600);
                }
                else
                {
                    if (gapOfFB < NEAR_DISTANCE)
                        Debug.DrawLine(forwardDestination, backwardDestinationForAnother, Color.yellow, 600);
                }

                float gapOfBF = (backwardDestination - forwardDestinationForAnother).magnitude;
                float gapOfBB = (backwardDestination - backwardDestinationForAnother).magnitude;
                if (gapOfBF < gapOfBB)
                {
                    if (gapOfBF < NEAR_DISTANCE)
                        Debug.DrawLine(backwardDestination, forwardDestinationForAnother, Color.yellow, 600);
                }
                else
                {
                    if (gapOfBB < NEAR_DISTANCE)
                        Debug.DrawLine(backwardDestination, backwardDestinationForAnother, Color.yellow, 600);
                }

                //if (anotherRoadInfo.IsRoadOn(forwardDestination))
                //{
                //    Debug.DrawLine(forwardDestination, forwardDestinationForAnother, Color.yellow, 600);
                //    Debug.DrawLine(forwardDestination, backwardDestinationForAnother, Color.yellow, 600);
                //}
                //if(anotherRoadInfo.IsRoadOn(backwardDestination))
                //{
                //    Debug.DrawLine(backwardDestination, forwardDestinationForAnother, Color.yellow, 600);
                //    Debug.DrawLine(backwardDestination, backwardDestinationForAnother, Color.yellow, 600);
                //}
            }
        }
    }
    
}
