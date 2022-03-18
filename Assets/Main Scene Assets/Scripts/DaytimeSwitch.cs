using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class DaytimeSwitch : MonoBehaviour
{
    public Light Light;

    [Tooltip(
        "IF:\n" +
        "*IsRandom & IsFromArray the light will be randomized from presets\n\n" +
        "*!IsRandom & IsFromArray the light will be changed according to array order\n\n" +
        "*IsRandom & !IsFromArray the light will be randomized between DaylightMin and DaylightMax")]
    public bool isRandom = false, isFromArray = true;

    [Space]
    [Tooltip("If TRUE light will follow a day-night cycle, overriding every other settings")]
    public bool DayNightCycle = false;

    //LIGHTING PRESETS
    [Header("LIGHT PRESETS")]
    [Space]
    public List<DaylightInfo> DaylightInfos;

    int currentDayLightIndex = 0;
    float lerptime = 0.4f;

    //USE THEESE ONLY FOR RANDOM LIGHT GENERATION
    [Space]
    [Header("FOR RANDOM LIGHT ONLY")]
    [SerializeField]
    [Tooltip("Only used when random sunlight is requested\nUse this as \"darkest\" values")]
    DaylightInfo daylightMin;
    [SerializeField]
    [Tooltip("Only used when random sunlight is requested\nUse this as \"brightest\" values")]
    DaylightInfo daylightMax;

    //VALUES FOR DAY-NIGHT CYCLE
    [Space]
    [Header("DAY-NIGHT CYCLE PARAMETERS")]
    public float MaxLightIntensityDay;
    public float MaxLightIntensityNight;
    public float MinLightIntensity;
    public Vector3 MinLightRotation, MaxLightRotation;
    public AnimationCurve SunlightIntensityCurve, MoonlightIntensityCurve;
    [Tooltip("Use this value to give a slight tint only (ex. slight yellow for sun, bluish for moon)")]
    public Color SunlightColor, MoonlightColor;
    [Tooltip("This value is blended with SunlightColor or MoonlightColor to give dusk/dawn light")]
    public Color TransitionColor;
    [Tooltip("Curve for blending Sunset/Sunrise color with day/night color\n1-> sunset/sunrise, 0 -> day/night")]
    public AnimationCurve ColorBlendCurve;
    public float DayDuration;
    float timer = 0;
    DaylightInfo startDaylight;
    bool startCycle = true;

    DaylightInfo daylightInfo = new DaylightInfo();

    private void OnEnable()
    {
        MapGeneration.MapChangeEvent.AddListener(SwitchDaytime);
        startDaylight = new DaylightInfo();
        startDaylight.LightIntensity = MinLightIntensity;
        startDaylight.SunRotation = new Vector3(MinLightRotation.x, MinLightRotation.y, 0);
    }
    private void Update()
    {
        if (DayNightCycle)
        {
            if (startCycle)
            {
                SetLightSettings(startDaylight);
                timer = 0;
                startCycle = false;
            }
            UpdateDayNightCycle(); 
        }
    }
    void SwitchDaytime()
    {
        if (DayNightCycle)
            return;
        //using return makes DayNightCycle override every other possible setting
        //preparing startcycle for a new cycle start occurrence
        if (!startCycle) startCycle = true;

        if (isFromArray)
        {
            if (isRandom)
                ChangeLightSettings(DaylightInfos[Random.Range(0,DaylightInfos.Count)]);
            else
            {
                currentDayLightIndex = (currentDayLightIndex + 1) % DaylightInfos.Count;
                ChangeLightSettings(DaylightInfos[currentDayLightIndex]);
            }
        }
        else
        {
            if (isRandom)
            {
                ChangeLightSettings(GetRandomLight());
            }
        }
    }

    /// <summary>
    /// Changes light settings using DOTween and lerpTime
    /// </summary>
    /// <param name="daylight"></param>
    void ChangeLightSettings(DaylightInfo daylight)
    {
        Light.DOIntensity(daylight.LightIntensity, lerptime);
        Light.DOColor(daylight.LightColor, lerptime);
        Light.gameObject.transform.DORotate(daylight.SunRotation, lerptime);
    }
    /// <summary>
    /// Sets light settings directly, without DOTween lerping
    /// </summary>
    /// <param name="daylight"></param>
    void SetLightSettings(DaylightInfo daylight)
    {
        Light.intensity = daylight.LightIntensity;
        Light.color = daylight.LightColor;
        Light.gameObject.transform.rotation = Quaternion.Euler(daylight.SunRotation);
    }
    DaylightInfo GetRandomLight()
    {
        DaylightInfo newInfo = new DaylightInfo();

        newInfo.LightIntensity =
            Random.Range(daylightMin.LightIntensity, daylightMax.LightIntensity);

        newInfo.SunRotation = new Vector3(
            Random.Range(daylightMin.SunRotation.x, daylightMax.SunRotation.x),
            Random.Range(daylightMin.SunRotation.y, daylightMax.SunRotation.y),
            Random.Range(daylightMin.SunRotation.z, daylightMax.SunRotation.z));

        newInfo.LightColor = new Color(
            Random.Range(daylightMin.LightColor.r, daylightMax.LightColor.r),
            Random.Range(daylightMin.LightColor.g, daylightMax.LightColor.g),
            Random.Range(daylightMin.LightColor.b, daylightMax.LightColor.b),
            Random.Range(daylightMin.LightColor.a, daylightMax.LightColor.a));

        return newInfo;
    }
    void UpdateDayNightCycle()
    {
        timer += Time.deltaTime;
        float halfDuration = DayDuration * 0.5f;
        //gets fraction value (0-1) on halfDay cycle. goes from 0 to 1 twice in a day
        float fractionNum = timer <= halfDuration ? timer : timer - halfDuration;
        //makes fractionNum run inside a Day cycle
        fractionNum = timer >= DayDuration ? 0 : fractionNum;
        //resets timer to 0 every day change (like a clock does at midnight)
        timer = timer >= DayDuration ? 0 : timer;

        float fraction = fractionNum / (halfDuration);

        if (timer <= halfDuration)//DAY
        {
            daylightInfo.LightColor = Color.Lerp(SunlightColor, TransitionColor, ColorBlendCurve.Evaluate(fraction));

            float intensity = SunlightIntensityCurve.Evaluate(fraction) * MaxLightIntensityDay;
            //Clamping intensity between max and min values (useful for low intensity values)
            intensity = Mathf.Clamp(intensity, MinLightIntensity, MaxLightIntensityDay);
            daylightInfo.LightIntensity = intensity;
        }
        else if (timer > halfDuration && timer <= DayDuration)//NIGHT
        {
            daylightInfo.LightColor = Color.Lerp(MoonlightColor, TransitionColor, ColorBlendCurve.Evaluate(fraction));

            float intensity = MoonlightIntensityCurve.Evaluate(fraction) * MaxLightIntensityNight;
            //Clamping intensity between max and min values (useful for low intensity values)
            intensity = Mathf.Clamp(intensity, MinLightIntensity, MaxLightIntensityNight);
            daylightInfo.LightIntensity = intensity;
        }
        daylightInfo.SunRotation = Vector3.Lerp(MinLightRotation, MaxLightRotation, fraction);

        SetLightSettings(daylightInfo);

        //Debug.Log(timer + ", " + fraction);
    }
}
