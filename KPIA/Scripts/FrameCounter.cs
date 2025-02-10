using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FrameCounter : MonoBehaviour
{
    float deltaTime = 0f;

    public TMP_Text fpsText;

    private void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
    }

    private void OnGUI()
    {
        float ms = deltaTime * 1000f;
        float fps = 1.0f / deltaTime;
        fpsText.text = string.Format("{0:0.} FPS ({1:0.0} ms)", fps, ms);
    }
}
