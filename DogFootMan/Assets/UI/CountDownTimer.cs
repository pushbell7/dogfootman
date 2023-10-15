using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CountDownTimer : MonoBehaviour
{
    Label TimeLabel;
    float StartTime;

    // Start is called before the first frame update
    void Start()
    {
        var uiDocument = GetComponent<UIDocument>();

        TimeLabel = uiDocument.rootVisualElement.Q<Label>("TimerTime");
    }

    private void OnEnable()
    {
        const float DefaultTimeLimit = 1 * 30;
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
    }

    float GetRemainingTime()
    {
        return StartTime - Time.time;
    }
}
