using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum eColorMode {
  NONE = 0,
  TEXTURE = 1,
  VERTEX_COLOR = 2
};


/// <summary>
/// The mesh object for ray tracing. every mesh object for the ray tracing shader must inherit this class.
/// </summary>
[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
public class RTmeshObject : MonoBehaviour {
  public eColorMode ColorMode;
  //public bool UV_XdirectionInversed;
  //public bool UV_YdirectionInversed;
  //public bool UV_XYdirectionInversed;
  /// <summary>
  /// OnEnable(), all the references of this gameObject is registered into the RTmeshObjectsList
  /// To rebuild every mesh objects!
  /// </summary>
  public virtual void OnEnable() {
    //InverseUVdirection();
    RTcomputeShaderHelper.RegisterToRTobject(this);
    RTcomputeShaderHelper.DoesNeedToRebuildRTobjects = true;
  }
  /// <summary>
  /// OnDisable(), all the references inside the RTmeshObjectsList is removed.
  /// </summary>
  public virtual void OnDisable() {
    RTcomputeShaderHelper.UnregisterToRTobject(this);
    RTcomputeShaderHelper.DoesNeedToRebuildRTobjects = true;
  }


  //void InverseUVdirection() {
  //  // UV inversed
  //  if (UV_XdirectionInversed) {
  //    var mesh = GetComponent<MeshFilter>().sharedMesh;
  //    var temp = new List<Vector2>();
  //    mesh.GetUVs(0, temp);
  //    for (int i = 0; i < temp.Count; ++i) {
  //      temp[i] = new Vector2(-temp[i].x, temp[i].y);
  //    }
  //    mesh.SetUVs(0, temp);
  //  }

  //  if (UV_YdirectionInversed) {
  //    var mesh = GetComponent<MeshFilter>().sharedMesh;
  //    var temp = new List<Vector2>();
  //    mesh.GetUVs(0, temp);
  //    for (int i = 0; i < temp.Count; ++i) {
  //      temp[i] = new Vector2(temp[i].x, -temp[i].y);
  //    }
  //    mesh.SetUVs(0, temp);
  //  }

  //  if (UV_XYdirectionInversed) {
  //    var mesh = GetComponent<MeshFilter>().sharedMesh;
  //    var temp = new List<Vector2>();
  //    mesh.GetUVs(0, temp);
  //    for (int i = 0; i < temp.Count; ++i) {
  //      temp[i] *= -1;
  //    }
  //    mesh.SetUVs(0, temp);
  //  }
  //}
};
