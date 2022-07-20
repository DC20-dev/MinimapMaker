using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WarningMgr : MonoBehaviour
{
    [SerializeField]
    private GameObject Warning;
    [SerializeField]
    private Toggle dayNightToggle;

    public void DisplayWarning()
    {
        if(dayNightToggle.isOn)
            Warning.SetActive(true);
        else
            Warning.SetActive(false);
    }
}
