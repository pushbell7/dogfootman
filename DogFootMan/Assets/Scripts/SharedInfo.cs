using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SharedInfo
{
    static SharedInfo instance;
    public enum ESceneState
    {
        Quick,
        Workplace,
        Stroll,
        Max
    }
    public ESceneState NextState
    {
        get;
        private set;
    }
    public Ability MyAbility
    {
        get;
        private set;
    }

    public int Money
    {
        get;
        set;
    }
    const int DEFAULT_BUDGET = 1000;

    SharedInfo()
    {
        NextState = ESceneState.Quick;

        MyAbility = AbilityContainer.DefaultAbilityFactory.Make(ObjectManager.ObjectType.Human);
        Money = DEFAULT_BUDGET;
    }

    public static SharedInfo Get()
    {
        if(instance == null)
        {
            instance = new SharedInfo();
        }
        return instance;
    }

    public void MoveNextScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene((int)(NextState) + 1); // Maintain scene is zero.
        NextState = (ESceneState)(((int)(NextState) + 1) % (int)ESceneState.Max);
    }
    public void MoveToMaintainScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(0); // Maintain scene is zero.

        // end of a cycle
        if (NextState == ESceneState.Quick)
        {

            if (MyAbility.Life < 5)
            {
                MyAbility.Life++;
            }

            Money += 1000;
        }
    }
}
