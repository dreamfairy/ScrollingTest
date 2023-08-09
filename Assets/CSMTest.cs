using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CSMTest : MonoBehaviour
{
    public bool UseOneFrame = false;
    public bool DoShakeX = false;
    public bool DoShakeZ = false;
    public float ShakeSpeed = 1.0f;
    public float ShakeLen = 1.0f;
    public bool TakeSnapShot = false;
    public static bool s_TakeSnapShot;

    private Vector3 MarkPos;
    private bool LastDoShake = false;
    private void OnValidate()
    {
        if (UseOneFrame)
        {
            Application.targetFrameRate = 1;
        }
        else
        {
           Application.targetFrameRate = -1;
        }
        
        if (TakeSnapShot)
        {
            s_TakeSnapShot = TakeSnapShot;
            TakeSnapShot = false;
        }

        bool thisFrameShaake = (DoShakeX || DoShakeZ);
        if (LastDoShake != thisFrameShaake)
        {
            LastDoShake = thisFrameShaake;
            MarkPos = this.transform.position;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (DoShakeX && DoShakeZ)
        {
            float dis = Mathf.Sin(Time.frameCount * ShakeSpeed) * ShakeLen;
            float disZ = Mathf.Cos(Time.frameCount * ShakeSpeed) * ShakeLen;
            this.transform.position = new Vector3(MarkPos.x + dis, MarkPos.y, MarkPos.z + disZ);
            return;
        }
        
        if (DoShakeX)
        {
            float dis = Mathf.Sin(Time.frameCount * ShakeSpeed) * ShakeLen;
            this.transform.position = new Vector3(MarkPos.x + dis, MarkPos.y, MarkPos.z);
        }

        if (DoShakeZ)
        {
            float dis = Mathf.Cos(Time.frameCount * ShakeSpeed) * ShakeLen;
            this.transform.position = new Vector3(MarkPos.x, MarkPos.y, MarkPos.z + dis);
        }
    }
    
    // void EnableRenderFeature(bool useLegacyCSM)
    // {
    //     UniversalAdditionalCameraData urpData = Camera.main.GetComponent<UniversalAdditionalCameraData>();
    //     if (urpData)
    //     {
    //         var renderer = urpData.scriptableRenderer;
    //         var property =
    //             typeof(ScriptableRenderer).GetProperty("rendererFeatures",
    //                 BindingFlags.NonPublic | BindingFlags.Instance);
    //
    //         List<ScriptableRendererFeature> features = property.GetValue(renderer) as List<ScriptableRendererFeature>;
    //
    //         foreach (var feature in features)
    //         {
    //             if (feature.GetType() == typeof(SSM_RenderFeature))
    //             {
    //                 feature.SetActive(useLegacyCSM);
    //             }
    //
    //             if (feature.GetType() == typeof(CSMS_RenderFeature))
    //             {
    //                 feature.SetActive(!useLegacyCSM);
    //                 m_csmsFeature = feature as CSMS_RenderFeature;
    //             }
    //         }
    //     }
    // }
}
