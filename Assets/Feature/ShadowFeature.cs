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
    public Camera ShadowCamera;
    
    ShadowPass m_ScriptablePass;
    private SnapshotPass m_SnapShotPass;
    private ScrollPass m_ScrollPass;

    /// <inheritdoc/>
    public override void Create()
    {
        m_ScriptablePass = new ShadowPass(ShadowTexture, DrawTarget, DrawMat, ShadowCamera);
        m_SnapShotPass = new SnapshotPass(ShadowTexture, SnapshotTexture, SnapShotMat);
        m_ScrollPass = new ScrollPass(SnapshotTexture, ScrollTexture, ScrollMat);
        
        // Configures where the render pass should be injected.
        m_ScriptablePass.renderPassEvent = RenderPassEvent.BeforeRenderingShadows;
        m_SnapShotPass.renderPassEvent = RenderPassEvent.BeforeRenderingShadows;
        m_ScrollPass.renderPassEvent = RenderPassEvent.BeforeRenderingShadows;
    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        //if (TakeSnapShot)
        {
            TakeSnapShot = false;
            
            renderer.EnqueuePass(m_ScriptablePass);
            
            renderer.EnqueuePass(m_SnapShotPass);
            
            m_ScrollPass.BackupPos();
        }
        
        renderer.EnqueuePass(m_ScrollPass);
    }
}


