using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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

  public void OnDisable() {
    // Check each compute buffers are still valid and release it!
    if (!MeshObjectsAttrsComputeBuf.Null()) {
      MeshObjectsAttrsComputeBuf.Release();
      MeshObjectsAttrsComputeBuf = null;
    }

    if (!VtxColorsComputeBuf.Null()) {
      VtxColorsComputeBuf.Release();
      VtxColorsComputeBuf = null;
    }

    if (!VtxColorsComputeBuf.Null()) {
      VtxColorsComputeBuf.Release();
      VtxColorsComputeBuf = null;
    }
  }

  public static void RegisterToRTobject(RTmeshObject obj) {
    MeshObjectsList.Add(obj);
    Debug.Log($"obj <{obj.name}> is added into the RT object list.");
    DoesNeedToRebuildRTobjects = true;
  }

  public static void UnregisterToRTobject(RTmeshObject obj) {
    if (MeshObjectsList.Contains(obj)) {
      MeshObjectsList.Remove(obj);
      Debug.Log($"obj <{obj.name}> is removed from the RT object list.");
      DoesNeedToRebuildRTobjects = true;
      return;
    }
    Debug.LogError($"obj <{obj.name}> isn't contained in the RT object list.");
  }
  
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

    // Loop over all objects and gather their data into a single list of the vertices,
    // the indices and the mesh objects.
    foreach (var go in MeshObjectsList) {
      // forward the mesh.
      var mesh = go.GetComponent<MeshFilter>().sharedMesh;

      // add vertices data first.
      int verticesStride = VerticesList.Count;
      // AddRange() -> Adds the elements of the specified collection to the end of the 'VerticesList'
      VerticesList.AddRange(mesh.vertices);

      // Add index data - if the vertex compute buffer wasn't empty before,
      // the indices need to push some offsets.
      int indicesStride = IndicesList.Count;
      int[] indices = mesh.GetIndices(0);
      IndicesList.AddRange(indices.Select(e => e + verticesStride));

      // If the element(go) is convertible of 'RTmeshCube' then we need to add more info
      // about the vertices colors.
      bool is_cube = !go.GetComponent<RTmeshCube>().Null();

      // Add the mesh object attributes.
      MeshObjectsAttrsList.Add(new RTmeshObjectAttr() {
        Local2WorldMatrix = go.transform.localToWorldMatrix,
        IndicesOffset = indicesStride,
        IndicesCount = indices.Length,
        UseVtxCol = is_cube ? 1 : 0
      });

      if (is_cube) {
        VtxColorsList.AddRange(mesh.colors.Select(e => new Vector3(e.r, e.g, e.b)));
      }
    }

    CreateOrBindDataToComputeBuffer(ref MeshObjectsAttrsComputeBuf, MeshObjectsAttrsList, 80); //
    CreateOrBindDataToComputeBuffer(ref VerticesComputeBuf, VerticesList, 12); // float3
    CreateOrBindDataToComputeBuffer(ref IndicesComputeBuf, IndicesList, 4); // int
    CreateOrBindDataToComputeBuffer(ref VtxColorsComputeBuf, VtxColorsList, 12); // float3
  }

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

  public static void SetComputeBuffer(ref ComputeShader shader, string name, ComputeBuffer buffer) {
    if (buffer.Null()) {
      return;
    }

    shader.SetBuffer(0, name, buffer);
  }
};
