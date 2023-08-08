using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ShadowFeature : ScriptableRendererFeature
{
    public bool TakeSnapShot = false;
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

    /// <inheritdoc/>
    public override void Create()
    {
        m_ScriptablePass = new ShadowPass(ShadowTexture, DrawTarget, DrawMat);
        m_SnapShotPass = new SnapshotPass(ShadowTexture, SnapshotTexture, SnapShotMat);
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
        
        //if (TakeSnapShot)
        {
            TakeSnapShot = false;
            
            m_ScriptablePass.Setup(ShadowCamera);
            
            renderer.EnqueuePass(m_ScriptablePass);
            
            renderer.EnqueuePass(m_SnapShotPass);
            
            m_ScrollPass.BackupPos(ShadowCamera);
        }
        
        renderer.EnqueuePass(m_ScrollPass);
        renderer.EnqueuePass(m_DrawPlanePass);
    }
}


