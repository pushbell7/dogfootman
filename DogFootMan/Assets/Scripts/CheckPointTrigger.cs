using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPointTrigger : MonoBehaviour
{
    GameObject NextCheckPoint;
    // Start is called before the first frame update
    void Start()
    {
        NextCheckPoint = GetComponentInParent<CheckPointManager>().GetNextCheckPoint(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter(Collider other)
    {
        if (NextCheckPoint == null) return;

        if(StrollObjectManager.Get().IsMyCharacter(other.gameObject))
        {
            StrollObjectManager.Get().Spawn(40, NextCheckPoint.transform.position);
        }
    }
}
