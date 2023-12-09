using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPointManager : MonoBehaviour
{
    List<GameObject> CheckPoints;
    // Start is called before the first frame update
    void Start()
    {
        CheckPoints = new List<GameObject>();
        for (int i = 0; i < transform.childCount; ++i)
        {
            CheckPoints.Add(transform.GetChild(i).gameObject);
        }
    }

    public GameObject GetNextCheckPoint(GameObject currentCheckPoint)
    {
        for(int i = 0; i < transform.childCount; ++i)
        {
            if (transform.GetChild(i).gameObject == currentCheckPoint)
            {
                if (i == transform.childCount - 1)
                {
                    break;
                }
                return transform.GetChild(i + 1).gameObject;
            }
        }
        return null;
    }
}
