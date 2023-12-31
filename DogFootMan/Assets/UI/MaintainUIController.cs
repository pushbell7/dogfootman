using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class MaintainUIController : MonoBehaviour
{
    VisualElement MyStatus;
    VisualElement NextObjective;

    private void Awake()
    {
        Init();
    }

    void Init()
    {
        var rootElement = GetComponent<UIDocument>().rootVisualElement;
        MyStatus = rootElement.Q<VisualElement>("Status");
        NextObjective = rootElement.Q<VisualElement>("MissionBrief");
        rootElement.Q<Button>("StartButton").clicked += () =>
        {
            SharedInfo.Get().MoveNextScene();
        };

        rootElement.Q<Button>("IncreaseSpeed").clicked += () =>
        {
            const int price = 500;
            if (SharedInfo.Get().Money >= price)
            {
                SharedInfo.Get().Money -= price;

                SharedInfo.Get().MyAbility.MaxSpeed += 2;

                RefreshStatus();
            }
        };
        rootElement.Q<Button>("IncreaseHp").clicked += () =>
        {
            const int price = 500;
            if (SharedInfo.Get().Money >= price)
            {
                SharedInfo.Get().Money -= price;

                SharedInfo.Get().MyAbility.Life += 1;
                RefreshStatus();
            }
        };
        rootElement.Q<Button>("IncreaseStamina").clicked += () =>
        {
            const int price = 500;
            if (SharedInfo.Get().Money >= price)
            {
                SharedInfo.Get().Money -= price;

                SharedInfo.Get().MyAbility.MaxStamina += 1000;
                SharedInfo.Get().MyAbility.CurrentStamina += 1000;
                RefreshStatus();
            }
        };
    }

    private void OnEnable()
    {
        MyStatus.Q<Label>("StatusText").text = MakeStatus();
        NextObjective.Q<Label>("BriefText").text = MakeBrief();
    }

    void RefreshStatus()
    {
        MyStatus.Q<Label>("StatusText").text = MakeStatus();
    }

    string MakeStatus()
    {
        Ability ability = SharedInfo.Get().MyAbility;
        if (ability == null) return "";

        int Money = SharedInfo.Get().Money;
        return string.Format("Life : {0} \nMax Speed : {1}\nMax Stamina : {2}\nPower : {3}\nMoney : {4}"
            , ability.Life, ability.MaxSpeed, ability.MaxStamina, ability.Power, Money);
    }

    string MakeBrief()
    {
        switch (SharedInfo.Get().NextState)
        {
            case SharedInfo.ESceneState.Quick:
                return "you sould go to your work space.";
            case SharedInfo.ESceneState.Workplace:
                return "you should work on your position.";
            case SharedInfo.ESceneState.Stroll:
                return "you need to stroll from work space to jeongja station.";
        }
        return "Do well.";
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
