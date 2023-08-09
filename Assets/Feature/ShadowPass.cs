using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ShadowPass : ScriptableRenderPass
{
    private RenderTexture m_shadowTexture;
    private Camera m_shadowCam;
    private GameObject m_drawTarget;
    private Material m_customMat;
    private int m_shadowMapSize = 2048;
    private Matrix4x4 _csmVPMatrix;
    private SSMFrustumCorners m_cornerData;
     
    public ShadowPass(RenderTexture shadowTexture, GameObject drawTarget, Material mat)
    {
        m_shadowTexture = shadowTexture;
        m_drawTarget = drawTarget;
        m_customMat = mat;
        
        m_cornerData = SSMFrustumCorners.RebuildShadowFrustumCorners(0.1f,10,Camera.main);
    }

    public SSMFrustumCorners GetData()
    {
        return m_cornerData;
    }

    public Camera GetShadowCamera()
    {
        return m_shadowCam;
    }

    public void Setup(Camera cam, bool lockCorner)
    {
        m_shadowCam = cam;
        
        m_cornerData._RefCamera = m_shadowCam;
        CalcShadowCameraCorner(m_cornerData, lockCorner);
        m_shadowCam.projectionMatrix = m_cornerData._OriginProjectionMatrix;
        
        Matrix4x4 vp = GL.GetGPUProjectionMatrix(m_shadowCam.projectionMatrix, true) * m_shadowCam.worldToCameraMatrix;
        Shader.SetGlobalMatrix("_ShadowVP", vp);
    }

    public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
    {
        base.Configure(cmd, cameraTextureDescriptor);
        this.ConfigureTarget(m_shadowTexture.depthBuffer);
        this.ConfigureClear(ClearFlag.All, clearColor);
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

       

       
        CommandBuffer cmd = CommandBufferPool.Get("Shadow");
        //cmd.SetGlobalMatrix("_ShadowVP", vp);

        cmd.DrawMesh(m_drawTarget.GetComponent<MeshFilter>().sharedMesh, m_drawTarget.transform.localToWorldMatrix, m_customMat);

        context.ExecuteCommandBuffer(cmd);
        
        CommandBufferPool.Release(cmd);
    }

     public void CalcShadowCameraCorner(SSMFrustumCorners cornerData, bool lockCameraCorner)
        {
            if (!cornerData._RefCamera)
            {
                return;
            }

            if (lockCameraCorner)
            {
                return;
            }
            Camera _mainCamera = Camera.main;
    
            var _NearCorner = cornerData._NearCorner;
            var _FarCorner = cornerData._FarCorner;
            var _SrcNearCorner = cornerData._SrcNearCorner;
            var _SrcFarCorner = cornerData._SrcFarCorner;
            var _camera = cornerData._RefCamera;

            float csmStart = cornerData._Near;
            float csmEnd = cornerData._Far;
            SSMFrustumCorners.CalcNerAndFarCorner(ref cornerData, csmStart, csmEnd, _mainCamera);
                
            //从主相机本地空间转换到世界空间
            for (int i = 0; i < 4; i++)
            {
                _NearCorner[i] = _mainCamera.transform.TransformPoint(_SrcNearCorner[i]);
                _FarCorner[i] = _mainCamera.transform.TransformPoint(_SrcFarCorner[i]);
            }

            Light _mainLight = GameObject.Find("Light").GetComponent<Light>();

            //从世界空间转换到朝向光源方向。
            Matrix4x4 shadow2World =Matrix4x4.TRS(Vector3.zero, _mainLight.transform.rotation ,Vector3.one);
            Matrix4x4 world2Shadow = shadow2World.inverse;

            for (int i = 0; i < 4; i++)
            {
                _NearCorner[i] = world2Shadow *_NearCorner[i];
                _FarCorner[i] = world2Shadow * _FarCorner[i];
            }

            float[] xs = { _NearCorner[0].x, _NearCorner[1].x, _NearCorner[2].x, _NearCorner[3].x,
                           _FarCorner[0].x, _FarCorner[1].x, _FarCorner[2].x, _FarCorner[3].x };

            float[] ys = { _NearCorner[0].y, _NearCorner[1].y, _NearCorner[2].y, _NearCorner[3].y,
                           _FarCorner[0].y, _FarCorner[1].y, _FarCorner[2].y, _FarCorner[3].y };

            float[] zs = { _NearCorner[0].z, _NearCorner[1].z, _NearCorner[2].z, _NearCorner[3].z,
                           _FarCorner[0].z, _FarCorner[1].z, _FarCorner[2].z, _FarCorner[3].z };

            float minX = Mathf.Min(xs);
            float maxX = Mathf.Max(xs);

            float minY = Mathf.Min(ys);
            float maxY = Mathf.Max(ys);

            float minZ = Mathf.Min(zs);
            float maxZ = Mathf.Max(zs);
            
            // {
                _NearCorner[0] = new Vector3(minX, minY, minZ);
                _NearCorner[1] = new Vector3(minX, maxY, minZ);
                _NearCorner[2] = new Vector3(maxX, maxY, minZ);
                _NearCorner[3] = new Vector3(maxX, minY, minZ);

                _FarCorner[0] = new Vector3(minX, minY, maxZ);
                _FarCorner[1] = new Vector3(minX, maxY, maxZ);
                _FarCorner[2] = new Vector3(maxX, maxY, maxZ);
                _FarCorner[3] = new Vector3(maxX, minY, maxZ);
                
                float farDist = Vector3.Distance(_FarCorner[0], _FarCorner[1]);
                //近平面到远平面对角线距离
                float crossDist = Vector3.Distance(_NearCorner[0], _FarCorner[2]);
                
                //Debug.LogFormat("farDist {0} crossdist {1}", farDist, crossDist);

                //TODO: 这里maxDist感觉应该取 x,y 较大的: 也就是 max (dis(_FarCorner[0], _FarCorner[1]),dis(_FarCorner[1], _FarCorner[2]))
                //因为游戏是横屏，理论上是x方向比较宽
                float maxDist = farDist; //crossDist;
                float fWorldUnitsPerTexel = maxDist / (float)(m_shadowMapSize);
                _camera.nearClipPlane = 0;

                _camera.farClipPlane = maxZ - minZ;
                // _camera.farClipPlane = 15; //TODO: 这里写死了，应该是为地形而写的
                _camera.aspect = 1.0f;
                cornerData._CSMParams.x = maxDist;
                cornerData._CSMParams.y = fWorldUnitsPerTexel;
                float halfMaxDist = maxDist * 0.5f;

                
                _camera.orthographicSize = halfMaxDist;
                Matrix4x4 orthProjectionMatrix = Matrix4x4.Ortho(-halfMaxDist, halfMaxDist, -halfMaxDist, halfMaxDist, 0, _camera.farClipPlane  );
                cornerData._ProjectionMatrix = GL.GetGPUProjectionMatrix(orthProjectionMatrix, true);
                cornerData._OriginProjectionMatrix = orthProjectionMatrix;
            // }

            float posX = (minX + maxX) * 0.5f;
            float posY = (minY + maxY) * 0.5f;
            float posZ = minZ;
            
            posX /= cornerData._CSMParams.y;
            posX = Mathf.Floor(posX);
            posX *= cornerData._CSMParams.y;

            posY /= cornerData._CSMParams.y;
            posY = Mathf.Floor(posY);
            posY *= cornerData._CSMParams.y;

            posZ /= cornerData._CSMParams.y;
            posZ = Mathf.Floor(posZ);
            posZ *= cornerData._CSMParams.y;

            //由于从主相机为透视相机，且宽高比不一致，因此阴影相机中心点重算位正方形视锥体
            Vector3 pos = new Vector3(posX, posY, posZ);
            Vector3 worldPos = shadow2World*pos;
            
            _camera.transform.position = worldPos;
            //Debug.LogFormat("SetCameraPosition:{0} frame {1}", _camera.transform.position, Time.frameCount);

            if (_mainLight)
            {
                _camera.transform.rotation = _mainLight.transform.rotation;
            }

            Matrix4x4 WorldToLocal = _camera.worldToCameraMatrix;
            //dx有点问题 后面再看
            // if (SystemInfo.usesReversedZBuffer)
            // {
            //     cornerData._ProjectionMatrix.m20 = -cornerData._ProjectionMatrix.m20;
            //     cornerData._ProjectionMatrix.m21 = -cornerData._ProjectionMatrix.m21;
            //     cornerData._ProjectionMatrix.m22 = -cornerData._ProjectionMatrix.m22;
            //     cornerData._ProjectionMatrix.m23 = -cornerData._ProjectionMatrix.m23;
            // }
            // Matrix4x4 trans = Matrix4x4.TRS(Vector3.one * 0.5f , Quaternion.identity, Vector3.one * 0.5f);
            // Matrix4x4 VP = trans * cornerData._ProjectionMatrix * WorldToLocal;
            // _csmVPMatrix = VP;
            _csmVPMatrix = cornerData._ProjectionMatrix * WorldToLocal;
        }
}