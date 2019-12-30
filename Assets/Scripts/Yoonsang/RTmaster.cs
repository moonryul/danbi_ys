using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Camera), typeof(RThelper))]
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
  uint CurrentSampleCount = 0;

  /// <summary>
  /// Resampler Material (Hidden/AddShader).
  /// </summary>
  Material ResampleAddeMat;

  /// <summary>
  /// Does Need To Rebuild RTobject to transfer the data into the RTshader?
  /// </summary>
  static bool IsNeedToRebuildRTobject = false;

  /// <summary>
  /// Ray Tracing Objects List that are transferred into the RTshader.
  /// RTobject is based on the polymorphism. (There are others inherited shapes).
  /// </summary>
  public static List<RTmeshObject> RTobjectsList = new List<RTmeshObject>();

  [Header("Light for Ray Tracing Rendering (Current -> Lambertian / Future -> BDRF PBR)"), Space(5)]
  public Light DirLight;


  RThelper Helper;

  RTdbg DbgInfo;

  void OnEnable() {
    SetupSphere();
  }

  void Awake() {
    MainCamRef = GetComponent<Camera>();
  }  

  void OnRenderImage(RenderTexture source, RenderTexture destination) {
    // Rebuild the mesh objects if new mesh objects are coming up.
    RebuildMeshObjects();
    // Set Shader parameters.
    SetShaderParams();
    // Render it.
    Render(destination);
    // Retrieve the data from the vertex color buffer.
    Helper.VtxColorsComputeBuf.GetData(DbgInfo.RetrivedColBuf);
    for (int i = 0; i < Helper.VtxColorsComputeBuf.count; ++i) {
      Debug.Log($"R: {DbgInfo.RetrivedColBuf[i].x}, "
              + $"G: {DbgInfo.RetrivedColBuf[i].y}"
              + $"B: {DbgInfo.RetrivedColBuf[i].z}");
    }
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
    if (ReferenceEquals(ResampleAddeMat, null)) {
      ResampleAddeMat = new Material(Shader.Find("Hidden/AddShader"));
      Debug.Log("add material is created!");
    }

    ResampleAddeMat.SetFloat("_Sample", CurrentSampleCount);
    Graphics.Blit(ResultRenderTex, destination);
    if (CurrentSampleCount < 100) {
      ++CurrentSampleCount;
    } else {
      return;
    }
    //Debug.Log($"{_current_sample}");
  }

  void InitRenderTexture() {
    if (ResultRenderTex == null || ResultRenderTex.width != Screen.width || ResultRenderTex.height != Screen.height) {
      // Release render texture if we already have one
      if (ResultRenderTex != null) {
        ResultRenderTex.Release();
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
    RTshader.SetVector("_PixelOffset", new Vector2(UnityEngine.Random.value, UnityEngine.Random.value));
    RTshader.SetTexture(0, "_SkyboxTexture", SkyboxTex);
    RTshader.SetMatrix("_CameraToWorld", MainCamRef.cameraToWorldMatrix);
    RTshader.SetMatrix("_CameraInverseProjection", MainCamRef.projectionMatrix.inverse);
    RTshader.SetBuffer(0, "_Spheres", Helper.SphereComputeBuf);
    var light_dir = DirLight.transform.forward;
    RTshader.SetVector("_DirectionalLight",
                              new Vector4(light_dir.x, light_dir.y, light_dir.z, DirLight.intensity));
    SetComputeBuffer("_Spheres", Helper.SphereComputeBuf);
    SetComputeBuffer("_MeshObjects", Helper.MeshObjectsComputeBuf);
    SetComputeBuffer("_Vertices", Helper.VtxComputeBuf);
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

  void SetupSphere() {
    var sphereRandomLocator = new RTobjectRandomLocator();
    var res = sphereRandomLocator.Locate();

    // Assign to compute shader.
    Helper.SphereComputeBuf = new ComputeBuffer(res.Count, 40);
    Helper.SphereComputeBuf.SetData<RTsphere>(res);
  }

  public static void register(RTmeshObject obj) {
    RTobjectsList.Add(obj);
    IsNeedToRebuildRTobject = true;
  }

  public static void unregister(RTmeshObject obj) {
    RTobjectsList.Remove(obj);
    IsNeedToRebuildRTobject = true;
  }

  void RebuildMeshObjects() {
    if (!IsNeedToRebuildRTobject) {
      return;
    }

    IsNeedToRebuildRTobject = false;
    CurrentSampleCount = 0;
    // Clear all lists.
    
    // Loop over all objects and gather their data into a single list of vertices, indices and mesh_objects.
    // vertex color is 
    foreach (var go in RTobjectsList) {
      var mesh = go.GetComponent<MeshFilter>().sharedMesh;
      // Add vertex data.
      int first_vtx = VerticesList.Count;
      // _vertices -> list<Vector4> , mesh.vertices -> Vector4[].
      // AddRange() ->
      // Adds the elements of the specified collection to the end of the '_vertices(System.Collections.Generic.List)'.
      VerticesList.AddRange(mesh.vertices);
      // Add index data - if the vertex buffer wasn't empty before, the indices need to be offset.
      int first_idx = IndicesList.Count;
      int[] indices = mesh.GetIndices(0);
      IndicesList.AddRange(indices.Select(i => i + first_vtx));
      //bool is_cube = go is ray_tracing_cube;
      bool is_cube = !ReferenceEquals(go.GetComponent<RTcube>(), null);

      // Add the object itself.
      MeshObjectsAttrsList.Add(new RTmeshObjectAttr() {
        Local2WorldMatrix = go.transform.localToWorldMatrix,
        IndicesOffset = first_idx,
        IndicesCount = indices.Length,
        // set 'true' if the element 'go' is convertible of 'ray_tracing_cube'.
        UseVtxCol = is_cube ? 1 : 0
      });

      // if element 'go' is convertible of 'ray_tracing_cube'
      // add the vertex color into the '_vertex_colors(list<Vector4>)'.
      if (is_cube) {
        // Add the vertex color.        
        VtxColsList.AddRange(mesh.colors.Select(i => new Vector4(i.r, i.g, i.b, i.a)));
      }
    }

    CreateOrSetComputeBuffer(ref MeshObjComputeBuf, MeshObjectsAttrsList, 76);
    CreateOrSetComputeBuffer(ref VtxComputeBuf, VerticesList, 12);
    CreateOrSetComputeBuffer(ref IndicesComputeBuf, IndicesList, 4);
    CreateOrSetComputeBuffer(ref VtxColComputeBuf, VtxColsList, 16);
  }

  static void CreateOrSetComputeBuffer<T>(ref ComputeBuffer buffer, List<T> data, int stride)
    where T : struct {
    // check if we already have a compute buffer.
    if (!ReferenceEquals(buffer, null)) {
      // If no data or buffer doesn't match the given condition, release it.
      if (data.Count == 0 || buffer.count != data.Count || buffer.stride != stride) {
        buffer.Release();
        buffer = null;
      }
    }

    if (data.Count != 0) {
      // If the buffer has been released or wasn't there to begin with, create it.
      if (ReferenceEquals(buffer, null)) {
        buffer = new ComputeBuffer(data.Count, stride);
      }
      // Set data on the buffer.
      buffer.SetData(data);
    }
  }

  void SetComputeBuffer(string name, ComputeBuffer buffer) {
    if (!ReferenceEquals(buffer, null)) {
      RTshader.SetBuffer(0, name, buffer);
    }
  }

};
