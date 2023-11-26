using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempAdjustCamera : MonoBehaviour
{
    GameObject MainCharacter;
    // Start is called before the first frame update
    void Start()
    {
        MainCharacter = GameObject.Find("MainCharacter");
    }

    // Update is called once per frame
    void Update()
    {
        if(MainCharacter)
        {
            transform.LookAt(MainCharacter.transform);
        }
    }
}
