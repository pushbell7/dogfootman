using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class StrollUIController : MonoBehaviour
{
    Label TimeLabel;
    float StartTime;

    Label HPLabel;
    ProgressBar StaminaProgress;

    GameObject MyCharacterRef;

    // Start is called before the first frame update
    void Start()
    {
        var uiDocument = GetComponent<UIDocument>();

        TimeLabel = uiDocument.rootVisualElement.Q<Label>("TimerTime");
        HPLabel = uiDocument.rootVisualElement.Q<Label>("HPValue");
        StaminaProgress = uiDocument.rootVisualElement.Q<ProgressBar>("Stamina");

        MyCharacterRef = GameObject.Find("MainCharacter");
    }

    private void OnEnable()
    {
        const float DefaultTimeLimit = 5 * 60;
        StartTime = Time.time + DefaultTimeLimit;
    }

    // Update is called once per frame
    void Update()
    {
        if (GetRemainingTime() < 0)
        {
            TimeLabel.text = string.Format(System.TimeSpan.FromSeconds(GetRemainingTime()).ToString(@"\-mm\:ss"));
        }
        else
        {
            TimeLabel.text = string.Format(System.TimeSpan.FromSeconds(GetRemainingTime()).ToString(@"mm\:ss"));
        }

        var MyAbility = MyCharacterRef.GetComponent<AbilityContainer>();
        if (MyAbility != null)
        {
            HPLabel.text = MyAbility.GetLife().ToString();

            StaminaProgress.highValue = MyAbility.GetMaxStamina();
            StaminaProgress.value = MyAbility.GetCurrentStamina();
        }
    }

    float GetRemainingTime()
    {
        return StartTime - Time.time;
    }
}
