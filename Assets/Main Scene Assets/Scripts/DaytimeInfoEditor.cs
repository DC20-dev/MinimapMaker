using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class DaytimeInfoEditor : MonoBehaviour
{
    public DaytimeSwitch DaylightInfosToManage;
    public Light Light;
    public bool AddThisLight;

    private void Update()
    {
        if (AddThisLight)
        {
            AddThisLight = false;
            AddLight();
        }
    }

    void AddLight()
    {
        DaylightInfo newField  = new DaylightInfo();
        newField.LightColor = Light.color;
        newField.LightIntensity = Light.intensity;
        newField.SunRotation = Light.transform.rotation.eulerAngles;

        DaylightInfosToManage.DaylightInfos.Add(newField);
    }

}
