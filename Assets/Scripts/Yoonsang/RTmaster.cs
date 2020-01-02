using UnityEngine;

/// <summary>
/// Ray Tracer masters that controls and assemble everything of the ray tracer.
/// </summary>
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
  public RTcomputeShaderHelper Helper;
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
    ResampleAddMat = new Material(Shader.Find("Hidden/AddShader"));
  }

  /// <summary>
  /// It's callbacked only by Camera component.
  /// </summary>
  /// <param name="source"></param>
  /// <param name="destination"></param>
  void OnRenderImage(RenderTexture source, RenderTexture destination) {
    Debug.Assert(!RTshader.Null(), $"Ray tracing compute shader cannot be null!");
    // Rebuild the mesh objects if new mesh objects are coming up.
    //dbg.DbgStopwatch.Start();
    RebuildMeshObjects();
    //dbg.DbgStopwatch.Stop();
    //RTdbg.Log($"Elapsed time of RebuildMeshObjects() is {dbg.DbgStopwatch.ElapsedMilliseconds}");
    // Set Shader parameters.

    //dbg.DbgStopwatch.Start();
    SetShaderParams();
    //dbg.DbgStopwatch.Stop();
    //RTdbg.Log($"Elapsed time of SetSharderParams() is {dbg.DbgStopwatch.ElapsedMilliseconds}");
    // Render it.

    Render(destination);
    // Retrieve the data from the vertex color buffer.
    //if (dbg.RetrivedColBuf == null) { 
    //  dbg.RetrivedColBuf = new Vector3[Helper.VtxColorsList.Count];
    //}    

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

    SetShaderParamsAtRuntime();
    //RayTracingShader.SetTexture(0, "_SkyboxTexture", _Video.mainTexture);
  }

  /// <summary>
  /// 
  /// </summary>
  /// <param name="destination"></param>
  void Render(RenderTexture destination) {
    // Make sure we have a current render target
    RefreshRenderTarget();

    // Set the target and dispatch the compute shader
    RTshader.SetTexture(0, "_Result", ResultRenderTex);
    // TODO: Check the ratio of Screen.Width and Screen.Height is 16 by 9.   

    int threadGroupsX = Mathf.CeilToInt(Screen.width / 8.0f);
    int threadGroupsY = Mathf.CeilToInt(Screen.height / 8.0f);
    RTshader.Dispatch(0, threadGroupsX, threadGroupsY, 1);

    //ResampleAddMat.SetFloat("_Sample", CurrentSampleCount);
    // Blit the result texture to the screen    
    Graphics.Blit(ResultRenderTex, destination);
    //if (CurrentSampleCount < 100) {
    //  ++CurrentSampleCount;
    //} else {
    //  return;
    //}
  }

  /// <summary>
  /// Render Target for RT must be refreshed on every render due to
  /// keep printing on the result.
  /// </summary>
  void RefreshRenderTarget() {
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

  /// <summary>
  /// Set the Shader paramter into the ray tracer.
  /// </summary>
  void SetShaderParams() {
    // Set the maximum count of ray bounces.
    RTshader.SetInt("_RayBounceCountMax", Bounce);
    // Set the pixel offset for screen uv.
    RTshader.SetVector("_PixelOffset", new Vector2(UnityEngine.Random.value, UnityEngine.Random.value));
    // Set the skybox texture.
    RTshader.SetTexture(0, "_SkyboxTexture", SkyboxTex);
    // Set the target texture.
    RTshader.SetTexture(0, "_TargetTexture", Helper.TargetTextures[0]);
    // Set the Camera to the World matrix.
    RTshader.SetMatrix("_CameraToWorld", MainCamRef.cameraToWorldMatrix);
    // Set the inversed projection matrix.
    RTshader.SetMatrix("_CameraInverseProjection", MainCamRef.projectionMatrix.inverse);

    // Set the light attributes.
    //var light_dir = DirLight.transform.forward;
    //RTshader.SetVector("_DirectionalLight",
    //                   new Vector4(light_dir.x, light_dir.y, light_dir.z, DirLight.intensity));

    // Set the sphere attributes compute buffers.                  
    RTcomputeShaderHelper.SetComputeBuffer(ref RTshader, "_Spheres", Locator.SpheresComputeBuf);
    // Set the mesh objects attributes compute buffers.
    RTcomputeShaderHelper.SetComputeBuffer(ref RTshader, "_MeshObjects", Helper.MeshObjectsAttrsComputeBuf);

    // if there's vertices, set the vertices and the indices compute buffers.
    if (Helper.VerticesList.Count > 0) {
      RTcomputeShaderHelper.SetComputeBuffer(ref RTshader, "_Vertices", Helper.VerticesComputeBuf);
      RTcomputeShaderHelper.SetComputeBuffer(ref RTshader, "_Indices", Helper.IndicesComputeBuf);
    }
    // if there's vertex color affected, set the vertex color compute buffers.
    if (Helper.VtxColorsList.Count > 0) {
      RTcomputeShaderHelper.SetComputeBuffer(ref RTshader, "_VertexColors", Helper.VtxColorsComputeBuf);
    }
    // if there's texture color affected, set the texture color compute buffers.
    if (Helper.TextureColorsList.Count > 0) {
      RTcomputeShaderHelper.SetComputeBuffer(ref RTshader, "_TextureColors", Helper.TextureColorsComputeBuf);
      RTcomputeShaderHelper.SetComputeBuffer(ref RTshader, "_UVs", Helper.UVsComputeBuf);
    }
  }

  /// <summary>
  /// Set Ray Tracing Shader Parameters at runtime (called in Update)
  /// </summary>
  void SetShaderParamsAtRuntime() {
    var timeOffset = new Vector4(Time.time * 10,
                                 Time.time * 20,
                                 Time.time * 50,
                                 Time.time * 100);
    // Set the Time parameter for moving the objects in Ray Tracer.
    RTshader.SetVector("_Time", timeOffset);
  }

  /// <summary>
  /// It's called when if some mesh objects is target to be added into the Ray Tracer.  
  /// </summary>
  void RebuildMeshObjects() {
    Helper.RebuildMeshObjects();
  }
};
