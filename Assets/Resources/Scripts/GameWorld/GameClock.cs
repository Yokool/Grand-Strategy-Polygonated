using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameClock : MonoBehaviour
{
    [SerializeField]
    private float timeToOneHour;

    [SerializeField]
    private float currentTime = 0f;

    [SerializeField]
    private long year;
    [SerializeField]
    private byte month;
    [SerializeField]
    private byte week;
    [SerializeField]
    private byte day;
    [SerializeField]
    private byte hour;

    private List<IOnNextHour> onNextHours = new List<IOnNextHour>();
    private List<IOnNextDay> onNextDays = new List<IOnNextDay>();
    private List<IOnNextWeek> onNextWeeks = new List<IOnNextWeek>();
    private List<IOnNextMonth> onNextMonths = new List<IOnNextMonth>();
    private List<IOnNextYear> onNextYears = new List<IOnNextYear>();

    public void RegisterOnNextHour(IOnNextHour onNextHour)
    {
        onNextHours.Add(onNextHour);
    }

    public void UnRegisterOnNextHour(IOnNextHour onNextHour)
    {
        onNextHours.Remove(onNextHour);
    }

    public void RegisterOnNextDay(IOnNextDay onNextDay)
    {
        onNextDays.Add(onNextDay);
    }

    public void UnRegisterOnNextDay(IOnNextDay onNextDay)
    {
        onNextDays.Remove(onNextDay);
    }

    public void RegisterOnNextWeek(IOnNextWeek onNextWeek)
    {
        onNextWeeks.Add(onNextWeek);
    }

    public void UnRegisterOnNextWeek(IOnNextWeek onNextWeek)
    {
        onNextWeeks.Remove(onNextWeek);
    }

    public void RegisterOnNextMonth(IOnNextMonth onNextMonth)
    {
        onNextMonths.Add(onNextMonth);
    }

    public void UnRegisterOnNextMonth(IOnNextMonth onNextMonth)
    {
        onNextMonths.Remove(onNextMonth);
    }

    public void RegisterOnNextYear(IOnNextYear onNextYear)
    {
        onNextYears.Add(onNextYear);
    }

    public void UnRegisterOnNextYear(IOnNextYear onNextYear)
    {
        onNextYears.Remove(onNextYear);
    }

    private static GameClock instance;
    public static GameClock INSTANCE
    {
        get
        {
            return instance;
        }
    }

    private void Awake()
    {
        
        if(instance != null)
        {
            Debug.LogWarning("Tried to create a second GameClock.");
            Destroy(gameObject);
            return;
        }

        instance = this;


    }



    void Update()
    {

        currentTime += Time.deltaTime;

        if(currentTime > timeToOneHour)
        {
            OnNextHour();
        }


    }

    private void OnNextHour()
    {
        currentTime = 0;
        ++hour;

        if (hour > 23)
        {
            OnNextDay();
        }

    }

    private void OnNextWeek()
    {
        day = 0;
        ++week;

        if (day > 30)
        {
            OnNextMonth();
        }

    }

    private void OnNextDay()
    {
        ++day;
        hour = 0;

        if(day > 7)
        {
            OnNextWeek();
        }

    }

    private void OnNextMonth()
    {
        week = 0;
        ++month;

        if(month > 12)
        {
            OnNextYear();
        }

    }

    private void OnNextYear()
    {
        ++year;
        day = 0;
    }

    


}

public interface IOnNextHour
{
    void OnNextHour();
}

public interface IOnNextDay
{
    void OnNextDay();
}

public interface IOnNextWeek
{
    void OnNextWeek();
}

public interface IOnNextMonth
{
    void OnNextMonth();
}

public interface IOnNextYear
{
    void OnNextYear();
}