using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class SnapshotPass : ScriptableRenderPass
{
   private RenderTexture m_shadowTexture;
   private RenderTexture m_targetTexture;
   private Material m_blitDepthMat;
   
   public SnapshotPass(RenderTexture shadowTexture, RenderTexture snapshotTexture, Material blitDepthMat)
   {
      m_shadowTexture = shadowTexture;
      m_targetTexture = snapshotTexture;
      m_blitDepthMat = blitDepthMat;
   }

   public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
   {
      base.Configure(cmd, cameraTextureDescriptor);
      this.ConfigureTarget(m_targetTexture.depthBuffer);
      this.ConfigureClear(ClearFlag.None, clearColor);
   }

   public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
   {
      var cmd = CommandBufferPool.Get("Snapshot");
      cmd.SetGlobalTexture("_SrcDepth", m_shadowTexture.depthBuffer);
      cmd.Blit(m_shadowTexture.depthBuffer, m_targetTexture.depthBuffer, m_blitDepthMat, 0);
      context.ExecuteCommandBuffer(cmd);
      CommandBufferPool.Release(cmd);
   }

 
}
