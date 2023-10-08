using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCharacterController : MonoBehaviour
{
    private Rigidbody rigidBody;
    private AbilityContainer MyAbility;
    // Start is called before the first frame update
    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
        MyAbility = GetComponent<AbilityContainer>();
    }

    // Update is called once per frame
    void Update()
    {
        Move();
    }

    private void Move()
    {
        float vertical = Input.GetAxis("Vertical");
        float horizontal = Input.GetAxis("Horizontal");
        rigidBody.AddForce(new Vector3(horizontal, 0, vertical).normalized * Time.deltaTime * MyAbility.GetPower());

        rigidBody.velocity = Vector3.ClampMagnitude(rigidBody.velocity, MyAbility.GetMaxSpeed());

    }

    public void TakeVehicle(GameObject vehicle)
    {
        // get ability from vehicle
        // transform apperance
        MyAbility.SetMass(5);
        MyAbility.SetPower(10000);
        MyAbility.SetMaxSpeed(10);
    }
}
