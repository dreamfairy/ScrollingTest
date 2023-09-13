using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ShadowFeature : ScriptableRendererFeature
{
    //public bool LockCameraCorner = false;
    public RenderTexture ShadowTexture;
    public RenderTexture SnapshotTexture;
    public RenderTexture ScrollTexture;
    public GameObject DrawTarget;
    public Material DrawMat;
    public Material SnapShotMat;
    public Material ScrollMat;
    public Material DrawPlaneMat;
    public Camera ShadowCamera;
    
    ShadowPass m_ScriptablePass;
    private SnapshotPass m_SnapShotPass;
    private ScrollPass m_ScrollPass;
    private DrawPlanePass m_DrawPlanePass;

    private RenderTexture m_innerShadowTexture;

    /// <inheritdoc/>
    public override void Create()
    {
        //ShadowTexture.format = RenderTextureFormat.Depth;

        if (null == m_innerShadowTexture)
        {
            m_innerShadowTexture =
                new RenderTexture(ShadowTexture.width, ShadowTexture.height, 24, RenderTextureFormat.Depth);
            m_innerShadowTexture.autoGenerateMips = false;
            m_innerShadowTexture.filterMode = FilterMode.Point;
            m_innerShadowTexture.wrapMode = TextureWrapMode.Clamp;
        }
        
        m_ScriptablePass = new ShadowPass(m_innerShadowTexture, DrawTarget, DrawMat);
        m_SnapShotPass = new SnapshotPass(m_innerShadowTexture, SnapshotTexture, SnapShotMat);
        m_ScrollPass = new ScrollPass(SnapshotTexture, ScrollTexture, ScrollMat);
        m_DrawPlanePass = new DrawPlanePass(DrawPlaneMat);
        
        // Configures where the render pass should be injected.
        m_ScriptablePass.renderPassEvent = RenderPassEvent.BeforeRenderingShadows;
        m_SnapShotPass.renderPassEvent = RenderPassEvent.BeforeRenderingShadows;
        m_ScrollPass.renderPassEvent = RenderPassEvent.BeforeRenderingShadows;
        m_DrawPlanePass.renderPassEvent = RenderPassEvent.BeforeRenderingOpaques;
    }
    
    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (!Application.isPlaying)
        {
            return;
        }

        if (!ShadowCamera)
        {
            GameObject go = GameObject.Find("Light/ShadowCamera");
            ShadowCamera = go.GetComponent<Camera>();

            ShadowCamera.enabled = false;
        }
        
        m_ScriptablePass.Setup(ShadowCamera, false);
        
        //if (CSMTest.s_TakeSnapShot)
        {
            renderer.EnqueuePass(m_ScriptablePass);
            
            renderer.EnqueuePass(m_SnapShotPass);
            
            m_ScrollPass.BackupPos(ShadowCamera);

            CSMTest.s_TakeSnapShot = false;
        }
        
        m_ScrollPass.Setup(ShadowCamera, m_ScriptablePass.GetData());
        renderer.EnqueuePass(m_ScrollPass);
        renderer.EnqueuePass(m_DrawPlanePass);
    }
}


