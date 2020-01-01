using UnityEngine;

/// <summary>
/// The mesh object for ray tracing. every mesh object for the ray tracing shader must inherit this class.
/// </summary>
[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
public class RTmeshObject : MonoBehaviour {
  /// <summary>
  /// OnEnable(), all the references of this gameObject is registered into the RTmeshObjectsList
  /// To rebuild every mesh objects!
  /// </summary>
  public virtual void OnEnable() {
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
};
