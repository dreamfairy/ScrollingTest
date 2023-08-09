using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class SSMFrustumCorners
{
    public Vector3[] _SrcNearCorner;//main相机坐标:近平面4个顶点
    public Vector3[] _SrcFarCorner;//main相机坐标:远平面4个顶点
    public Vector3[] _NearCorner;  //中间变量：一开始是main相机近平面世界坐标，然后转化到朝向光源的相机坐标:近平面4个顶点
    public Vector3[] _FarCorner;   //中间变量：一开始是main相机远平面世界坐标，然后转化到朝向光源的相机坐标:远平面4个顶点
    public Vector3[] _NearCornerWorld; //朝向光源的世界坐标:近平面4个顶点;
    public Vector3[] _FarCornerWorld;  //朝向光源的世界坐标:近平面4个顶点;

    public float3x4 _IntersectYTopPoints; //地表平面交点 (世界坐标)
    public float3x4 _IntersectYBottomPoints; //地面平面交点 (世界坐标)
    public float3   _IntersectNearPos; //求出的clamp近平面中心点 (世界坐标)
    public float3   _IntersectFarPos;  //求出的clamp远平面中心点 (世界坐标)
    
    public float _Near;
    public float _Far;
    public Camera _RefCamera;
    public Transform _RefTrans;
    public RenderTexture _RefRenderTexture;
    public int _Index;
    //CSM相关参数 x:本帧最大对角线长度，y:像素距离
    //备注: 目前版本x已经不是对角线长度了，y:这个表示1像素表示多大的世界距离,不同于纹素大小:1像素表示多大的uv距离
    public Vector2 _CSMParams;
    public Matrix4x4 _ProjectionMatrix;
    public Matrix4x4 _OriginProjectionMatrix;
    public Rect _ViewPort;
    public string _Keyword;
    
    public static SSMFrustumCorners RebuildShadowFrustumCorners(float nearDistance, float endDistance, Camera mainCamera)
    {
        SSMFrustumCorners cornersData = new SSMFrustumCorners();
        cornersData._Near = nearDistance;
        cornersData._Far = endDistance;

        cornersData._SrcNearCorner = new Vector3[4];
        cornersData._SrcFarCorner = new Vector3[4];
        cornersData._NearCorner = new Vector3[4];
        cornersData._FarCorner = new Vector3[4];
        cornersData._NearCornerWorld = new Vector3[4];
        cornersData._FarCornerWorld = new Vector3[4];

        CalcNerAndFarCorner(ref cornersData, nearDistance, endDistance, mainCamera);

        return cornersData;
    }
        
    public static void CalcNerAndFarCorner(ref SSMFrustumCorners cornersData, float nearDistance, float endDistance, Camera mainCamera)
    {
        mainCamera.CalculateFrustumCorners(new Rect(0, 0, 1, 1), nearDistance, Camera.MonoOrStereoscopicEye.Mono, cornersData._SrcNearCorner);
        mainCamera.CalculateFrustumCorners(new Rect(0, 0, 1, 1), endDistance, Camera.MonoOrStereoscopicEye.Mono, cornersData._SrcFarCorner);
    }
}
    