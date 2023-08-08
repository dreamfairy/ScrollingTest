using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ScrollPass : ScriptableRenderPass
{
    private RenderTexture m_scrollTexture;
    private RenderTexture m_snapShotTexture;
    private Material m_scrollMat;
    private Vector4 m_blitData;
    private Camera m_shadowCam;

    public ScrollPass(RenderTexture scrollTexture, RenderTexture snapshotTexture, Material scrollMat)
    {
        m_scrollTexture = scrollTexture;
        m_snapShotTexture = snapshotTexture;
        m_scrollMat = scrollMat;
    }

    public void BackupPos()
    {
        
    }

    public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
    {
        base.Configure(cmd, cameraTextureDescriptor);
        this.ConfigureTarget(m_scrollTexture.depthBuffer);
        this.ConfigureClear(ClearFlag.Depth, clearColor);
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        var cmd = CommandBufferPool.Get("Scroll");
        cmd.SetGlobalTexture("SrcDepth", m_snapShotTexture.depthBuffer);
        cmd.SetGlobalVector("_BlitST", m_blitData);
        cmd.Blit(m_snapShotTexture, m_scrollTexture, m_scrollMat, 0);
        context.ExecuteCommandBuffer(cmd);
        
        //CommandBufferPool.Release(cmd);
        
        cmd.Clear();
        cmd.SetGlobalTexture("FinalDepth", m_scrollTexture.depthBuffer);
        context.ExecuteCommandBuffer(cmd);
        
        CommandBufferPool.Release(cmd);
    }
}
