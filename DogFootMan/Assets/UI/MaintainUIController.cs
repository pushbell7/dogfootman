using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class MaintainUIController : MonoBehaviour
{
    VisualElement MyStatus;
    VisualElement NextObjective;
    // Start is called before the first frame update
    void Awake()
    {
        var rootElement = GetComponent<UIDocument>().rootVisualElement;
        MyStatus = rootElement.Q<VisualElement>("Status");
        NextObjective = rootElement.Q<VisualElement>("MissionBrief");
        rootElement.Q<Button>("StartButton").clicked += () =>
        {
            SharedInfo.Get().MoveNextScene();
        };
    }

    private void OnEnable()
    {
        MyStatus.Q<Label>("StatusText").text = MakeStatus(SharedInfo.Get().MyAbility);
        NextObjective.Q<Label>("BriefText").text = MakeBrief();
    }

    string MakeStatus(Ability ability)
    {
        return string.Format("Life : {0} \nMax Speed : {1}\nMax Stamina : {2}\nPower : {3}"
            , ability.Life, ability.MaxSpeed, ability.MaxStamina, ability.Power);
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
