using System.Collections.Generic;
using UnityEngine;

public class RThelper : MonoBehaviour {
  public static bool DoesNeedToRebuildRTobjects { get; set; } = false;
  public static List<RTmeshObject> RTobjectsList { get; set; } = new List<RTmeshObject>();

  [Header("Offset Radius for Sphere ")]
  public Vector2 OffsetRadius = new Vector2(3.0f, 8.0f);

  public int SphereMaxCount = 20;
  public float SpehrePlacementRadius = 100.0f;
  public ComputeBuffer SphereComputeBuf;


  public List<RTmeshObjectAttr> MeshObjectsAttrsList = new List<RTmeshObjectAttr>();
  public ComputeBuffer MeshObjectsComputeBuf;

  public List<Vector3> VerticesList = new List<Vector3>();
  public ComputeBuffer VtxComputeBuf;

  public List<int> IndicesList = new List<int>();
  public ComputeBuffer IndicesComputeBuf;

  public List<Vector3> VtxColorsList = new List<Vector3>();
  public ComputeBuffer VtxColorsComputeBuf;

  public void OnDisable() {
    if (SphereComputeBuf != null) {
      SphereComputeBuf.Release();
      SphereComputeBuf = null;
    }

    if (MeshObjectsComputeBuf != null) {
      MeshObjectsComputeBuf.Release();
      MeshObjectsComputeBuf = null;
    }

    if (VtxColorsComputeBuf != null) {
      VtxColorsComputeBuf.Release();
      VtxColorsComputeBuf = null;
    }

    if (VtxColorsComputeBuf != null) {
      VtxColorsComputeBuf.Release();
      VtxColorsComputeBuf = null;
    }
  }

  public static void RegisterToRTobject(RTmeshObject obj) {
    RTobjectsList.Add(obj);
    Debug.Log($"obj <{obj.name}> is added into the RT object list.");
    DoesNeedToRebuildRTobjects = true;
  }

  public static void UnregisterToRTobject(RTmeshObject obj) {
    if (RTobjectsList.Contains(obj)) {
      RTobjectsList.Remove(obj);
      Debug.Log($"obj <{obj.name}> is removed from the RT object list.");
      DoesNeedToRebuildRTobjects = true;
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
  }
};
