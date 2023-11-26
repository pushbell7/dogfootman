using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StrollMainCharacterController : MonoBehaviour
{
    private Rigidbody RigidBody;
    private AbilityContainer MyAbility;
    float CurrentRotation;
    // Start is called before the first frame update
    void Start()
    {
        RigidBody = GetComponent<Rigidbody>();
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
        CurrentRotation += horizontal * Time.deltaTime * 100.0f;
        CurrentRotation %= 360;
        RigidBody.AddForce(transform.forward * vertical * Time.deltaTime * MyAbility.GetPower());
        RigidBody.rotation = (Quaternion.Euler(new(0, CurrentRotation, 0)));
        RigidBody.velocity = Vector3.ClampMagnitude(RigidBody.velocity, MyAbility.GetMaxSpeed());

    }
}
