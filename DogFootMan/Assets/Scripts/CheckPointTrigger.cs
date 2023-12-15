using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPointTrigger : MonoBehaviour
{
    int CurrentIndex;
    // Start is called before the first frame update
    void Start()
    {
        CurrentIndex = GetComponentInParent<CheckPointManager>().GetIndex(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter(Collider other)
    {
        if (StrollObjectManager.Get().IsMyCharacter(other.gameObject))
        {
            StrollObjectManager.Get().Spawn(40, CurrentIndex + 1);
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
