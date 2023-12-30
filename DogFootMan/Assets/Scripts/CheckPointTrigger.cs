using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPointTrigger : MonoBehaviour
{
    int CurrentIndex;
    bool bIsLastCheckPoint;
    // Start is called before the first frame update
    void Start()
    {
        CurrentIndex = GetComponentInParent<CheckPointManager>().GetIndex(gameObject);
        bIsLastCheckPoint = GetComponentInParent<CheckPointManager>().GetLastIndex() == CurrentIndex;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter(Collider other)
    {
        if (StrollObjectManager.Get().IsMyCharacter(other.gameObject))
        {
            if (bIsLastCheckPoint)
            {
                SharedInfo.Get().MoveToMaintainScene();
            }
            else
            {
                StrollObjectManager.Get().Spawn(20, CurrentIndex + 1);
            }
        }
        else
        {
            var strollHuman = other.GetComponent<StrollHumanController>();
            if(strollHuman)
            {
                strollHuman.MakeTargetPosition(CurrentIndex);
            }
        }
    }
}
