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

    public int GetIndex(GameObject obj)
    {
        for (int i = 0; i < CheckPoints.Count; ++i)
        {
            if(CheckPoints[i] == obj)
            {
                return i;
            }
        }
        return -1;
    }

    public Vector3 GetPositionFrom(int index)
    {
        if (index < 0 || index >= CheckPoints.Count) index = 0;

        return CheckPoints[index].transform.position;
    }

    public Vector3 GetDestinationWithRandomRange(int index, bool bIsForward)
    {
        if (index < 0 || index >= CheckPoints.Count) index = 0;

        GameObject basePositionObject = CheckPoints[index];
        Vector3 basePosition = basePositionObject.transform.position;
        return basePosition - (bIsForward ? 1 : -1) * basePositionObject.transform.forward * Random.Range(0, basePositionObject.transform.localScale.z * 0.3f);

    }
}
