using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class RenderTextureSaver : MonoBehaviour
{
    public bool Screenshot;

    Camera cam;
    Texture2D toExport;
    int texNum = 0, screenshotLayerMask, layermaskWithUI;
    string SavePath, filename;
    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;
        SavePath = Application.dataPath + "/Screenshots/";
        filename = "screenshot";
        screenshotLayerMask = ~screenshotLayerMask;
        screenshotLayerMask = 0 << 5;
        screenshotLayerMask = screenshotLayerMask & cam.cullingMask;

        layermaskWithUI = cam.cullingMask;

        if (!Directory.Exists(SavePath))
        {
            Directory.CreateDirectory(SavePath);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!Screenshot) return;

        Screenshot = false;

        TakeScreenshot();
    }

    void TakeScreenshot()
    {
        //5 is UI layer, so it does not render UI and therefore no UI is in the screenshot
        cam.cullingMask = screenshotLayerMask;     
        ScreenCapture.CaptureScreenshot(GetValidFilename());
        cam.cullingMask = layermaskWithUI;
    }

    string GetValidFilename()
    {
        bool valid = false;
        string tempFilename = null;

        while (!valid)
        {
            tempFilename = $"{SavePath}{filename}{texNum}.png";

            if(!File.Exists(tempFilename)) 
                valid = true;
            else 
                texNum++;
        }

        return tempFilename;
    }
}
