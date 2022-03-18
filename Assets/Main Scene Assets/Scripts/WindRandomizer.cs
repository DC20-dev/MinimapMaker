using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(WindZone))]
public class WindRandomizer : MonoBehaviour
{
    [Header("Simple script that randomizes wind direction")]
    [Space]

    public bool UpdateWind;
    [Space]
    public float MinTimeUpdate;
    public float MaxTimeUpdate;
    [Space]
    public float MinXRotation;
    public float MaxXRotation;
    [Space]
    public float TweenDuration = 0.4f;


    float timer = 0, currentWaitTime;

    private void Start()
    {
        currentWaitTime = Random.Range(MinTimeUpdate, MaxTimeUpdate);
    }
    // Update is called once per frame
    void Update()
    {
        if (!UpdateWind) return;

        timer += Time.deltaTime;
        if(timer >= currentWaitTime)
            RandomizeWind();
    }

    void RandomizeWind()
    {
        timer = 0;
        currentWaitTime = Random.Range(MinTimeUpdate, MaxTimeUpdate);

        Vector3 newRot = new Vector3(
            Random.Range(MinXRotation, MaxXRotation),
            Random.Range(0f,359f),
            0);
        transform.DORotate(newRot, TweenDuration);
    }
}
