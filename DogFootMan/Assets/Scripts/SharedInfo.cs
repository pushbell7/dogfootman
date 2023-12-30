using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SharedInfo : MonoBehaviour
{
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

    // Start is called before the first frame update
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        NextState = ESceneState.Quick;

        MyAbility = AbilityContainer.DefaultAbilityFactory.Make(ObjectManager.ObjectType.Human);
    }

    public static SharedInfo Get()
    {
        return GameObject.Find("SharedGameObject").GetComponent<SharedInfo>();
    }

    public void MoveNextScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene((int)(NextState) + 1); // Maintain scene is zero.
        NextState = (ESceneState)(((int)(NextState) + 1) % (int)ESceneState.Max);        
    }
    public void MoveToMaintainScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(0); // Maintain scene is zero.
    }
}
