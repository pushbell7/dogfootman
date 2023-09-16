using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{
    Rigidbody RigidBody;
    GameObject CurrentRoad;
    Vector3 DirectionOfMovement;
    public int LaneToUse;
    public int CurrentLane;

    Vector3 DestinationInLane;

    bool bIsWaiting;
    bool bIsUnderTheControlTraffic = false;

    // Start is called before the first frame update
    void Start()
    {
        RigidBody = GetComponent<Rigidbody>();

        CurrentRoad = ObjectManager.Get().FindRoadOn(gameObject.transform.position);
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

        if(bIsUnderTheControlTraffic)
        {
            if (bIsWaiting)
                Brake();
            else
                Accelerate();
            return;
        }


        RaycastHit hitResult;
        Physics.Raycast(ray, out hitResult, SafeDistance);
        if ((hitResult.collider && hitResult.collider.tag == "Obstacles"))
        {
            Brake();
            return;
        }
        else
        {
            Accelerate();
        }

        var newRoad = ObjectManager.Get().FindRoadOn(gameObject.transform.position);
        if(newRoad && newRoad != CurrentRoad)
        {
            CurrentRoad = newRoad;
        }

        if((transform.position - DestinationInLane).magnitude < 1.0f)
        {
            SetReadyToRunOnCurrentRoad();
        }

    }

    public void SetTraffic(bool bIsSet)
    {
        bIsUnderTheControlTraffic = bIsSet;
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

    public void SetDestination(Vector3 destination)
    {
        DestinationInLane = destination;
    }
}
