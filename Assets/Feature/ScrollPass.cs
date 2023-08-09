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
    
    struct CSM_CachePointInfo
    {
        public float ab;
        public float ac;
        public float bc;
        public float depth;
        public Vector3 worldPos;
        public Vector3 focusCenterPos;
        public int captureFrame;
        public Matrix4x4 worldToCameraMatrix;
    }
    
    private CSM_CachePointInfo m_SnapShotShadowCamreraInfo;

    public ScrollPass(RenderTexture scrollTexture, RenderTexture snapshotTexture, Material scrollMat)
    {
        m_scrollTexture = scrollTexture;
        m_snapShotTexture = snapshotTexture;
        m_scrollMat = scrollMat;
    }

    public void Setup(Camera shadowCamera, SSMFrustumCorners cornerData)
    {
        if (Application.isPlaying)
        {
            m_blitData = CalcOffsetUV2(shadowCamera, cornerData);
        }
    }


    private GameObject BackUpPosGo;
    public void BackupPos(Camera shadowCamera)
    {
        float ab = shadowCamera.transform.position.y;
        float bc = 0;
        float ac = 0;
        float degree = 90 - shadowCamera.transform.rotation.eulerAngles.x;
        ac = ab / Mathf.Cos((degree * Mathf.PI)/180.0f);
        bc = Mathf.Sqrt(ab * ab + ac * ac);

        Vector3 centerPos = shadowCamera.transform.position + shadowCamera.transform.forward * ac;
        
        m_SnapShotShadowCamreraInfo.ab = ab;
        m_SnapShotShadowCamreraInfo.ac = ac;
        m_SnapShotShadowCamreraInfo.bc = bc;
        m_SnapShotShadowCamreraInfo.depth = ac / (shadowCamera.farClipPlane - shadowCamera.nearClipPlane);
        m_SnapShotShadowCamreraInfo.focusCenterPos = centerPos;
        m_SnapShotShadowCamreraInfo.worldPos = shadowCamera.transform.position;
        m_SnapShotShadowCamreraInfo.worldToCameraMatrix = shadowCamera.worldToCameraMatrix;

        if (!BackUpPosGo)
        {
            BackUpPosGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            BackUpPosGo.transform.localScale = Vector3.one * 0.1f;
            BackUpPosGo.name = "BackupPos";
        }

        BackUpPosGo.transform.position = centerPos;
    }

    private GameObject NowPosGo;
    private float scrollSpeed;
    Vector4 CalcOffsetUV2(Camera shadowCamera, SSMFrustumCorners cornerData)
        {
            float ab = shadowCamera.transform.position.y;
            float bc = 0;
            float ac = 0;
            float degree = 90 - shadowCamera.transform.rotation.eulerAngles.x;
            ac = ab / Mathf.Cos((degree * Mathf.PI)/180.0f);
            bc = Mathf.Sqrt(ab * ab + ac * ac);

            Vector3 nowFrameCenterPos = shadowCamera.transform.position + shadowCamera.transform.forward * ac;

            if (!NowPosGo)
            {
                NowPosGo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                NowPosGo.transform.localScale = Vector3.one * 0.1f;
                NowPosGo.name = "NowPos";
            }

            NowPosGo.transform.position = nowFrameCenterPos;

            Matrix4x4 lastVP = GL.GetGPUProjectionMatrix(shadowCamera.projectionMatrix, true) * m_SnapShotShadowCamreraInfo.worldToCameraMatrix;
            Vector4 nowPosProj = lastVP.MultiplyPoint(nowFrameCenterPos);
            Vector4 lastPosProj = lastVP.MultiplyPoint(m_SnapShotShadowCamreraInfo.focusCenterPos);
            nowPosProj.x = nowPosProj.x * 0.5f + 0.5f;
            nowPosProj.y = nowPosProj.y * 0.5f + 0.5f;
            lastPosProj.x = lastPosProj.x * 0.5f + 0.5f;
            lastPosProj.y = lastPosProj.y * 0.5f + 0.5f;
            
            Vector3 diffPos = lastPosProj - nowPosProj;
            
            float onePixelDis = 1.0f/2048.0f;
            //float fw = 1.0f / Vector3.Distance(cornerData._FarCorner[0], cornerData._FarCorner[3]);

            // if (Mathf.Abs(diffPos.x) < onePixelDis)
            // {
            //     diffPos.x = 0;
            // }
            //
            // if (Mathf.Abs(diffPos.y) < onePixelDis)
            // {
            //     diffPos.y = 0;
            // }

            float dx = diffPos.x / onePixelDis;
            float dy = diffPos.y / onePixelDis;
            float fx = Mathf.Round(dx);
            float fy = Mathf.Round(dy);

            float fixedResultX = fx * onePixelDis;
            float fixedResultY = fy * onePixelDis;
            
            float lastStepX = m_blitData.z - fixedResultX;
            float lastStepY = m_blitData.w - fixedResultY;
            
            //Debug.LogFormat("Scroll X {0} Step {1}", fixedResultX, lastStep);

            if (lastStepX != 0 || lastStepY != 0)
            {
                Debug.LogFormat("StepChanged nowStep {0} , {1} _ Diff {2}, {3} _ {4}",fixedResultX, fixedResultY, lastStepX, lastStepY, Time.frameCount);
            }

            if (Mathf.Abs((float)lastStepX) / onePixelDis >= 2 || Mathf.Abs((float)lastStepY) / onePixelDis >= 2)
            {
                Debug.Log("May case shake_" + Time.frameCount);
            }

            return new Vector4(1, 1, (float)fixedResultX, (float)fixedResultY);
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
