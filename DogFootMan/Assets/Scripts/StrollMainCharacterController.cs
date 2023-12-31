using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StrollMainCharacterController : MonoBehaviour
{
    private Rigidbody RigidBody;
    private AbilityContainer MyAbility;
    float CurrentRotation;
    Vector3 LastPositionOnRoad;
    // Start is called before the first frame update
    void Start()
    {
        RigidBody = GetComponent<Rigidbody>();
        MyAbility = GetComponent<AbilityContainer>();

        CurrentRotation = transform.rotation.eulerAngles.y;
        LastPositionOnRoad = transform.position;
        //StartCoroutine(TestSetting());

        if(MyAbility.GetCurrentStamina() < MyAbility.GetMaxStamina())
        {
            MyAbility.AdjustStamina(MyAbility.GetMaxStamina() / 4);
        }
    }

    IEnumerator TestSetting()
    {
        yield return new WaitForSeconds(3);
        MyAbility.SetMaxSpeed(MyAbility.GetMaxSpeed() * 3);
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
            transform.position = LastPositionOnRoad;
            RigidBody.velocity = Vector3.zero;
            Debug.Log("Make sure to follow the walking rule.");
        }
        else
        {
            LastPositionOnRoad = transform.position;
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Obstacles"))
        {
            var direction = transform.position - collision.collider.transform.position;
            RigidBody.AddForce(direction * MyAbility.GetPower());
        }
    }
}
