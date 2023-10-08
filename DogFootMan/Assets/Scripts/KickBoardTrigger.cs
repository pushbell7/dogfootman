using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KickBoardTrigger : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if(ObjectManager.Get().IsMyCharacter(other.gameObject))
        {
            var characterController = other.gameObject.GetComponent<MainCharacterController>();
            characterController.TakeVehicle(gameObject);
            Destroy(gameObject);
        }
    }

}
