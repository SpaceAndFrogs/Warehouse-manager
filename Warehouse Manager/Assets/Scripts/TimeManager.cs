using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class TimeManager : MonoBehaviour
{
    public static TimeManager instance;
    [SerializeField]
    float oneHourInSeconds;
    [SerializeField]
    TimeUi timeUi;

    void Awake()
    {
        MakeInstance();
    }

    void Start()
    {
        AddListeners();
    }

    void MakeInstance()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public float GetOneHour()
    {
        return oneHourInSeconds;
    }

    public float GetOneDay()
    {
        return oneHourInSeconds*24;
    }

    public float GetOneMounth()
    {
        return oneHourInSeconds*720;
    }

    void AddListeners()
    {
        timeUi.stopTimeButton.onClick.AddListener(() => ChangeTimeScale(0));
        timeUi.slowDownTimeButton.onClick.AddListener(() => ChangeTimeScale(-0.25f));
        timeUi.speedUpTimeButton.onClick.AddListener(() => ChangeTimeScale(0.25f));
    }

    public void ChangeTimeScale(float timeScale)
    {
        if(timeScale != 0)
        {
            Time.timeScale += timeScale;
        }
        else
        {
            Time.timeScale = timeScale;
        }
        
        if(Time.timeScale < 0)
        {
            Time.timeScale = 0;
        }
    }

    [System.Serializable]
    public class TimeUi
    {
        public Button stopTimeButton;
        public Button speedUpTimeButton;
        public Button slowDownTimeButton;
    }
}
