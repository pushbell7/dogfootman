using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrafficControlTrigger : MonoBehaviour
{
    List<GameObject> WaitingObjectUnderControl;

    enum TrafficState
    {
        GO,
        STOP,
        MAX
    }
    private TrafficState CurrentTrafficState;

    // Start is called before the first frame update
    void Start()
    {
        WaitingObjectUnderControl = new List<GameObject>();
    }

    private void Update()
    {
        const float INTERVAL = 5;
        CurrentTrafficState = (TrafficState)(Time.time / INTERVAL % (int)TrafficState.MAX);

        switch(CurrentTrafficState)
        {
            case TrafficState.GO:
                WaitingObjectUnderControl.ForEach(obj => obj.GetComponent<CarController>()?.SetWait(false));
                break;
            case TrafficState.STOP:
                WaitingObjectUnderControl.ForEach(obj => obj.GetComponent<CarController>()?.SetWait(true));
                break;
            default: break;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        WaitingObjectUnderControl.Add(other.gameObject);
    }

    private void OnTriggerExit(Collider other)
    {
        WaitingObjectUnderControl.Remove(other.gameObject);
    }
}
