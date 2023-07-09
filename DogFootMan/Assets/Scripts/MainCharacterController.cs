using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCharacterController : MonoBehaviour
{
    private Rigidbody rigidBody;
    // Start is called before the first frame update
    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
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
        const float Power = 1000f; // it should be managed in MyCharacter ability container
        rigidBody.AddForce(new Vector3(horizontal, 0, vertical).normalized * Time.deltaTime * Power);

        const float MaxSpeed = 10f;
        rigidBody.velocity = Vector3.ClampMagnitude(rigidBody.velocity, MaxSpeed);

    }
}
