using UnityEngine;

/// <summary>
/// 
/// </summary>
[System.Serializable]
public enum eCurrentVertexColor {
  RED, GREEN, BLUE, MAGENTA
};
/// <summary>
/// 
/// </summary>
public class RTmeshCube : RTmeshObject {
  public eCurrentVertexColor VtxColor;
  Mesh ThisMesh;

  public override void OnEnable() {
    ThisMesh = GetComponent<MeshFilter>().sharedMesh;
    SetVertexColor();
    base.OnEnable();
  }
  public override void OnDisable() { base.OnDisable(); }

  void SetVertexColor() {
    int len = ThisMesh.vertices.Length;
    var col = new Color[len];
    switch (VtxColor) {
      case eCurrentVertexColor.RED:
      for (int i = 0; i < len; ++i) {
        col[i] = Color.red;
      }
      break;

      case eCurrentVertexColor.GREEN:
      for (int i = 0; i < len; ++i) {
        col[i] = Color.green;
      }
      break;

      case eCurrentVertexColor.BLUE:
      for (int i = 0; i < len; ++i) {
        col[i] = Color.blue;
      }
      break;

      case eCurrentVertexColor.MAGENTA:
      for (int i = 0; i < len; ++i) {
        col[i] = Color.magenta;
      }
      break;

      default:
      return;
    }
    ThisMesh.colors = col;
  }
}
