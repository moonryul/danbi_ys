using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ProceduralCylinder))]

public class ProceduralCylinderEditor : Editor {
  [MenuItem("GameObject/Create Other/Procedural/Cylinder")]

  static void Create() {
    var gameObject = new GameObject("ProceduralCylinder");
    var cylinder = gameObject.AddComponent<ProceduralCylinder>();
    var meshFilter = gameObject.GetComponent<MeshFilter>();
    meshFilter.mesh = new Mesh();
    Debug.Assert(!cylinder.GetComponent<ProceduralCylinder>().Null(),
                 $"Procedurally Creating Cylinder failed!");
    cylinder.Rebuild();
    cylinder.AssignDefaultShader();
  }

  public override void OnInspectorGUI() {
    var cylinder = target as ProceduralCylinder;
    //if (obj == null) {
    //  return;
    //}
    Debug.Assert(!cylinder.Null(),
                 $"Target cannot be casted to the ProceduralCylinder!");

    base.DrawDefaultInspector();
    if (GUI.changed) {
      cylinder.Rebuild();
    }
  }
}
