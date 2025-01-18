using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class Reflection : MonoBehaviour
{
    public RenderTexture renderTexture;

    [HideInInspector]
    public Material reflectMaterial;

    public LayerMask reflectLayerMask;

    [Range(0.3f, 1.0f)]
    public float reflectQualilty;

    Camera reflectCamera;
    Transform cameraTrans;

    private void OnValidate()
    {
        renderTexture.Release();
        renderTexture.width = Mathf.RoundToInt(Screen.width * reflectQualilty);
        renderTexture.height = Mathf.RoundToInt(Screen.height * reflectQualilty);
        renderTexture.Create();
    }

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
        reflectCamera.enabled = false;
        reflectCamera.targetTexture = renderTexture;
        reflectMaterial.SetTexture("_ReflectionTex", renderTexture);
        //Shader.SetGlobalTexture("_ReflectionTex", renderTexture);

        reflectCamera.CopyFrom(Camera.main);
        reflectCamera.cullingMask = reflectLayerMask;
    }

    void BeginRender(ScriptableRenderContext context, Camera camera)
    {
        if (camera != Camera.main)
            return;

 

        var dis = Vector3.Dot(cameraTrans.position - transform.position, transform.up);
        var pos = cameraTrans.position - transform.up.normalized * dis * 2;
        reflectCamera.transform.position = pos;

        var dir = Vector3.Reflect(cameraTrans.forward, transform.up);
        reflectCamera.transform.rotation = Quaternion.LookRotation(dir);

        var viewPos = Camera.main.worldToCameraMatrix.MultiplyPoint(transform.position);
        var viewNormal = Camera.main.worldToCameraMatrix.MultiplyVector(-transform.up);
        float w = -Vector3.Dot(viewPos, viewNormal);
        reflectCamera.projectionMatrix = reflectCamera.CalculateObliqueMatrix(new Vector4(viewNormal.x, viewNormal.y, viewNormal.z, w));

        UniversalRenderPipeline.RenderSingleCamera(context, reflectCamera);
    }

    /*
    var plane = CameraSpacePlane(mainCamera.worldToCameraMatrix, transform.position - transform.up * 0.001f, -transform.up);
    Vector4 CameraSpacePlane(Matrix4x4 worldToCameraMatrix, Vector3 pos, Vector3 normal)
    {
        Vector3 viewPos = worldToCameraMatrix.MultiplyPoint(pos);
        Vector3 viewNormal = worldToCameraMatrix.MultiplyVector(normal).normalized;
        
        return new Vector4(viewNormal.x, viewNormal.y, viewNormal.z, w);
    }
    */

}
