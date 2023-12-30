using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCharacterController : MonoBehaviour
{
    private Rigidbody RigidBody;
    private AbilityContainer MyAbility;
    // Start is called before the first frame update
    void Start()
    {
        RigidBody = GetComponent<Rigidbody>();
        MyAbility = GetComponent<AbilityContainer>();
        MyAbility.OnDeathDelegator += OnDeathHandler;
    }

    static void OnDeathHandler(GameObject MyObject)
    {
        Debug.Log("Die.");
        SharedInfo.Get().MoveToMaintainScene();
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
        RigidBody.AddForce(new Vector3(horizontal, 0, vertical).normalized * Time.deltaTime * MyAbility.GetPower());

        RigidBody.velocity = Vector3.ClampMagnitude(RigidBody.velocity, MyAbility.GetMaxSpeed());

    }

    public void TakeVehicle(GameObject vehicle)
    {
        var ability = vehicle.GetComponent<AbilityContainer>();
        MyAbility.GetItem(ability);

        // transform apperance
    }

    public void OnCollisionEnter(Collision collision)
    {
        var type = ObjectManager.GetType(collision.gameObject);
        if (type == ObjectManager.ObjectType.Car)
        {
            const float POWER_TO_MAKE_ME_FLY = 100000.0f;
            RigidBody.AddForce(collision.transform.forward * POWER_TO_MAKE_ME_FLY, ForceMode.Force);

            // game over
            MyAbility.Kill();
        }
        else if (type == ObjectManager.ObjectType.Human)
        {
            // decrease hp
            const float POWER_TO_MAKE_ME_KNOCK_BACK = 5000.0f;
            RigidBody.AddForce(collision.transform.forward * POWER_TO_MAKE_ME_KNOCK_BACK, ForceMode.Force);
            MyAbility.DecreaseLife();
        }
    }
}
