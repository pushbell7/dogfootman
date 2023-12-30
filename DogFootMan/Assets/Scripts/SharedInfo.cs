using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SharedInfo : MonoBehaviour
{
    enum ESceneState
    {
        Maintain,
        Quick,
        Workplace,
        Stroll,
    }
    ESceneState CurrentState;
    public Ability MyAbility
    {
        get;
        private set;
    }

    // Start is called before the first frame update
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        CurrentState = ESceneState.Maintain;

        MyAbility = AbilityContainer.DefaultAbilityFactory.Make(ObjectManager.ObjectType.Human);
    }

    public static SharedInfo Get()
    {
        return GameObject.Find("SharedGameObject").GetComponent<SharedInfo>();
    }
}
