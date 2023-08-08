using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DrawPlanePass : ScriptableRenderPass
{
    private Material m_drawMat;
    private RenderTexture m_scrollShadowTexture;
    private GameObject m_renderTarget;

    public DrawPlanePass(Material drawMat)
    {
        //m_scrollShadowTexture = scrollTexture;
        m_drawMat = drawMat;
    }
    
    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        if (!m_renderTarget)
        {
            m_renderTarget = GameObject.Find("Plane");
            m_renderTarget.SetActive(false);
        }

        var cmd = CommandBufferPool.Get("Plane");
        cmd.DrawMesh(m_renderTarget.GetComponent<MeshFilter>().sharedMesh, m_renderTarget.transform.localToWorldMatrix, m_drawMat, 0);
        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }
}
