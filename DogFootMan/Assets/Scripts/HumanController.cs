using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanController : MonoBehaviour
{
    enum State
    { 
        WAIT,
        FIND_DESTINATION,
        MOVE_TO,
    }
    State CurrentState;
    float TargetTimeToWait;
    Vector3 ObjectivePosition;
    Rigidbody RigidBody;

    float MinTimeToWait = 0.3f;
    float MaxTimeToWait = 2.0f;
    float MaxDistanceToMove = 30.0f;


    // Start is called before the first frame update
    void Start()
    {
        RigidBody = GetComponent<Rigidbody>();
        SetWait();
    }

    // Update is called once per frame
    void Update()
    {
        switch(CurrentState)
        {
            case State.WAIT:
                if(Time.time >= TargetTimeToWait)
                {
                    SetFindDestination();
                }
                break;
            case State.FIND_DESTINATION:
                if(IsObjectivePositionReachable())
                {
                    SetMoveTo();
                }
                else
                {
                    SetFindDestination();
                }
                break;
            case State.MOVE_TO:
                if(IsReached())
                {
                    SetWait();
                }
                else
                {
                    MoveTo();
                }
                break;
            default:
                break;
        }
    }

    void SetWait()
    {
        CurrentState = State.WAIT;
        TargetTimeToWait = Time.time + Random.Range(MinTimeToWait, MaxTimeToWait);
    }

    void SetFindDestination()
    {
        CurrentState = State.FIND_DESTINATION;
        ObjectivePosition = MakeRandomPosition();
    }

    bool IsObjectivePositionReachable()
    {
        return ObjectManager.Get().IsOnRoad(ObjectivePosition) == false;
    }

    void SetMoveTo()
    {
        CurrentState = State.MOVE_TO;
    }

    void MoveTo()
    {
        var ray = new Ray(transform.position, ObjectivePosition - transform.position);
        const float SafeDistance = 3.0f;
        RaycastHit hitResult;
        Physics.SphereCast(ray, 0.5f, out hitResult, SafeDistance);
        Debug.DrawRay(transform.position, ObjectivePosition - transform.position, Color.black);
        if ((hitResult.collider && hitResult.collider.tag == "Obstacles"))
        {
            SetFindDestination();
            return;
        }

        const float POWER = 1000.0f;
        var forceDirection = (ObjectivePosition - transform.position).normalized;
        RigidBody.AddForce(forceDirection * Time.deltaTime * POWER, ForceMode.Acceleration);

        const float MaxSpeed = 5f;
        RigidBody.velocity = Vector3.ClampMagnitude(RigidBody.velocity, MaxSpeed);
    }

    bool IsReached()
    {
        return (ObjectivePosition - transform.position).magnitude < 1.0f;
    }
    Vector3 MakeRandomPosition()
    {
        var result = Quaternion.Euler(0, Random.Range(0, 360), 0) * transform.forward;
        return transform.position + result * Random.Range(5, MaxDistanceToMove);
    }
}
