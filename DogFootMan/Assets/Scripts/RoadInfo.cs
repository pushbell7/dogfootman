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
}
