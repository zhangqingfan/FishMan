using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class Reflection : MonoBehaviour
{
    public RenderTexture renderTexture;

    [HideInInspector]
    public Material reflectMaterial;

    public LayerMask reflectLayerMask;

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
        //reflectCamera.CopyFrom(Camera.main);  //不知道为什么不能在这里调用
        reflectCamera.enabled = false;
        reflectCamera.targetTexture = renderTexture;

        //reflectCamera.CopyFrom(Camera.main);  //在这里调用就可以，很奇怪！
        Shader.SetGlobalTexture("_ReflectionTex", renderTexture);

        Debug.Log(reflectCamera.pixelRect);
    }
    void BeginRender(ScriptableRenderContext context, Camera camera)
    {
        if (camera != Camera.main)
            return;

        reflectCamera.CopyFrom(Camera.main);
        reflectCamera.cullingMask = reflectLayerMask;

        var dis = Vector3.Dot(cameraTrans.position - transform.position, transform.up);
        var pos = cameraTrans.position - transform.up.normalized * dis * 2;
        reflectCamera.transform.position = pos;

        var dir = Vector3.Reflect(cameraTrans.forward, transform.up);
        reflectCamera.transform.rotation = Quaternion.LookRotation(dir);

        var viewPos = reflectCamera.worldToCameraMatrix.MultiplyPoint(transform.position);
        var viewNormal = reflectCamera.worldToCameraMatrix.MultiplyVector(transform.up);
        float w = -Vector3.Dot(viewPos, viewNormal);
        reflectCamera.projectionMatrix = reflectCamera.CalculateObliqueMatrix(new Vector4(viewNormal.x, viewNormal.y, viewNormal.z, w));

        UniversalRenderPipeline.RenderSingleCamera(context, reflectCamera);
    }
}
