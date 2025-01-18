using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class Reflection : MonoBehaviour
{
    public RenderTexture renderTexture;
    public Material reflectMaterial;
    Camera reflectCamera;
    Transform cameraTrans;

    private void OnEnable()
    {
        RenderPipelineManager.beginCameraRendering += BeginRender;
    }

    private void OnDisable()
    {
        RenderPipelineManager.beginCameraRendering -= BeginRender;
    }

    void Start()
    {
        cameraTrans = Camera.main.transform;

        var go = new GameObject("Reflection Camera");
        reflectCamera = go.AddComponent<Camera>();
        reflectCamera.CopyFrom(Camera.main);
        reflectCamera.cullingMask = LayerMask.GetMask("reflection");  //must have different layer with main camera
        reflectCamera.enabled = false;
        reflectCamera.targetTexture = renderTexture;

        reflectMaterial.SetTexture("_ReflectionTex", renderTexture);
    }

    void BeginRender(ScriptableRenderContext context, Camera camera)
    {
        if (camera != Camera.main)
            return;

        var dis = Vector3.Dot(cameraTrans.position - transform.position, transform.up);
        var pos = cameraTrans.position - transform.up.normalized * dis * 2;
        reflectCamera.transform.position = pos;

        var dir = Vector3.Reflect(cameraTrans.forward, transform.up);
        reflectCamera.transform.LookAt(reflectCamera.transform.position + dir);//

        reflectCamera.Render();
    }
}
