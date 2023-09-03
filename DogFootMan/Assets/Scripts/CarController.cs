using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{
    Rigidbody RigidBody;
    ObjectManager ObjectManagerRef;
    GameObject CurrentRoad;
    Vector3 DirectionOfMovement;
    public int LaneToUse;
    public int CurrentLane;

    Vector3 DestinationInLane;

    bool bIsWaiting;

    // Start is called before the first frame update
    void Start()
    {
        RigidBody = GetComponent<Rigidbody>();

        ObjectManagerRef = GameObject.Find("ObjectManager").GetComponent<ObjectManager>();
        CurrentRoad = ObjectManagerRef.FindRoadOn(gameObject.transform.position);
        if(CurrentRoad == null)
        {
            Debug.Log(string.Format("error {0}", gameObject.transform.position));
        }
        SetReadyToRunOnCurrentRoad();
    }

    void SetReadyToRunOnCurrentRoad()
    {
        bool bIsForward = IsForwardDirectionInRotatedRectangle();

        DirectionOfMovement = CurrentRoad.transform.forward * (bIsForward ? 1 : -1);

        // Decide which lane this is going to use
        var roadInfo = CurrentRoad.GetComponent<RoadInfo>();
        int maxCount = bIsForward ? roadInfo.ForwardLaneCount : roadInfo.BackwardLaneCount;
        LaneToUse = Random.Range(1, maxCount + 1) * (bIsForward ? 1 : -1);

        DestinationInLane = roadInfo.GetDestinationOfLane(LaneToUse);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        var ray = new Ray(transform.position, DirectionOfMovement);

        const float SafeDistance = 15.0f;
        Debug.DrawRay(transform.position, ray.direction * SafeDistance, Color.red);
        Debug.DrawLine(transform.position, DestinationInLane, Color.green);
        RaycastHit hitResult;
        Physics.Raycast(ray, out hitResult, SafeDistance);
        if (bIsWaiting || (hitResult.collider && hitResult.collider.tag == "Obstacles"))
        {
            Brake();
            return;
        }
        else
        {
            Accelerate();
        }

        var newRoad = ObjectManagerRef.FindRoadOn(gameObject.transform.position);
        if(newRoad && newRoad != CurrentRoad)
        {
            CurrentRoad = newRoad;
        }

        if((transform.position - DestinationInLane).magnitude < 1.0f)
        {
            SetReadyToRunOnCurrentRoad();
        }

        MakeHovering();
    }

    // to prevent this object from becoming stuck to the road
    void MakeHovering()
    {
        var position = transform.position;
        position.y = CurrentRoad.transform.position.y + 0.5f;
        transform.position = position;
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

        CurrentLane = CurrentRoad.GetComponent<RoadInfo>().GetTheNumberOfLane(transform.position);
        RigidBody.AddForce((DestinationInLane - transform.position).normalized * Time.deltaTime * Power, ForceMode.Acceleration);

        const float MaxSpeed = 10f;
        RigidBody.velocity = Vector3.ClampMagnitude(RigidBody.velocity, MaxSpeed);
    }

    void Brake()
    {
        if (RigidBody.velocity.magnitude > 100.0f)
        {
            const float Power = 20000f;
            RigidBody.AddForce(-RigidBody.velocity.normalized * Time.deltaTime * Power, ForceMode.Acceleration);
        }
    }

    public void SetWait(bool bInWait)
    {
        bIsWaiting = bInWait;
    }
}
