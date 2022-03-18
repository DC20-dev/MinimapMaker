using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DaytimeSwitch))]
[ExecuteInEditMode]
public class DaytimeSwitcherCustomInspector : Editor
{
    DaytimeSwitch daytimeSwitch;
    SerializedProperty IsRandom, IsFromArray, DayNightCycle;

    List<string> paramsToModify, daynightCycleParams;
    private void OnEnable()
    {
        daytimeSwitch = (DaytimeSwitch)target;

        paramsToModify = new List<string> 
        {
            "DaylightInfos",
            "daylightMin", 
            "daylightMax",
            //daynightcycle
            "MaxLightIntensityDay",
            "MaxLightIntensityNight",
            "MinLightIntensity",
            "MinLightRotation",
            "MaxLightRotation",
            "SunlightIntensityCurve",
            "MoonlightIntensityCurve",
            "SunlightColor",
            "MoonlightColor",
            "TransitionColor",
            "ColorBlendCurve",
            "DayDuration"
        };

        daynightCycleParams = new List<string> 
        {
            "MaxLightIntensityDay",
            "MaxLightIntensityNight",
            "MinLightIntensity",
            "MinLightRotation",
            "MaxLightRotation",
            "SunlightIntensityCurve",
            "MoonlightIntensityCurve",
            "SunlightColor",
            "MoonlightColor",
            "TransitionColor",
            "ColorBlendCurve",
            "DayDuration"
        };

        DayNightCycle = serializedObject.FindProperty("DayNightCycle");
        IsRandom = serializedObject.FindProperty("isRandom");
        IsFromArray = serializedObject.FindProperty("isFromArray");
    }

    public override void OnInspectorGUI()
    {
        if (IsFromArray.boolValue && !DayNightCycle.boolValue)
        {
            if (paramsToModify.Contains("DaylightInfos"))
                paramsToModify.Remove("DaylightInfos");
        }
        else
        {
            if (!paramsToModify.Contains("DaylightInfos"))
                paramsToModify.Add("DaylightInfos");
        }

        if (IsRandom.boolValue && !DayNightCycle.boolValue)
        {
            if (paramsToModify.Contains("daylightMin"))
                paramsToModify.Remove("daylightMin");

            if (paramsToModify.Contains("daylightMax"))
                paramsToModify.Remove("daylightMax");
        }
        else
        {
            if (!paramsToModify.Contains("daylightMin"))
                paramsToModify.Add("daylightMin");

            if (!paramsToModify.Contains("daylightMax"))
                paramsToModify.Add("daylightMax");
        }

        serializedObject.Update();

        DrawPropertiesExcluding(serializedObject, paramsToModify.ToArray());

        if (DayNightCycle.boolValue)
        {
            DrawDayNightCycleOptions();
        }

        serializedObject.ApplyModifiedProperties();
    }

    void DrawDayNightCycleOptions()
    {
        EditorGUILayout.Separator();
        EditorGUI.indentLevel++;
        serializedObject.FindProperty(daynightCycleParams[0]).floatValue =
             EditorGUILayout.FloatField(daynightCycleParams[0], daytimeSwitch.MaxLightIntensityDay);
        serializedObject.FindProperty(daynightCycleParams[1]).floatValue =
            EditorGUILayout.FloatField(daynightCycleParams[1], daytimeSwitch.MaxLightIntensityNight);
        serializedObject.FindProperty(daynightCycleParams[2]).floatValue =
            EditorGUILayout.FloatField(daynightCycleParams[2], daytimeSwitch.MinLightIntensity);
        serializedObject.FindProperty(daynightCycleParams[3]).vector3Value =
            EditorGUILayout.Vector3Field(daynightCycleParams[3], daytimeSwitch.MinLightRotation);
        serializedObject.FindProperty(daynightCycleParams[4]).vector3Value =
            EditorGUILayout.Vector3Field(daynightCycleParams[4], daytimeSwitch.MaxLightRotation);
        serializedObject.FindProperty(daynightCycleParams[5]).animationCurveValue =
            EditorGUILayout.CurveField(daynightCycleParams[5], daytimeSwitch.SunlightIntensityCurve);
        serializedObject.FindProperty(daynightCycleParams[6]).animationCurveValue =
            EditorGUILayout.CurveField(daynightCycleParams[6], daytimeSwitch.MoonlightIntensityCurve);
        serializedObject.FindProperty(daynightCycleParams[7]).colorValue =
            EditorGUILayout.ColorField(daynightCycleParams[7], daytimeSwitch.SunlightColor);
        serializedObject.FindProperty(daynightCycleParams[8]).colorValue =
            EditorGUILayout.ColorField(daynightCycleParams[8], daytimeSwitch.MoonlightColor);
        serializedObject.FindProperty(daynightCycleParams[9]).colorValue =
            EditorGUILayout.ColorField(daynightCycleParams[9], daytimeSwitch.TransitionColor);
        serializedObject.FindProperty(daynightCycleParams[10]).animationCurveValue =
            EditorGUILayout.CurveField(daynightCycleParams[10], daytimeSwitch.ColorBlendCurve);
        serializedObject.FindProperty(daynightCycleParams[11]).floatValue =
            EditorGUILayout.FloatField(daynightCycleParams[11], daytimeSwitch.DayDuration);
    }
}
