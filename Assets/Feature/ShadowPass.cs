using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ShadowPass : ScriptableRenderPass
{
    private RenderTexture m_shadowTexture;
    private Camera m_shadowCam;
    private GameObject m_drawTarget;
    private Material m_customMat;
    
    public ShadowPass(RenderTexture shadowTexture, GameObject drawTarget, Material mat, Camera shadowCam)
    {
        m_shadowTexture = shadowTexture;
        m_shadowCam = shadowCam;
        m_drawTarget = drawTarget;
        m_customMat = mat;
    }

    public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
    {
        base.Configure(cmd, cameraTextureDescriptor);
        this.ConfigureTarget(m_shadowTexture.depthBuffer);
        this.ConfigureClear(ClearFlag.None, clearColor);
    }

    // This method is called before executing the render pass.
    // It can be used to configure render targets and their clear state. Also to create temporary render target textures.
    // When empty this render pass will render to the active camera render target.
    // You should never call CommandBuffer.SetRenderTarget. Instead call <c>ConfigureTarget</c> and <c>ConfigureClear</c>.
    // The render pipeline will ensure target setup and clearing happens in a performant manner.
    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {
    }

    // Here you can implement the rendering logic.
    // Use <c>ScriptableRenderContext</c> to issue drawing commands or execute command buffers
    // https://docs.unity3d.com/ScriptReference/Rendering.ScriptableRenderContext.html
    // You don't have to call ScriptableRenderContext.submit, the render pipeline will call it at specific points in the pipeline.
    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        if (!m_drawTarget)
        {
            m_drawTarget = GameObject.Find("ShadowTarget");
        }

        if (!m_shadowCam)
        {
            GameObject go = GameObject.Find("Light/ShadowCamera");
            m_shadowCam = go.GetComponent<Camera>();

            if (Application.isPlaying)
            {
                m_shadowCam.enabled = false;
            }
        }
        
        Matrix4x4 vp = GL.GetGPUProjectionMatrix(m_shadowCam.projectionMatrix, true) * m_shadowCam.worldToCameraMatrix;

        CommandBuffer cmd = CommandBufferPool.Get("Shadow");
        cmd.SetGlobalMatrix("_ShadowVP", vp);

        cmd.DrawMesh(m_drawTarget.GetComponent<MeshFilter>().sharedMesh, m_drawTarget.transform.localToWorldMatrix, m_customMat);

        context.ExecuteCommandBuffer(cmd);
        
        CommandBufferPool.Release(cmd);
    }

    // Cleanup any allocated resources that were created during the execution of this render pass.
    public override void OnCameraCleanup(CommandBuffer cmd)
    {
    }
}