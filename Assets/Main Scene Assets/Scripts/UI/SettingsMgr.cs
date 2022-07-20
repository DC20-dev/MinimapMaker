using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class SettingsMgr : MonoBehaviour
{
    [SerializeField]
    private DaytimeSwitch lightSettings;

    [SerializeField]
    private Toggle DayNightToggle;
    [SerializeField]
    private Toggle RandomLightToggle;


    public void UpdateDayNightCycle(bool value)
    {
        lightSettings.DayNightCycle = DayNightToggle.isOn;
    }

    public void UpdateRandomLight(bool value)
    {
        lightSettings.isRandom = RandomLightToggle.isOn;
    }
}
