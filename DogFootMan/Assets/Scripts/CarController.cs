using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{
    Rigidbody rigidBody;
    ObjectManager objectManagerRef;
    GameObject CurrentRoad;
    Vector3 DirectionOfMovement;

    // Start is called before the first frame update
    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();

        objectManagerRef = GameObject.Find("ObjectManager").GetComponent<ObjectManager>();
        CurrentRoad = objectManagerRef.FindRoadOn(gameObject.transform.position);
        if(CurrentRoad == null)
        {
            Debug.Log(string.Format("error {0}", gameObject.transform.position));
        }
        bool bIsForward = IsForwardDirectionInRotatedRectangle();

        DirectionOfMovement = CurrentRoad.transform.forward * (bIsForward ? 1 : -1);

    }

    // Update is called once per frame
    void Update()
    {
        var ray = new Ray(transform.position, DirectionOfMovement);

        const float SafeDistance = 15.0f;
        if(Physics.Raycast(ray, SafeDistance))
        {
            Debug.DrawRay(transform.position, DirectionOfMovement, Color.red, 100.0f);
            Brake();
        }
        else
        {
            Accelerate();
        }
    }

    bool IsForwardDirectionInRotatedRectangle()
    {
        var boxCollider = CurrentRoad.GetComponent<BoxCollider>();
        var center3D = CurrentRoad.transform.position + boxCollider.center;

        // Translate the point to the rectangle's local coordinate space
        Vector2 point = new Vector2(gameObject.transform.position.x, gameObject.transform.position.z);
        Vector2 center = new Vector2(center3D.x, center3D.z);
        Vector2 translatedPoint = point - center;

        // Apply the inverse rotation of the rectangle to the translated point
        Quaternion inverseRotation = Quaternion.Euler(0f, 0f, CurrentRoad.transform.rotation.eulerAngles.y);
        Vector2 rotatedPoint = inverseRotation * translatedPoint;

        return rotatedPoint.x > 0;
    }

    void Accelerate()
    {
        const float Power = 10000f; // it should be managed in MyCharacter ability container
        rigidBody.AddForce(DirectionOfMovement * Time.deltaTime * Power, ForceMode.Acceleration);

        const float MaxSpeed = 10f;
        rigidBody.velocity = Vector3.ClampMagnitude(rigidBody.velocity, MaxSpeed);
    }

    void Brake()
    {
        if (rigidBody.velocity.magnitude > 100.0f)
        {
            const float Power = 20000f;
            rigidBody.AddForce(-rigidBody.velocity.normalized * Time.deltaTime * Power, ForceMode.Acceleration);
        }
    }
}
