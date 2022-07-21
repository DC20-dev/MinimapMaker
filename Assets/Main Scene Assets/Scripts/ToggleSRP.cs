using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class ToggleSRP : MonoBehaviour
{
    public bool SRP_ON;
    private bool temp;

    // Update is called once per frame
    void Update()
    {
        temp = GraphicsSettings.useScriptableRenderPipelineBatching;

        if (temp != SRP_ON)
            GraphicsSettings.useScriptableRenderPipelineBatching = SRP_ON;
    }
}
