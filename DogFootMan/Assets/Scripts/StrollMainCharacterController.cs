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

        CurrentRotation = transform.rotation.eulerAngles.y;
    }

    // Update is called once per frame
    void Update()
    {
        Move();
        CheckOnPath();
    }

    private void Move()
    {
        if (MyAbility.GetCurrentStamina() > 0)
        {
            if (Input.GetButton("Fire1"))
            {
                MyAbility.AdjustStamina(Time.deltaTime * -20);
                MyAbility.SetBoostMode(true);
            }
            else
            {
                MyAbility.AdjustStamina(Time.deltaTime * 10);
                MyAbility.SetBoostMode(false);
            }
        }
        else
        {
            MyAbility.AdjustStamina(Time.deltaTime * 10);
            MyAbility.SetBoostMode(false);
        }

        {
            float vertical = Input.GetAxis("Vertical");
            Vector3 acceleration = transform.forward * vertical * Time.deltaTime * MyAbility.GetPower();
            float maxSpeed = MyAbility.GetMaxSpeed();

            RigidBody.AddForce(acceleration);
            RigidBody.velocity = Vector3.ClampMagnitude(RigidBody.velocity, maxSpeed);
        }
        {
            float horizontal = Input.GetAxis("Horizontal");
            CurrentRotation += horizontal * Time.deltaTime * 100.0f;
            CurrentRotation %= 360;
            RigidBody.rotation = (Quaternion.Euler(new(0, CurrentRotation, 0)));
        }

    }

    void CheckOnPath()
    {
        if(StrollObjectManager.IsOnPath(gameObject) == false)
        {
            RigidBody.AddForce(-transform.forward * 1000.0f);
            Debug.Log("Make sure to follow the walking rule.");
        }
    }
}
