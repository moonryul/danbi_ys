using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class RTmaster : MonoBehaviour {
  [Header("Bounced amount of Ray Tracing (default = 2)"), Space(5)]
  [Range(0, 8), Header("---!Ray Tracer Parameters!---"), Space(10)]
  public int Bounce = 2;

  [Header("Actual Ray Tracing Compute Shader."), Space(5)]
  [Header("---!Required Resources!---"), Space(10), SerializeField]
  ComputeShader RTshader;

  /// <summary>
  /// Hard-Reference of the main camera.
  /// </summary>
  Camera MainCamRef;

  [Header("Result Image of Ray tracing is stored into here"), Space(5)]
  public RenderTexture ResultRenderTex;

  [Header("Skybox Texture for testing."), Space(5)]
  public Texture SkyboxTex;

  [Header("Video Material for testing the realtime-reflection."), Space(5)]
  public Material VideoMat;

  /// <summary>
  /// Current Sample Count for the optimized resampler of pixel edges.
  /// </summary>
  public uint CurrentSampleCount { get; set; }

  /// <summary>
  /// Resampler Material (Hidden/AddShader).
  /// </summary>
  Material ResampleAddMat;

  [Header("Light for Ray Tracing Rendering (Current -> Lambertian / Future -> BDRF PBR)"), Space(5)]
  public Light DirLight;
  /// <summary>
  /// 
  /// </summary>
  public RThelper Helper;
  /// <summary>
  /// 
  /// </summary>
  public RTsphereLocator Locator;
  /// <summary>
  /// 
  /// </summary>
  RTdbg dbg;

  void Start() {
    MainCamRef = GetComponent<Camera>();
    Locator.LocateSphereRandomly();
    dbg = new RTdbg();
  }

  void OnRenderImage(RenderTexture source, RenderTexture destination) {
    Debug.Assert(!RTshader.Null(), $"Ray tracing compute shader cannot be null!");
    // Rebuild the mesh objects if new mesh objects are coming up.
    RebuildMeshObjects();
    // Set Shader parameters.
    SetShaderParams();
    // Render it.
    Render(destination);
    // Retrieve the data from the vertex color buffer.
    if (dbg.RetrivedColBuf == null) {
      dbg.RetrivedColBuf = new Vector3[Helper.VtxColorsList.Count];
    }

    //Helper.VtxColorsComputeBuf.GetData(dbg.RetrivedColBuf);
    //for (int i = 0; i < dbg.RetrivedColBuf.Length; ++i) {
    //  Debug.Log($"R: {dbg.RetrivedColBuf[i].x}, "
    //          + $"G: {dbg.RetrivedColBuf[i].y}, "
    //          + $"B: {dbg.RetrivedColBuf[i].z}");
    //}
  }

  void Update() {
    if (transform.hasChanged) {
      CurrentSampleCount = 0;
      transform.hasChanged = false;
    }

    SetShaderParamsRuntime();
    //RayTracingShader.SetTexture(0, "_SkyboxTexture", _Video.mainTexture);
  }

  void Render(RenderTexture destination) {
    // Make sure we have a current render target
    InitRenderTexture();

    // Set the target and dispatch the compute shader
    RTshader.SetTexture(0, "_Result", ResultRenderTex);
    // Check the ratio of Screen.Width and Screen.Height is 16 by 9.
    //  if (1) {
    //   // ASSERT
    //  }

    //  if (1) {
    //    // ASSERt
    //  }

    int threadGroupsX = Mathf.CeilToInt(Screen.width / 8.0f);
    int threadGroupsY = Mathf.CeilToInt(Screen.height / 8.0f);
    RTshader.Dispatch(0, threadGroupsX, threadGroupsY, 1);

    // Blit the result texture to the screen
    if (ResampleAddMat.Null()) {
      ResampleAddMat = new Material(Shader.Find("Hidden/AddShader"));
      //Debug.Log("add material is created!");
    }

    ResampleAddMat.SetFloat("_Sample", CurrentSampleCount);
    Graphics.Blit(ResultRenderTex, destination);
    if (CurrentSampleCount < 100) {
      ++CurrentSampleCount;
    } else {
      return;
    }
  }

  void InitRenderTexture() {
    if (ResultRenderTex.Null()
      || ResultRenderTex.width != Screen.width
      || ResultRenderTex.height != Screen.height) {
      // Release render texture if we already have one
      if (!ResultRenderTex.Null()) {
        ResultRenderTex.Release();
        ResultRenderTex = null;
      }

      // Get a render target for Ray Tracing
      ResultRenderTex = new RenderTexture(Screen.width, Screen.height, 0,
          RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
      ResultRenderTex.enableRandomWrite = true;
      ResultRenderTex.Create();
    }
  }

  void SetShaderParams() {
    RTshader.SetInt("_Bounces", Bounce);
    RTshader.SetVector("_PixelOffset",
                       new Vector2(UnityEngine.Random.value, UnityEngine.Random.value));
    RTshader.SetTexture(0, "_SkyboxTexture", SkyboxTex);
    RTshader.SetMatrix("_CameraToWorld", MainCamRef.cameraToWorldMatrix);
    RTshader.SetMatrix("_CameraInverseProjection", MainCamRef.projectionMatrix.inverse);

    var light_dir = DirLight.transform.forward;
    RTshader.SetVector("_DirectionalLight",
                       new Vector4(light_dir.x, light_dir.y, light_dir.z, DirLight.intensity));
    SetComputeBuffer("_Spheres", Locator.SpheresComputeBuf);
    SetComputeBuffer("_MeshObjects", Helper.MeshObjectsAttrsComputeBuf);
    SetComputeBuffer("_Vertices", Helper.VerticesComputeBuf);
    SetComputeBuffer("_Indices", Helper.IndicesComputeBuf);
    SetComputeBuffer("_Colors", Helper.VtxColorsComputeBuf);
    //RayTracingShader.SetInt("_Test_Colors_Length", _vertex_color_buffer.count);
  }

  void SetShaderParamsRuntime() {
    RTshader.SetVector("_Time",
    new Vector4(Time.time * 10,
                Time.time * 20,
                Time.time * 50,
                Time.time * 100));
  }

  void RebuildMeshObjects() {
    Helper.RebuildMeshObjects();

    CreateOrSetComputeBuffer(ref Helper.MeshObjectsAttrsComputeBuf, Helper.MeshObjectsAttrsList, 80); //
    CreateOrSetComputeBuffer(ref Helper.VerticesComputeBuf, Helper.VerticesList, 12); // float3
    CreateOrSetComputeBuffer(ref Helper.IndicesComputeBuf, Helper.IndicesList, 4); // int
    CreateOrSetComputeBuffer(ref Helper.VtxColorsComputeBuf, Helper.VtxColorsList, 12); // float3
  }

  static void CreateOrSetComputeBuffer<T>(ref ComputeBuffer buffer,
                                          List<T> data,
                                          int stride) where T : struct {
    // check if we already have a compute buffer.
    if (!buffer.Null()) {
      // If no data or buffer doesn't match the given condition, release it.
      if (data.Count == 0
        || buffer.count != data.Count
        || buffer.stride != stride) {
        buffer.Release();
        buffer = null;
      }
    }

    if (data.Count != 0) {
      // If the buffer has been released or wasn't there to begin with, create it.
      if (buffer.Null()) {
        buffer = new ComputeBuffer(data.Count, stride);
      }
      // Set data on the buffer.
      buffer.SetData(data);
    }
  }

  void SetComputeBuffer(string name, ComputeBuffer buffer) {
    if (!buffer.Null()) {
      RTshader.SetBuffer(0, name, buffer);
    }
  }
};
