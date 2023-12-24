using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StrollHumanController : MonoBehaviour
{
    enum EState
    { 
        Idle,
        Move,
        Run,
        Max
    }

    EState State;
    Vector3 TargetMovingPosition;
    Rigidbody RigidBody;
    AbilityContainer MyAbility;
    CheckPointManager CheckPointManagerRef;
    int SpawnedCheckPointIndex;
    bool bIsForward;

    // Start is called before the first frame update
    void Start()
    {
        State = EState.Idle;
        RigidBody = GetComponent<Rigidbody>();
        MyAbility = GetComponent<AbilityContainer>();
        CheckPointManagerRef = GameObject.Find("CheckPoints").GetComponent<CheckPointManager>();
        bIsForward = Random.Range(0, 3) > 0;

        MakeTargetPosition(SpawnedCheckPointIndex);
        StartCoroutine(SetState());
    }

    IEnumerator SetState()
    {
        while (true)
        {
            SetState(MakeState());
            yield return new WaitForSeconds(10.0f);
        }
    }

    static readonly int[] WEIGHT_OF_STATE = { 1, 7, 2 };
    EState MakeState()
    {
        int summary = 0;
        foreach(int num in WEIGHT_OF_STATE) { summary += num; }

        int select = Random.Range(0, summary);
        int resultIndex = 0;
        foreach(int num in WEIGHT_OF_STATE)
        {
            if (select < num) break;
            select -= num;
            resultIndex++;
        }

        return (EState)resultIndex;
    }

    void SetState(EState state)
    {
        State = state;
    }

    // Update is called once per frame
    void Update()
    {
        Act();
    }

    void Act()
    {
        switch (State)
        {
            case EState.Idle:
                break;
            case EState.Move:
                Move(false);
                break;
            case EState.Run:
                Move(true);
                break;
        }
    }

    void Move(bool bIsBoosted)
    {
        MyAbility.SetBoostMode(bIsBoosted);

        var forceDirection = (TargetMovingPosition - transform.position).normalized;
        RigidBody.AddForce(forceDirection * Time.deltaTime * MyAbility.GetPower(), ForceMode.Acceleration);

        RigidBody.velocity = Vector3.ClampMagnitude(RigidBody.velocity, MyAbility.GetMaxSpeed());
    }

    public void MakeTargetPosition(int currentCheckPointIndex)
    {
        if (CheckPointManagerRef == null) return; // when it was spawned on trigger

        if(IsFInishing(currentCheckPointIndex))
        {
            StrollObjectManager.Get().RemoveHuman(gameObject);
            return;
        }
        TargetMovingPosition = CheckPointManagerRef.GetDestinationWithRandomRange(GetNextCheckPointIndex(currentCheckPointIndex), bIsForward);

        Debug.DrawLine(transform.position, TargetMovingPosition, Color.green, 60.0f);
    }

    bool IsFInishing(int currentIndex)
    {
        return currentIndex == -1 || GetNextCheckPointIndex(currentIndex) < 0 || GetNextCheckPointIndex(currentIndex) >= CheckPointManagerRef.GetLastIndex();
    }

    int GetNextCheckPointIndex(int currentIndex)
    {
        return currentIndex + (bIsForward ? 1 : -1);
    }
    public void SetSpawnedCheckPointIndex(int index)
    {
        SpawnedCheckPointIndex = index;
    }
}
