using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class RTcomputeShaderHelper : MonoBehaviour {
  /// <summary>
  /// Does need to rebuild RTobject to transfer the data into the RTshader?
  /// </summary>
  public static bool DoesNeedToRebuildRTobjects { get; set; } = false;
  /// <summary>
  /// Ray tracing objects list that are transferred into the RTshader.
  /// RTobject is based on the polymorphism (There're others inherited shapes).
  /// </summary>
  public static List<RTmeshObject> MeshObjectsList { get; set; } = new List<RTmeshObject>();
  /// <summary>
  /// 
  /// </summary>
  public List<RTmeshObjectAttr> MeshObjectsAttrsList = new List<RTmeshObjectAttr>();
  /// <summary>
  /// 
  /// </summary>
  public ComputeBuffer MeshObjectsAttrsComputeBuf;
  /// <summary>
  /// 
  /// </summary>
  public List<Vector3> VerticesList = new List<Vector3>();
  /// <summary>
  /// 
  /// </summary>
  public ComputeBuffer VerticesComputeBuf;
  /// <summary>
  /// 
  /// </summary>
  public List<int> IndicesList = new List<int>();
  /// <summary>
  /// 
  /// </summary>
  public ComputeBuffer IndicesComputeBuf;
  /// <summary>
  /// 
  /// </summary>
  public List<Vector3> VtxColorsList = new List<Vector3>();
  /// <summary>
  /// 
  /// </summary>
  public ComputeBuffer VtxColorsComputeBuf;
  /// <summary>
  /// 
  /// </summary>
  public List<Vector3> TextureColorsList = new List<Vector3>();
  /// <summary>
  /// 
  /// </summary>
  public ComputeBuffer TextureColorsComputeBuf;
  /// <summary>
  /// 
  /// </summary>
  public List<Vector2> UVsList = new List<Vector2>();
  /// <summary>
  /// 
  /// </summary>
  public ComputeBuffer UVsComputeBuf;
  /// <summary>
  ///  
  /// </summary>
  public Texture2D[] TargetTextures;
  /// <summary>
  /// 
  /// </summary>
  Texture2D ResultTexture;
  /// <summary>
  /// 
  /// </summary>
  public RenderTexture TempRenderTexture;


  void Start() {
    RTdbg.DbgStopwatch.Start();
    RTdbg.DbgStopwatch.Stop();
    Debug.Log($"Elapsed time of Decomposing the texture into the pixel : {RTdbg.DbgStopwatch.ElapsedMilliseconds} ms", this);

    Assert.IsNotNull(TempRenderTexture, "TempRenderTexture is null!");
    RenderTexture.active = TempRenderTexture;
  }

  void OnDisable() {
    // Check each compute buffers are still valid and release it!    
    DisposeComputeBuffers(ref MeshObjectsAttrsComputeBuf);
    DisposeComputeBuffers(ref VtxColorsComputeBuf);
    DisposeComputeBuffers(ref VtxColorsComputeBuf);
    DisposeComputeBuffers(ref TextureColorsComputeBuf);
    DisposeComputeBuffers(ref UVsComputeBuf);
  }

  /// <summary>
  /// Check the compute buffer are still valid and release it!    
  /// </summary>
  /// <param name="buf"></param>
  void DisposeComputeBuffers(ref ComputeBuffer buf) {
    if (!buf.Null()) {
      buf.Release();
      buf = null;
    }
  }

  /// <summary>
  /// 
  /// </summary>
  /// <param name="obj"></param>
  public static void RegisterToRTobject(RTmeshObject obj) {
    MeshObjectsList.Add(obj);
    Debug.Log($"obj <{obj.name}> is added into the RT object list.");
    DoesNeedToRebuildRTobjects = true;
  }

  /// <summary>
  /// 
  /// </summary>
  /// <param name="obj"></param>
  public static void UnregisterToRTobject(RTmeshObject obj) {
    if (MeshObjectsList.Contains(obj)) {
      MeshObjectsList.Remove(obj);
      Debug.Log($"obj <{obj.name}> is removed from the RT object list.");
      DoesNeedToRebuildRTobjects = true;
      return;
    }
    Debug.LogError($"obj <{obj.name}> isn't contained in the RT object list.");
  }

  /// <summary>
  /// Rebuild the entire list that is going to be transferred into the Compute Shader.
  /// (currently : Vertices, Indices, VertexColors, TextureColors).
  /// </summary>
  public void RebuildMeshObjects() {
    if (!DoesNeedToRebuildRTobjects) {
      return;
    }

    DoesNeedToRebuildRTobjects = false;
    // Clear all lists.
    MeshObjectsAttrsList.Clear();
    VerticesList.Clear();
    IndicesList.Clear();
    VtxColorsList.Clear();
    bool DoesNeedToDecomposeTexture = false;

    // Loop over all objects and gather their data into a single list of the vertices,
    // the indices and the mesh objects.
    foreach (var go in MeshObjectsList) {
      // forward the mesh.
      var mesh = go.GetComponent<MeshFilter>().sharedMesh;

      // 1. Vertices.
      // add vertices data first.
      int verticesStride = VerticesList.Count;
      // AddRange() -> Adds the elements of the specified collection to the end of the 'VerticesList'
      VerticesList.AddRange(mesh.vertices);

      // 2. Indices.
      // Add index data - if the vertex compute buffer wasn't empty before,
      // the indices need to push some offsets.
      int indicesStride = IndicesList.Count;
      int[] indices = mesh.GetIndices(0);
      IndicesList.AddRange(indices.Select(e => e + verticesStride));

      // 3. UVs.
      // Add uv data
      int uvStride = UVsList.Count;
      var forwarded = new List<Vector2>();
      mesh.GetUVs(0, forwarded);
      UVsList.AddRange(forwarded);


      // If the element(go) is convertible of 'RTmeshCube' then we need to add more info
      // about the vertices colors.      

      var rtObject = go.GetComponent<RTmeshObject>();

      // Add the mesh object attributes.
      MeshObjectsAttrsList.Add(new RTmeshObjectAttr() {
        Local2WorldMatrix = go.transform.localToWorldMatrix,
        IndicesOffset = indicesStride,
        IndicesCount = indices.Length,
        colorMode = (int)rtObject.ColorMode
      });

      switch (rtObject.ColorMode) {
        case eColorMode.NONE:
        // Nothing to do!
        break;

        case eColorMode.TEXTURE:
        DoesNeedToDecomposeTexture = true;
        break;

        case eColorMode.VERTEX_COLOR:
        //VtxColorsList.AddRange(mesh.colors.Select(e => new Vector3(e.r, e.g, e.b)));
        break;
      }
    }

    if (DoesNeedToDecomposeTexture) {
      DecomposeTextureIntoPixels(ref TargetTextures);
    }
    CreateOrBindDataToComputeBuffer(ref MeshObjectsAttrsComputeBuf, MeshObjectsAttrsList, 80); //
    CreateOrBindDataToComputeBuffer(ref VerticesComputeBuf, VerticesList, 12); // float3
    CreateOrBindDataToComputeBuffer(ref IndicesComputeBuf, IndicesList, 4); // int

    //if (VtxColorsList.Count >= 0) {
    //  CreateOrBindDataToComputeBuffer(ref VtxColorsComputeBuf, VtxColorsList, 12); // float3
    //}

    if (TextureColorsList.Count >= 0) {
      CreateOrBindDataToComputeBuffer(ref TextureColorsComputeBuf, TextureColorsList, 12); // float3
      CreateOrBindDataToComputeBuffer(ref UVsComputeBuf, UVsList, 8);
    }
  }

  /// <summary>
  /// 
  /// </summary>
  /// <typeparam name="T"></typeparam>
  /// <param name="buffer"></param>
  /// <param name="data"></param>
  /// <param name="stride"></param>
  void CreateOrBindDataToComputeBuffer<T>(ref ComputeBuffer buffer,
                                   List<T> data, int stride) where T : struct {
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

  /// <summary>
  /// 
  /// </summary>
  /// <param name="shader"></param>
  /// <param name="name"></param>
  /// <param name="buffer"></param>
  public static void SetComputeBuffer(ref ComputeShader shader, string name, ComputeBuffer buffer) {
    if (buffer.Null()) {
      return;
    }

    shader.SetBuffer(0, name, buffer);
  }

  /// <summary>
  /// 
  /// </summary>
  /// <param name="tex"></param>
  /// <returns></returns>
  public void DecomposeTextureIntoPixels(ref Texture2D[] targetTextures) {
    Assert.IsNotNull(targetTextures, "Target texture cannot be null!");
    Assert.IsFalse(targetTextures.Length == 0, "Target textures count cannot be zero!");
    // Retrieve the dimensions from the target texture.
    for (int a = 0; a < targetTextures.Length; ++a) {
      var dimensions = (x: targetTextures[a].width, y: targetTextures[a].height);
      var colArr = new Vector3[dimensions.x, dimensions.y];
      var resCol = new Color[dimensions.x, dimensions.y];
      int stride = 0;
      ResultTexture = new Texture2D(dimensions.x, dimensions.y);
      // read the texture array vertically->horizontally
      for (int i = 0; i < dimensions.y; ++i) {
        for (int j = 0; j < dimensions.x; ++j, ++stride) {
          // Forward the pixel into variable.
          var pixel = targetTextures[a].GetPixel(j, i);
          // Add the colors values and the stride into the result.
          //result.Add((
          //  new Vector3(pixel.r, pixel.g, pixel.b),
          //  stride));
          TextureColorsList.Add(new Vector3(pixel.r, pixel.g, pixel.b));
          resCol[j, i] = pixel;
          ResultTexture.SetPixel(j, i, pixel);
        }
      }
    }

    // Apply the render texture.
    ResultTexture.Apply();
    Graphics.Blit(ResultTexture, TempRenderTexture);
  }
};
